using FocusAI.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Cloops.Exceptions;
using Newtonsoft.Json.Linq;

namespace FocusAI.Backend.Controllers;

[ApiController]
public class SchedulingController : ControllerBase
{
    const string base_route = "/api";

    [HttpPost]
    [Route(base_route + "/reschedule")]
    public JObject Reschedule([FromBody] SchedulingRequest sr) {
        JObject retval = new JObject();
        Scheduler s = new Scheduler(sr.Blocks.ToArray(), sr.Tasks.ToArray());
        s.Schedule();
        retval["status"] = "success";
        return retval;
    }
}
