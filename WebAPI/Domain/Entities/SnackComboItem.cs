using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class SnackComboItem :BaseEntity
{
    public int? Quantity { get; set; }

    public int? ComboId { get; set; }
    public virtual SnackCombo? Combo { get; set; }

    public int? SnackId { get; set; }
    public virtual Snack? Snack { get; set; }
}
