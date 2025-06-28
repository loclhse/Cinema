using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IRepos
{
    public interface IPaymentRepo : IGenericRepo<Payment>
    {
        // Only add custom methods here if they can't be achieved with GenericRepo functions
        // For now, we can use GenericRepo's GetAllAsync, GetByIdAsync, UpdateAsync, etc. directly
    }
} 