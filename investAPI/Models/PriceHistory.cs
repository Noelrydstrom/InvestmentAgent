using System;
using System.Collections.Generic;

namespace investAPI.Models;

public partial class PriceHistory
{
    public int Id { get; set; }

    public int SecurityId { get; set; }

    public DateOnly PriceDate { get; set; }

    public decimal ClosePrice { get; set; }

    public virtual Security Security { get; set; } = null!;
}
