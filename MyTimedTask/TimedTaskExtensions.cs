using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MyTimedTask;

public static class TimedTaskExtensions
{
    public static IServiceCollection AddTimedTask(this IServiceCollection services, Assembly assembly)
    {
        services.AddSingleton<TimeTaskScheduler>();
        var types = assembly.GetTypes().Where(t => typeof(ITimedTask).IsAssignableFrom(t)).ToList();
        if (types.Count != 0)
        {
            foreach (var type in types)
            {
                services.AddTransient(type);
            }
        }
        return services;
    }
}
