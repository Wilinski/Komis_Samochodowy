using Komis.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace Komis.Data
{
    public class Seeder : ISeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public Seeder(ApplicationDbContext context, RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task Seed()
        {
            if (await _context.Database.CanConnectAsync() && await _context.Roles.CountAsync() <= 1)
            {
                var data = new List<IdentityRole>()
                {
                    new IdentityRole()
                    {
                        Name = "Admin",
                        NormalizedName = "ADMIN",
                        ConcurrencyStamp = Guid.NewGuid().ToString(),
                        Id = Guid.NewGuid().ToString()
                    },
                    new IdentityRole("User")
                };

                await _context.AddRangeAsync(data);
                await _context.SaveChangesAsync();
            }

            if (await _context.Database.CanConnectAsync() && !_context.Users.Any())
            {
                var user = new IdentityUser()
                {
                    UserName = "admin@test.pl",
                    NormalizedUserName = "ADMIN@TEST.PL",
                    Email = "admin@test.pl",
                    NormalizedEmail = "ADMIN@TEST.PL",
                    EmailConfirmed = true,
                    LockoutEnabled = true,
                    Id = Guid.NewGuid().ToString()
                };

                user.PasswordHash = new PasswordHasher<IdentityUser>()
                    .HashPassword(user, "Admin12!");

                await _context.AddAsync(user);
                await _context.SaveChangesAsync();
                await _userManager.AddToRoleAsync(user, "Admin");
                await _context.SaveChangesAsync();
            }
        }
    }

    public interface ISeeder
    {
        Task Seed();
    }

    public static class SeederExtension
    {
        public static async Task ExecuteSeeds(this IServiceCollection serviceCollection)
        {
            using var scope = serviceCollection.BuildServiceProvider().CreateScope();

            var seeder = scope.ServiceProvider
                .GetRequiredService<ISeeder>();

            await seeder.Seed();
        }
    }
}
