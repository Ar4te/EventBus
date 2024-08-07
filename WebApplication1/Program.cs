using EventBus.EventLog.EFCore.Services;
using EventBus.EventLog.EFCore.Utilities;
using Microsoft.AspNetCore.Mvc;
using TimedTask;
using TimedTask.Base;
using TimedTask.Extensions;
using WebApplication1.Apis;
using WebApplication1.Extensions;
using WebApplication1.TimedTasks;

namespace WebApplication1;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();
        builder.AddApplicationServices();
        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddTransient<ITestServices, TestServices>();
        builder.Services.AddTransient<TestCustomTimedTaskService>();
        //builder.adddefaultopenapi
        var app = builder.Build();

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        app.MapGet("/weatherforecast", (HttpContext httpContext) =>
        {
            var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = summaries[Random.Shared.Next(summaries.Length)]
                })
                .ToArray();
            return forecast;
        })
        .WithName("GetWeatherForecast")
        .WithOpenApi();

        app.MapGet("/testEvent", async (HttpContext httpContext, [FromServices] ITestServices testServices, [FromServices] IIntegrationEventLogService integrationEventLogService, [FromServices] TestDbContext testDbContext) =>
        {
            var @event = new Tes32tIntegrationEvent("TestInfo");
            await ResilientTransacation.New(testDbContext).ExecuteAsync(async () =>
            {
                await integrationEventLogService.SaveEventAsync(@event, testDbContext.Database.CurrentTransaction!);
                await integrationEventLogService.MarkEventAsInProgressAsync(@event.Id);
                await testServices.EventBus.PublishAsync(@event);
                await integrationEventLogService.MarkEventAsPublishedAsync(@event.Id);
            });
            await Task.CompletedTask;
        })
        .WithName("TestEvent")
        .WithOpenApi();

        app.MapGet("/testTimedTask", (HttpContext httpContext, TimedTaskScheduler scheduler, IServiceProvider serviceProvider) =>
        {
            var ta = serviceProvider.GetRequiredService<CustomTimedTask2>();
            var dataMap = new TimedTaskDataMap();
            //scheduler.AddTask<CustomTimedTask>($"Task1.{i}", TimeSpan.FromSeconds(1), new TimedTaskDataMap());

            for (int i = 0; i < 50; i++)
            {
                var t = TimedTaskDetail.Build()
                .WithName($"Task1.{i}")
                .WithInterval(TimeSpan.FromSeconds(1))
                .WithRepeats(-1)
                .For<CustomTimedTask2>(() => ta.Execute(dataMap))
                .StartAt(10)
                .UseTaskDataMap(new TimedTaskDataMap());
                try
                {
                    scheduler.AddTask(t);
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            scheduler.StartAll();

        })
        .WithName("TestTimedTask")
        .WithOpenApi();

        app.MapGet("/testTimedTaskPause", (HttpContext httpContext, [FromServices] TimedTaskScheduler timeTaskScheduler, string timedTaskGroupName) =>
        {
            return timeTaskScheduler.PauseTasks(timedTaskGroupName).IsSuccess;
        })
        .WithName("TestTimedTaskPause")
        .WithOpenApi();

        app.MapGet("/testTimedTaskResume", (HttpContext httpContext, [FromServices] TimedTaskScheduler timeTaskScheduler, string timedTaskGroupName) =>
        {
            return timeTaskScheduler.ResumeTasks(timedTaskGroupName).IsSuccess;
        })
        .WithName("TestTimedTaskResume")
        .WithOpenApi();

        app.Run();
    }
}
