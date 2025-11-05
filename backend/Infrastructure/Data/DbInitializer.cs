using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Common.Security;

namespace Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(DocflowDbContext db)
    {
        await db.Database.MigrateAsync();

        // роль Admin
        var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Administrator");
        if (adminRole is null)
        {
            adminRole = new Role { Id = Guid.NewGuid(), Name = "Administrator", Description = "System administrators" };
            db.Roles.Add(adminRole);
        }

        // пользователь-админ
        var admin = await db.Users.FirstOrDefaultAsync(u => u.Username == "AdmistratorDF");
        if (admin is null)
        {
            admin = new User
            {
                Id = Guid.NewGuid(),
                Username = "AdmistratorDF",
                PasswordHash = PasswordHasher.Hash("password"),
                DisplayName = "System Administrator",
                Email = "saikoniusss@gmail.com",
                IsApproved = true,   // активен сразу
                IsActive = true
            };
            db.Users.Add(admin);
        }

        // связь Admin ↔ Role
        var linkExists = await db.UserRoles.AnyAsync(ur => ur.UserId == admin.Id && ur.RoleId == adminRole.Id);
        if (!linkExists)
            db.UserRoles.Add(new UserRole { UserId = admin.Id, RoleId = adminRole.Id });

        await db.SaveChangesAsync();
    }
}