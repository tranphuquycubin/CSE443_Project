using EbookApp.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace EbookApp.Data
{
    public class DbSeeder
    {
        public static async Task SeedDefaultData(IServiceProvider service)
        {
            var userManager = service.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = service.GetRequiredService<RoleManager<IdentityRole>>();

            // Check if roleManager is null
            if (roleManager == null)
            {
                throw new Exception("RoleManager is not available");
            }

            // Check if userManager is null
            if (userManager == null)
            {
                throw new Exception("UserManager is not available");
            }

            // Adding roles to Db
            if (!await roleManager.RoleExistsAsync(Roles.Admin.ToString()))
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
            }

            if (!await roleManager.RoleExistsAsync(Roles.User.ToString()))
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.User.ToString()));
            }

            // Create admin user
            var admin = new IdentityUser
            {
                UserName = "admin@gmail.com",
                Email = "admin@gmail.com",
                EmailConfirmed = true,
            };

            var userInDb = await userManager.FindByEmailAsync(admin.Email);
            if (userInDb == null)
            {
                var result = await userManager.CreateAsync(admin, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, Roles.Admin.ToString());
                }
                else
                {
                    throw new Exception("Failed to create admin user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}
