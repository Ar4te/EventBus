using System.Reflection;
using System.Text.Json.Serialization;
using EventBus.EventLog.EFCore.Extensions;
using EventBus.Extensions;
using EventBus.RabbitMQ;
using Microsoft.EntityFrameworkCore;
using MyTimedTask;
using WebApplication1.Apis;

namespace WebApplication1.Extensions;

public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        //builder.AddRedisClient("redis");
        //builder.Services.AddSingleton<IBasketRespository, RedisBasketRepository>();
        builder.Services.AddDbContext<TestDbContext>(opt =>
        {
            opt.UseNpgsql(builder.Configuration.GetSection("PgSql").Value);
            //var connStr = builder.Configuration.GetSection("MySql").Value;
            //opt.UseMySql(connStr, ServerVersion.AutoDetect(connStr));
            //var connStr = builder.Configuration.GetSection("Sqlite").Value;
            //opt.UseSqlite(connStr);
        }, ServiceLifetime.Transient)
        //.AddIntegrationEventLog<TestDbContext>(DbTypeEnum.SQLite);
        //.AddIntegrationEventLog<TestDbContext>(DbTypeEnum.MySQL);
        .AddIntegrationEventLog<TestDbContext>(DbTypeEnum.PostgreSQL);


        builder.AddRabbitMqEventBus()
            .AddSubscription<Tes32tIntegrationEvent, TestEventHandler>()
            .AddSubscription<Tes32tIntegrationEvent, TestEventHandler2>()
            .ConfigureJsonOptions(opt => opt.TypeInfoResolverChain.Add(IntegrationEventContext.Default));
        builder.Services.AddTimedTask(Assembly.GetExecutingAssembly());
    }
}

[JsonSerializable(typeof(Tes32tIntegrationEvent))]
partial class IntegrationEventContext : JsonSerializerContext
{

}
