using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services, ILogger logger, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var scopedProvider = scope.ServiceProvider;
        var context = scopedProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scopedProvider.GetRequiredService<UserManager<AppUser>>();

        await context.Database.MigrateAsync(cancellationToken);

        const string adminEmail = "admin@example.com";
        const string adminPassword = "Pass123$";

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };
            var createResult = await userManager.CreateAsync(admin, adminPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(",", createResult.Errors.Select(e => e.Description));
                logger.LogError("Failed to create seed admin user: {Errors}", errors);
                return;
            }
        }

        if (!await context.Boards.AnyAsync(b => b.OwnerId == admin.Id, cancellationToken))
        {
            var demoBoard = new Board
            {
                Id = Guid.NewGuid(),
                Name = "Demo Board",
                OwnerId = admin.Id
            };

            var todo = new Column
            {
                Id = Guid.NewGuid(),
                Name = "To Do",
                Position = 1,
                BoardId = demoBoard.Id
            };
            var inProgress = new Column
            {
                Id = Guid.NewGuid(),
                Name = "In Progress",
                Position = 2,
                BoardId = demoBoard.Id
            };
            var done = new Column
            {
                Id = Guid.NewGuid(),
                Name = "Done",
                Position = 3,
                BoardId = demoBoard.Id
            };

            var cards = new List<Card>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    BoardId = demoBoard.Id,
                    ColumnId = todo.Id,
                    Title = "Configurar entorno",
                    Description = "Instalar SDK .NET y Node",
                    Position = 1
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    BoardId = demoBoard.Id,
                    ColumnId = inProgress.Id,
                    Title = "Diseñar API",
                    Description = "Definir endpoints y DTOs",
                    Position = 1
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    BoardId = demoBoard.Id,
                    ColumnId = done.Id,
                    Title = "Crear tablero demo",
                    Description = "Poblar columnas iniciales",
                    Position = 1
                }
            };

            demoBoard.Columns = new List<Column> { todo, inProgress, done };
            demoBoard.Cards = cards;

            await context.Boards.AddAsync(demoBoard, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
