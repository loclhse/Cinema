using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Helper
{
    public class SeedData
    {
        public static async Task SeedRolesAsync(RoleManager<AppRole> roleManager)
        {
            var roles = new[] { "Admin", "Staff", "Member", "Guest" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new AppRole(role));
                }
            }
        }
    }
}
