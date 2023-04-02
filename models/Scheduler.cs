using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FocusAI.Backend.Models;
using Google.OrTools.Sat;

namespace FocusAI.Backend.Models;
class Scheduler {
    public Block[] blocks;
    public Task[] tasks;
    public DateTime slot_start;
    public int valid_st;
    public int valid_et;


    public int n_slots;
    public int n_tasks;

    public Scheduler(Block[] blocks, Task[] tasks, int valid_st = 8, int valid_et = 22) {
        this.blocks = blocks;
        this.tasks = tasks;
        this.valid_st = valid_st;
        this.valid_et = valid_et;
        DateTime cdt = DateTime.Now;
        int final_minute = 0;
        int final_hour = cdt.Hour;
        if (cdt.Minute > 0 && cdt.Minute < 30) {
            final_minute = 30;
        } else if (cdt.Minute > 30) {
            final_hour++;
        }
        DateTime slot_start = new DateTime(cdt.Year, cdt.Month, cdt.Day, final_hour, final_minute, 0);
        this.slot_start = slot_start;
        this.n_slots = 192; // four days
        this.n_tasks = this.tasks.Length;
    }

    public bool isBlocked(int s) {
        DateTime target = this.slot_start.AddMinutes(s*30);
        foreach (Block b in this.blocks) {
            if (target >= b.Sdt && target <= b.Edt) {
                return true;
            }
            if (target.Hour < this.valid_st || target.Hour > this.valid_et) {
                // I sleep during this time
                return true;
            }
        }
        return false;
    }

    public List<Block> Schedule() {
        int[] all_tasks = Enumerable.Range(0, n_tasks).ToArray();
        int[] all_slots = Enumerable.Range(0, n_slots).ToArray();

        CpModel model = new CpModel();
        model.Model.Variables.Capacity = n_slots * n_tasks;

        Dictionary<(int, int), BoolVar> ts_assignments = new Dictionary<(int, int), BoolVar>(n_tasks * n_slots);
        // ts_assignment[0][0] is true iff task 0 is scheduled on slot 0
        foreach (int t in all_tasks)
        {
            foreach (int s in all_slots)
            {
                ts_assignments.Add((t, s), model.NewBoolVar($"assignment_t{t}s{s}"));
            }
        }


        // constraint: each slot is assigned to at most one task
        List<ILiteral> literals = new List<ILiteral>();
        foreach (int s in all_slots)
        {
            foreach (int t in all_tasks)
            {
                literals.Add(ts_assignments[(t, s)]);
            }
            model.AddAtMostOne(literals);
            literals.Clear();
        }

        // constraint: a blocked slot should never be scheduled
        List<IntVar> forbiddenSlots = new List<IntVar>();
        foreach (int s in all_slots) {
            // check if s is blocked
            if (isBlocked(s)) {
                foreach (int t in all_tasks) {
                    forbiddenSlots.Add(ts_assignments[(t,s)]);
                    Console.WriteLine($"Slot {s} is marked as forbidden");
                }
            }
        }
        model.AddLinearConstraint(LinearExpr.Sum(forbiddenSlots), 0, 0);
        forbiddenSlots.Clear();

        // constraint: there shouldn't be more than 6 consecutive slots scheduled to same task
        int max_consecutive_slots = 6;
        List<IntVar> consecutive_slots_assignment = new List<IntVar>();
        foreach (int t in all_tasks)
        {
            foreach (int s in all_slots)
            {
                if (s >= (max_consecutive_slots - 1))
                {
                    for (int x = s - (max_consecutive_slots - 1); x < max_consecutive_slots; x++)
                    {
                        consecutive_slots_assignment.Add(ts_assignments[(t, all_slots[x])]);
                    }
                }
            }
            model.AddLinearConstraint(LinearExpr.Sum(consecutive_slots_assignment), 0, max_consecutive_slots);
            consecutive_slots_assignment.Clear();
        }

        List<IntVar> slot_assignments = new List<IntVar>();
        foreach (int t in all_tasks)
        {
            foreach (int s in all_slots)
            {
                slot_assignments.Add(ts_assignments[(t, s)]);
            }
            model.AddLinearConstraint(LinearExpr.Sum(slot_assignments), this.tasks[t].RemainingEffortMins/30, this.tasks[t].RemainingEffortMins / 30);
            slot_assignments.Clear();

        }

        // optimize to finish all tasks as early as possible
        List<int> slot_assignment_weights = new List<int>();
        foreach (int t in all_tasks)
        {
            foreach (int s in all_slots)
            {
                slot_assignments.Add(ts_assignments[(t, s)]);
                slot_assignment_weights.Add(s);
            }

        }
        model.Minimize(LinearExpr.WeightedSum(slot_assignments, slot_assignment_weights));

        CpSolver solver = new CpSolver();
        CpSolverStatus status = solver.Solve(model);
        Console.WriteLine($"Solve status: {status}");
        Block current_block = new Block();
        current_block.Uuid = "";
        current_block.TaskId = "";
        List<Block> new_blocks = new List<Block>();
        if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible) {
            Console.WriteLine("Solution:");
            foreach(int t in all_tasks) {
                foreach (int s in all_slots) {
                    if (solver.Value(ts_assignments[(t, s)]) == 1L) {
                        Console.WriteLine($"Task {t} scheduled on slot {s}");
                        if ((string.IsNullOrEmpty(current_block.Uuid) && string.IsNullOrEmpty(current_block.TaskId)) || current_block.TaskId != this.tasks[t].TaskId) {
                            if (current_block.TaskId != this.tasks[t].TaskId) {
                                // add to list
                                if (!string.IsNullOrEmpty(current_block.Uuid)) {
                                    new_blocks.Add(current_block);
                                }
                                // new task
                                current_block = new Block();
                            }
                            // first time
                            current_block.Uuid = "focus-ai-" + Guid.NewGuid().ToString();
                            current_block.TaskId = this.tasks[t].TaskId;
                            current_block.Title = this.tasks[t].Title;
                            current_block.Sdt = this.slot_start.AddMinutes(s*30);
                            current_block.Edt = this.slot_start.AddMinutes(s*30 + 30);
                        } else if (current_block.TaskId == this.tasks[t].TaskId) {
                            // extend
                            current_block.Edt = current_block.Edt.AddMinutes(30);
                        }

                        if (current_block.Edt > this.tasks[t].Deadline) {
                            throw new Exception("If you proceed with this, you will not be able to finish " + this.tasks[t].Title + " on time. ");
                        }
                    }
                }
            }
            // last block  add to list
            if (!string.IsNullOrEmpty(current_block.Uuid)) {
                new_blocks.Add(current_block);
            }
        } else {
            throw new Cloops.Exceptions.InvalidParameterException("Not able to schedule the tasks. Not enough time!");
        }

        return new_blocks;

    }
}