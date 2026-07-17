namespace ByHofman.Dispatch;

/// <summary>Marker for a command that produces no result.</summary>
public interface ICommand;

/// <summary>Marker for a command that produces a result of type <typeparamref name="TResult"/>.</summary>
public interface ICommand<TResult>;

/// <summary>Marker for a query that returns a result of type <typeparamref name="TResult"/>.</summary>
public interface IQuery<TResult>;

/// <summary>Marker for an event delivered to zero or more handlers.</summary>
public interface IEvent;
