using EventBus.EventLog.Npgsql.Models;
using EventBus.EventLog.Npgsql.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventBus.EventLog.Npgsql.Extensions;

public static class IntegrationLogExtensions
{
    public static IServiceCollection AddIntegrationEventLog<TDbContext>(this IServiceCollection services) where TDbContext : DbContext
    {
        services.AddTransient<IIntegrationEventLogService, IntegrationEventLogService<TDbContext>>();
        return services;
    }

    public static ModelBuilder UseIntegrationEventLogs(this ModelBuilder builder)
    {
        builder.Entity<IntegrationEventLogEntry>(builder =>
        {
            builder.ToTable("IntegrationEventLog");
            builder.HasKey(e => e.EventId);
        });

        return builder;
    }
}
