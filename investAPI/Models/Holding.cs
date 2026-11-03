using System;
using System.Collections.Generic;

namespace investAPI.Models;

public partial class Holding
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int SecurityId { get; set; }

    public int Shares { get; set; }

    public decimal AvgCostPrice { get; set; }

    public DateOnly PurchaseDate { get; set; }

    public virtual Security Security { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
