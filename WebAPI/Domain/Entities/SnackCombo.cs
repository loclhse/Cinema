using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;

public partial class SnackCombo : BaseEntity
{
   

    public string? Name { get; set; }

    public string? ImgUrl { get; set; }

    public string? Description { get; set; }

    public decimal TotalPrice { get; set; }

    public decimal? discount { get; set; }

    public SnackStatus SnackComboStatus { get; set; }

    public virtual ICollection<SnackComboItem> SnackComboItems { get; set; } = new List<SnackComboItem>();
}
