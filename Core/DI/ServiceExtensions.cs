using System.Reflection;
using Core.DI.LiftetimeAttributes;
using Microsoft.Extensions.DependencyInjection;

namespace Core.DI;

public static class ServiceExtensions
{
    public static void AddServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var types = assembly.GetTypes().Where(type =>
            type.IsClass && !type.IsAbstract && !type.IsGenericType
            && type.GetInterfaces().Contains(typeof(IMarkService)));

        foreach (var type in types)
        {
            var lifetime = GetLifeTime(type);
            var otherInterfaces = type.GetInterfaces()
                .Where(i => i != typeof(IMarkService))
                .ToArray();
            if (!otherInterfaces.Any()) continue;
            foreach (var @otherInterface in otherInterfaces)
            {
                
                services.Add(new ServiceDescriptor(@otherInterface, type, lifetime));
            }
        }
    }

    private static ServiceLifetime GetLifeTime(Type type)
    {
        if (type.GetCustomAttributes<SingletonAttribute>().Any())
        {
            return ServiceLifetime.Singleton;
        }

        if (type.GetCustomAttributes<ScopedAttribute>().Any())
        {
            return ServiceLifetime.Scoped;
        }

        if (type.GetCustomAttributes<TransientAttribute>().Any())
        {
            return ServiceLifetime.Transient;
        }

        return ServiceLifetime.Scoped;
    }
}