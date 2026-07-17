namespace ByHofman.Dispatch;

/// <summary>In-memory dispatcher for commands, queries and events.</summary>
public interface IDispatcher
{
    /// <summary>Sends a command to its single handler.</summary>
    Task SendAsync(ICommand command, CancellationToken cancellationToken = default);

    /// <summary>Sends a command to its single handler and returns the result.</summary>
    Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);

    /// <summary>Sends a query to its single handler and returns the result.</summary>
    Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);

    /// <summary>Publishes an event to all registered handlers, sequentially, in registration order.</summary>
    Task PublishAsync(IEvent @event, CancellationToken cancellationToken = default);
}
