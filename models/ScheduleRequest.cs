using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FocusAI.Backend.Models;
public partial class SchedulingRequest
{
    [JsonProperty("blocks")]
    public List<Block> Blocks { get; set; }

    [JsonProperty("tasks")]
    public List<Task> Tasks { get; set; }

    [JsonProperty("valid_st")]
    public int valid_st { get; set; } = 8;

    [JsonProperty("valid_et")]
    public int valid_et { get; set; } = 22;
}

public partial class Block
{
    [JsonProperty("uuid")]
    public string Uuid { get; set; }

    [JsonProperty("task_id")]
    public string TaskId { get; set; }
    
    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("sdt")]
    public DateTime Sdt { get; set; }

    [JsonProperty("edt")]
    public DateTime Edt { get; set; }

    
}


public partial class Task
{
    [JsonProperty("task_id")]
    public string TaskId { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("total_effort_mins")]
    public long TotalEffortMins { get; set; }

    [JsonProperty("deadline")]
    public DateTime Deadline { get; set; }

    [JsonProperty("remaining_effort_mins")]
    public long RemainingEffortMins { get; set; }
    
}

public partial class SchedulingRequest
{
    public static SchedulingRequest FromJson(string json) => JsonConvert.DeserializeObject<SchedulingRequest>(json, FocusAI.Backend.Models.Converter.Settings);
}

public static class Serialize
{
    public static string ToJson(this SchedulingRequest self) => JsonConvert.SerializeObject(self, FocusAI.Backend.Models.Converter.Settings);
}

internal static class Converter
{
    public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    {
        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        DateFormatString = "dd-MM-yyyy HH:mm:ss"
    };
}