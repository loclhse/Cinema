using Application.IRepos;
using Domain.Entities;
using Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repos
{
    public class ScoreItemRepo : GenericRepo<ScoreItem>, IScoreItemRepo
    {
        public ScoreItemRepo(AppDbContext context) : base(context)
        {
        }
       
    }
}
