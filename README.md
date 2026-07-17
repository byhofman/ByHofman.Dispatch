# ByHofman.Dispatch

Minimal, in-memory CQRS dispatcher for **commands**, **queries** and **events**.

No message bus, no transport, no pipeline behaviours, no per-call reflection. Handlers are
resolved from `IServiceProvider` through cached generic wrappers.

## Install

```bash
dotnet add package ByHofman.Dispatch
```

## Register

```csharp
builder.Services.AddDispatch(typeof(Program).Assembly);
// or
builder.Services.AddDispatch<SomeHandler>();
```

`AddDispatch` scans the given assemblies and registers every `ICommandHandler<>`,
`ICommandHandler<,>`, `IQueryHandler<,>` and `IEventHandler<>` it finds (transient), plus
`IDispatcher`.

## Messages

```csharp
public sealed record CreateOrder(string Sku, int Qty) : ICommand<Guid>;   // command with result
public sealed record ArchiveOrder(Guid Id) : ICommand;                    // command, no result
public sealed record GetOrder(Guid Id) : IQuery<OrderDto>;                // query
public sealed record OrderCreated(Guid Id) : IEvent;                      // event
```

## Handlers

```csharp
public sealed class CreateOrderHandler : ICommandHandler<CreateOrder, Guid>
{
    public Task<Guid> HandleAsync(CreateOrder command, CancellationToken ct = default)
        => Task.FromResult(Guid.NewGuid());
}

public sealed class OrderCreatedHandler : IEventHandler<OrderCreated>
{
    public Task HandleAsync(OrderCreated @event, CancellationToken ct = default)
        => Task.CompletedTask;
}
```

The result type is anything you want — a DTO, a primitive, or a `Result<T>` from your
result library of choice. `ByHofman.Dispatch` has no opinion on it.

## Dispatch

```csharp
public sealed class Orders(IDispatcher dispatcher)
{
    public Task<Guid> Create(string sku, int qty) => dispatcher.SendAsync(new CreateOrder(sku, qty));
    public Task Archive(Guid id)                   => dispatcher.SendAsync(new ArchiveOrder(id));
    public Task<OrderDto> Get(Guid id)             => dispatcher.QueryAsync(new GetOrder(id));
    public Task Announce(Guid id)                  => dispatcher.PublishAsync(new OrderCreated(id));
}
```

## Semantics

- A command / query resolves **exactly one** handler; a missing handler throws
  `InvalidOperationException`.
- An event is delivered to **all** registered handlers, sequentially, in registration order.
  The first handler that throws stops the rest.

## License

MIT
