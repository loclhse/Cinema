using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class SnackComboItem:BaseEntity 
{
   

    public Guid? ComboId { get; set; }

    public Guid? SnackId { get; set; }

    public int? Quantity { get; set; }

    public virtual SnackCombo? Combo { get; set; }

    public virtual Snack? Snack { get; set; }
}
