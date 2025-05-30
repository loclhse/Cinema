using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class SnackCombo: BaseEntity
{
    public string? Name { get; set; }

    public decimal? Price { get; set; }

    public virtual ICollection<SnackComboItem> SnackComboItems { get; set; } = new List<SnackComboItem>();
}
