using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class InjectLeftTimeAttribute : Attribute
{
    public ServiceLifetime ServiceLifetime { get; set; }

    public InjectLeftTimeAttribute(ServiceLifetime serviceLifetime)
    {
        ServiceLifetime = serviceLifetime;
    }
}