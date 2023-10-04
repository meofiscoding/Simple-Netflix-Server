using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MongoConnector.Models;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace MongoConnector.Seeder
{
    public static class SeedManager
    {
        private const string Admin = "Admin";
        private const string User = "User";
        public static async Task Seed(IServiceProvider services)
        {
            await SeedRoles(services);

            await SeedAdminUser(services);

            await SeedMovieData(services);
        }

        private static async Task SeedRoles(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            if (!roleManager.Roles.Any(r => r.Name == Admin))
            {
                await roleManager.CreateAsync(new IdentityRole(Admin));
            }

            if (!roleManager.Roles.Any(r => r.Name == User))
            {
                await roleManager.CreateAsync(new IdentityRole(User));
            }
        }

        private static async Task SeedAdminUser(IServiceProvider services)
        {
            var context = services.GetRequiredService<MongoDbService>();
            var userManager = services.GetRequiredService<UserManager<User>>();

            var adminUser = (await context.Users.FindAsync(u => u.UserName == "AuthenticationAdmin")).FirstOrDefault();

            if (adminUser is null)
            {
                adminUser = new User
                {
                    UserName = "AuthenticationAdmin",
                    Email = "tra@admin.com",
                    Provider = "Password"
                };
                await userManager.CreateAsync(adminUser, "VerySecretPassword!1");
                await userManager.AddToRoleAsync(adminUser, Role.Admin);
            }
        }

          // Seed Movie
        public static async Task SeedMovieData(IServiceProvider services)
        {
            var context = services.GetRequiredService<MongoDbService>();
            var moviesData = (await context.Movies.FindAsync(_ => true)).ToList();
            if (moviesData.Count == 0)
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "Seeder", "movies.json");
                // var data = File.ReadAllText("../MongoConnector/Seeder/movies.json");
                var data = File.ReadAllText(path);
                var movies = JsonConvert.DeserializeObject<List<Movies>>(data);
                await context.Movies.InsertManyAsync(movies);
            }
        }
    }
}
