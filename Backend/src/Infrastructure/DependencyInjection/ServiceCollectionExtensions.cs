using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Services.Export;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' was not found.");
        var provider = configuration.GetValue<string>("Database:Provider")?.ToLowerInvariant();

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (provider == "sqlserver")
            {
                options.UseSqlServer(connectionString);
            }
            else
            {
                options.UseNpgsql(connectionString);
            }
        });

        services
            .AddIdentityCore<AppUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = false;
                options.Password.RequiredLength = 6;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IExcelExporter, ExcelExporter>();
        services.AddScoped<IPdfExporter, PdfExporter>();

        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        return services;
    }
}
