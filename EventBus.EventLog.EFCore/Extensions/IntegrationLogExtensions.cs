﻿using EventBus.EventLog.EFCore.Models;
using EventBus.EventLog.EFCore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventBus.EventLog.EFCore.Extensions;

public static class IntegrationLogExtensions
{
    public static IServiceCollection AddIntegrationEventLog<TDbContext>(this IServiceCollection services, DbTypeEnum dbTypeEnum = DbTypeEnum.PostgreSQL) where TDbContext : DbContext
    {
        services.CreateIntegrationEventLogTable<TDbContext>(dbTypeEnum);
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

    public static void CreateIntegrationEventLogTable<TDbContext>(this IServiceCollection services, DbTypeEnum dbTypeEnum)
        where TDbContext : DbContext
    {
        var CreateTable = dbTypeEnum switch
        {
            DbTypeEnum.MySQL => LogTableSQLStr.MySQL.CreateTable("Test"),
            DbTypeEnum.Oracle => LogTableSQLStr.Oracle.CreateTable,
            DbTypeEnum.SQLServer => LogTableSQLStr.SQLServer.CreateTable,
            DbTypeEnum.SQLite => LogTableSQLStr.SQLite.CreateTable,
            DbTypeEnum.PostgreSQL => LogTableSQLStr.PostgreSQL.CreateTable("Test2"),
            _ => throw new InvalidOperationException(nameof(dbTypeEnum))
        };

        switch (dbTypeEnum)
        {
            case DbTypeEnum.MySQL:
                services.CreateIntegrationEventLogTableOnNpgsql<TDbContext>(CreateTable);
                break;
            case DbTypeEnum.Oracle://todo
            case DbTypeEnum.SQLServer://todo
            case DbTypeEnum.SQLite:
                services.CreateIntegrationEventLogTableOnNpgsql<TDbContext>(CreateTable);
                break;
            case DbTypeEnum.PostgreSQL:
            default:
                services.CreateIntegrationEventLogTableOnNpgsql<TDbContext>(CreateTable);
                break;
        }
    }

    private static void CreateIntegrationEventLogTableOnNpgsql<TDbContext>(this IServiceCollection services, string createTableQuery)
       where TDbContext : DbContext
    {
        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        using var tDbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        tDbContext.Database.EnsureCreated();
        using var connection = tDbContext.Database.GetDbConnection();
        connection.Open();
        using var createCommand = connection.CreateCommand();
        createCommand.CommandText = createTableQuery;
        createCommand.ExecuteScalar();
    }
}
