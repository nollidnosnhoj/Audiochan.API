using System;
using System.Collections.Generic;
using System.Linq;
using Audiochan.Core.Common.Constants;
using Audiochan.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace Audiochan.Infrastructure.Persistence
{
    public static class ApplicationDbSeeder
    {
        public static void GetSeed(ApplicationDbContext context, UserManager<User> userManager,
            RoleManager<Role> roleManager)
        {
            if (!userManager.Users.Any())
            {
                var superuser = new User("superuser", "superuser@localhost", DateTime.UtcNow);

                // TODO: Do not hardcode superuser password when deploying into production haha
                userManager
                    .CreateAsync(superuser, "Password1")
                    .GetAwaiter()
                    .GetResult();

                var superUserRole = roleManager.FindByNameAsync(UserRoleConstants.Admin).GetAwaiter().GetResult();

                if (superUserRole == null)
                {
                    roleManager.CreateAsync(new Role {Name = UserRoleConstants.Admin})
                        .GetAwaiter()
                        .GetResult();
                }

                userManager.AddToRoleAsync(superuser, UserRoleConstants.Admin)
                    .GetAwaiter()
                    .GetResult();
            }

            AddGenreSeeds(context);
        }

        private static void AddGenreSeeds(ApplicationDbContext context)
        {
            if (!context.Genres.Any())
            {
                var genres = new List<Genre>
                {
                    new() {Name = "Alternative Rock", Slug = "alternative-rock"},
                    new() {Name = "Ambient", Slug = "ambient"},
                    new() {Name = "Classical", Slug = "classical"},
                    new() {Name = "Country", Slug = "country"},
                    new() {Name = "Deep House", Slug = "deep-house"},
                    new() {Name = "Disco", Slug = "disco"},
                    new() {Name = "Drum & Bass", Slug = "drum-n-bass"},
                    new() {Name = "Dubstep", Slug = "dubstep"},
                    new() {Name = "Electronic", Slug = "electronic"},
                    new() {Name = "Folk", Slug = "folk"},
                    new() {Name = "House", Slug = "house"},
                    new() {Name = "Indie", Slug = "indie"},
                    new() {Name = "Jazz & Blue", Slug = "jazz-n-blue"},
                    new() {Name = "Latin", Slug = "latin"},
                    new() {Name = "Metal", Slug = "metal"},
                    new() {Name = "Miscellaneous", Slug = "misc"},
                    new() {Name = "Piano", Slug = "piano"},
                    new() {Name = "Pop", Slug = "pop"},
                    new() {Name = "R&B & Soul", Slug = "rnb-n-soul"},
                    new() {Name = "Reggae", Slug = "reggae"},
                    new() {Name = "Rock", Slug = "rock"},
                    new() {Name = "Soundtrack", Slug = "soundtrack"},
                    new() {Name = "Techno", Slug = "techno"},
                    new() {Name = "Trance", Slug = "trance"},
                    new() {Name = "Trap", Slug = "trap"},
                    new() {Name = "World", Slug = "world"}
                };

                context.Genres.AddRange(genres);
                context.SaveChanges();
            }
        }
    }
}