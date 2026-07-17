using System.Collections.Concurrent;

namespace ByHofman.Dispatch;

/// <summary>Default <see cref="IDispatcher"/> that resolves handlers from an <see cref="IServiceProvider"/>.</summary>
public sealed class Dispatcher(IServiceProvider provider) : IDispatcher
{
    private static readonly ConcurrentDictionary<Type, VoidCommandWrapper> CommandWrappers = new();
    private static readonly ConcurrentDictionary<(Type Command, Type Result), object> CommandResultWrappers = new();
    private static readonly ConcurrentDictionary<(Type Query, Type Result), object> QueryWrappers = new();
    private static readonly ConcurrentDictionary<Type, EventWrapper> EventWrappers = new();

    public Task SendAsync(ICommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var wrapper = CommandWrappers.GetOrAdd(
            command.GetType(),
            static type => (VoidCommandWrapper)Activator.CreateInstance(
                typeof(VoidCommandWrapper<>).MakeGenericType(type))!);
        return wrapper.HandleAsync(command, provider, cancellationToken);
    }

    public Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var wrapper = (CommandWrapper<TResult>)CommandResultWrappers.GetOrAdd(
            (command.GetType(), typeof(TResult)),
            static key => Activator.CreateInstance(
                typeof(CommandWrapper<,>).MakeGenericType(key.Command, key.Result))!);
        return wrapper.HandleAsync(command, provider, cancellationToken);
    }

    public Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var wrapper = (QueryWrapper<TResult>)QueryWrappers.GetOrAdd(
            (query.GetType(), typeof(TResult)),
            static key => Activator.CreateInstance(
                typeof(QueryWrapper<,>).MakeGenericType(key.Query, key.Result))!);
        return wrapper.HandleAsync(query, provider, cancellationToken);
    }

    public Task PublishAsync(IEvent @event, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);
        var wrapper = EventWrappers.GetOrAdd(
            @event.GetType(),
            static type => (EventWrapper)Activator.CreateInstance(
                typeof(EventWrapper<>).MakeGenericType(type))!);
        return wrapper.HandleAsync(@event, provider, cancellationToken);
    }

    internal static InvalidOperationException NoHandler(Type messageType)
        => new($"No handler registered for '{messageType}'.");
}
