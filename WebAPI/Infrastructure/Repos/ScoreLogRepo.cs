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
    public class ScoreLogRepo : GenericRepo<ScoreLog>, IScoreLogRepo
    {
        public ScoreLogRepo(AppDbContext context) : base(context)
        {
        }

    }
}