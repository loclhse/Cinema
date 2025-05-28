using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class Snack
{
    public int SnackId { get; set; }

    public string? Name { get; set; }

    public decimal? Price { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<SnackComboItem> SnackComboItems { get; set; } = new List<SnackComboItem>();
}
