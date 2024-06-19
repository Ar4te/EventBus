using EventBus.EventLog.Npgsql.Models;
using EventBus.EventLog.Npgsql.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventBus.EventLog.Npgsql.Extensions;

public static class IntegrationLogExtensions
{
    public static IServiceCollection AddIntegrationEventLog<TDbContext>(this IServiceCollection services) where TDbContext : DbContext
    {
        services.CreateIntegrationEventLogTable<TDbContext>();
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

    public static void CreateIntegrationEventLogTable<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.CreateIntegrationEventLogTableOnNpgsql<TDbContext>();
    }

    private static void CreateIntegrationEventLogTableOnNpgsql<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        using var tDbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        tDbContext.Database.EnsureCreated();
        string checkTableExistsQuery = @"
                SELECT EXISTS (
                    SELECT FROM information_schema.tables 
                    WHERE table_schema = 'public' AND table_name = 'IntegrationEventLog'
                );
            ";
        var tableExists = tDbContext.Database.ExecuteSqlRaw(checkTableExistsQuery);

        // 如果表不存在，创建表
        if (tableExists <= 0)
        {
            string createTableQuery = @"
                    CREATE TABLE ""IntegrationEventLog"" (
                        ""EventId"" UUID PRIMARY KEY,
                        ""EventTypeName"" VARCHAR NOT NULL,
                        ""State"" INTEGER NOT NULL,
                        ""TimesSent"" INTEGER NOT NULL,
                        ""CreationTime"" TIMESTAMP WITHOUT TIME ZONE NOT NULL,
                        ""Content"" VARCHAR NOT NULL,
                        ""TransactionId"" UUID NOT NULL
                    );
                ";
            tDbContext.Database.ExecuteSqlRaw(createTableQuery);
        }
    }
}
