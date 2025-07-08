using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;

public partial class Snack : BaseEntity
{
    public string? Name { get; set; }

    public SnackType? Type { get; set; }

    public string? imgUrl { get; set; }
    
    public int quantity { get; set; }

    public decimal Price { get; set; }

    public decimal? discount { get; set; }

    public string? Description { get; set; }

    public SnackStatus SnackStatus { get; set; }

    public virtual ICollection<SnackComboItem> SnackComboItems { get; set; } = new List<SnackComboItem>();


}
