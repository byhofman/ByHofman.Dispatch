using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ByHofman.Dispatch.Tests;

public class DispatcherTests
{
    private static ServiceProvider BuildProvider(Sink? sink = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton(sink ?? new Sink());
        services.AddDispatch(typeof(DispatcherTests).Assembly);
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task Command_with_result_returns_handler_value()
    {
        var dispatcher = BuildProvider().GetRequiredService<IDispatcher>();

        var result = await dispatcher.SendAsync(new Add(2, 3));

        Assert.Equal(5, result);
    }

    [Fact]
    public async Task Void_command_invokes_handler()
    {
        var sink = new Sink();
        var dispatcher = BuildProvider(sink).GetRequiredService<IDispatcher>();

        await dispatcher.SendAsync(new Store("hello"));

        Assert.Equal("hello", sink.Last);
    }

    [Fact]
    public async Task Query_returns_handler_value()
    {
        var dispatcher = BuildProvider().GetRequiredService<IDispatcher>();

        var result = await dispatcher.QueryAsync(new Echo("hi"));

        Assert.Equal("hi", result);
    }

    [Fact]
    public async Task Event_is_delivered_to_all_handlers()
    {
        var sink = new Sink();
        var dispatcher = BuildProvider(sink).GetRequiredService<IDispatcher>();

        await dispatcher.PublishAsync(new Pinged());

        Assert.Equal(2, sink.Events.Count);
        Assert.Contains("first", sink.Events);
        Assert.Contains("second", sink.Events);
    }

    [Fact]
    public async Task Missing_handler_throws()
    {
        var dispatcher = new Dispatcher(new ServiceCollection().BuildServiceProvider());

        await Assert.ThrowsAsync<InvalidOperationException>(() => dispatcher.QueryAsync(new Echo("x")));
    }
}

public sealed class Sink
{
    public string? Last { get; set; }
    public List<string> Events { get; } = [];
}

public sealed record Add(int A, int B) : ICommand<int>;

public sealed class AddHandler : ICommandHandler<Add, int>
{
    public Task<int> HandleAsync(Add command, CancellationToken cancellationToken = default)
        => Task.FromResult(command.A + command.B);
}

public sealed record Store(string Value) : ICommand;

public sealed class StoreHandler(Sink sink) : ICommandHandler<Store>
{
    public Task HandleAsync(Store command, CancellationToken cancellationToken = default)
    {
        sink.Last = command.Value;
        return Task.CompletedTask;
    }
}

public sealed record Echo(string Value) : IQuery<string>;

public sealed class EchoHandler : IQueryHandler<Echo, string>
{
    public Task<string> HandleAsync(Echo query, CancellationToken cancellationToken = default)
        => Task.FromResult(query.Value);
}

public sealed record Pinged : IEvent;

public sealed class FirstPingHandler(Sink sink) : IEventHandler<Pinged>
{
    public Task HandleAsync(Pinged @event, CancellationToken cancellationToken = default)
    {
        sink.Events.Add("first");
        return Task.CompletedTask;
    }
}

public sealed class SecondPingHandler(Sink sink) : IEventHandler<Pinged>
{
    public Task HandleAsync(Pinged @event, CancellationToken cancellationToken = default)
    {
        sink.Events.Add("second");
        return Task.CompletedTask;
    }
}
