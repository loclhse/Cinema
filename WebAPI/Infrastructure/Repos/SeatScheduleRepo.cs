using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.IRepos;
using Domain.Entities;
using Infrastructure.Repositories;

namespace Infrastructure.Repos
{
    public class SeatScheduleRepo : GenericRepo<SeatSchedule>, ISeatScheduleRepo
    {
        public SeatScheduleRepo(AppDbContext context) : base(context)
        {
        }
    }
}
