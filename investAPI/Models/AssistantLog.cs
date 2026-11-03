using System;
using System.Collections.Generic;

namespace investAPI.Models;

public partial class AssistantLog
{
    public long Id { get; set; }

    public int? UserId { get; set; }

    public string? RunLabel { get; set; }

    public DateTime Timestamp { get; set; }

    public string UserMessage { get; set; } = null!;

    public string? ToolName { get; set; }

    public string? ToolArgsJson { get; set; }

    public string? ToolResultJson { get; set; }

    public bool Success { get; set; }

    public int? LatencyMs { get; set; }

    public int? InputTokens { get; set; }

    public int? OutputTokens { get; set; }

    public virtual User? User { get; set; }
}
