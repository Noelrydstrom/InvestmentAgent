using System;
using System.Collections.Generic;

namespace investAPI.Models;

public partial class WatchlistItem
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int SecurityId { get; set; }

    public string? Notes { get; set; }

    public virtual Security Security { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
