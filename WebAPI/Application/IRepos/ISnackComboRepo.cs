using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IRepos
{
    public interface ISnackComboRepo: IGenericRepo<SnackCombo>
    {
        Task<IEnumerable<SnackCombo>> GetCombosWithSnacksAsync();
        Task<SnackCombo> GetComboWithItemsAsync(Guid id);
    }
}
