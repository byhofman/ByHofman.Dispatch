using Microsoft.Extensions.DependencyInjection;

namespace ByHofman.Dispatch;

internal abstract class VoidCommandWrapper
{
    public abstract Task HandleAsync(ICommand command, IServiceProvider provider, CancellationToken cancellationToken);
}

internal sealed class VoidCommandWrapper<TCommand> : VoidCommandWrapper where TCommand : ICommand
{
    public override Task HandleAsync(ICommand command, IServiceProvider provider, CancellationToken cancellationToken)
    {
        var handler = provider.GetService<ICommandHandler<TCommand>>()
            ?? throw Dispatcher.NoHandler(typeof(TCommand));
        return handler.HandleAsync((TCommand)command, cancellationToken);
    }
}

internal abstract class CommandWrapper<TResult>
{
    public abstract Task<TResult> HandleAsync(ICommand<TResult> command, IServiceProvider provider, CancellationToken cancellationToken);
}

internal sealed class CommandWrapper<TCommand, TResult> : CommandWrapper<TResult> where TCommand : ICommand<TResult>
{
    public override Task<TResult> HandleAsync(ICommand<TResult> command, IServiceProvider provider, CancellationToken cancellationToken)
    {
        var handler = provider.GetService<ICommandHandler<TCommand, TResult>>()
            ?? throw Dispatcher.NoHandler(typeof(TCommand));
        return handler.HandleAsync((TCommand)command, cancellationToken);
    }
}

internal abstract class QueryWrapper<TResult>
{
    public abstract Task<TResult> HandleAsync(IQuery<TResult> query, IServiceProvider provider, CancellationToken cancellationToken);
}

internal sealed class QueryWrapper<TQuery, TResult> : QueryWrapper<TResult> where TQuery : IQuery<TResult>
{
    public override Task<TResult> HandleAsync(IQuery<TResult> query, IServiceProvider provider, CancellationToken cancellationToken)
    {
        var handler = provider.GetService<IQueryHandler<TQuery, TResult>>()
            ?? throw Dispatcher.NoHandler(typeof(TQuery));
        return handler.HandleAsync((TQuery)query, cancellationToken);
    }
}

internal abstract class EventWrapper
{
    public abstract Task HandleAsync(IEvent @event, IServiceProvider provider, CancellationToken cancellationToken);
}

internal sealed class EventWrapper<TEvent> : EventWrapper where TEvent : IEvent
{
    public override async Task HandleAsync(IEvent @event, IServiceProvider provider, CancellationToken cancellationToken)
    {
        foreach (var handler in provider.GetServices<IEventHandler<TEvent>>())
            await handler.HandleAsync((TEvent)@event, cancellationToken).ConfigureAwait(false);
    }
}
