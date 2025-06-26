using Application.IRepos;
using Domain.Entities;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repos
{
    public class SnackRepo : GenericRepo<Snack>, ISnackRepo
    {
        public SnackRepo(AppDbContext context) : base(context)
        {
        }
       
    }
    
    }

