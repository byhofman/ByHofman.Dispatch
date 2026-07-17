using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ByHofman.Dispatch;

/// <summary>Registration extensions for ByHofman.Dispatch.</summary>
public static class DependencyInjection
{
    private static readonly Type[] HandlerInterfaces =
    [
        typeof(ICommandHandler<>),
        typeof(ICommandHandler<,>),
        typeof(IQueryHandler<,>),
        typeof(IEventHandler<>),
    ];

    /// <summary>Registers <see cref="IDispatcher"/> and every handler found in the given assemblies.</summary>
    public static IServiceCollection AddDispatch(this IServiceCollection services, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        if (assemblies is null || assemblies.Length == 0)
            throw new ArgumentException("At least one assembly must be provided.", nameof(assemblies));

        services.TryAddTransient<IDispatcher, Dispatcher>();

        foreach (var type in assemblies.SelectMany(GetLoadableTypes))
        {
            if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition)
                continue;

            foreach (var contract in type.GetInterfaces())
            {
                if (!contract.IsGenericType)
                    continue;
                if (!HandlerInterfaces.Contains(contract.GetGenericTypeDefinition()))
                    continue;
                services.AddTransient(contract, type);
            }
        }

        return services;
    }

    /// <summary>Registers <see cref="IDispatcher"/> and every handler in the assembly of <typeparamref name="TMarker"/>.</summary>
    public static IServiceCollection AddDispatch<TMarker>(this IServiceCollection services)
        => services.AddDispatch(typeof(TMarker).Assembly);

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(type => type is not null)!;
        }
    }
}
