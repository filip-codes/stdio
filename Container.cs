namespace Studio;

public class Container
{
    private readonly Dictionary<Type, Func<object>> _registrations = new();
    
    public T Register<T>(Func<T> factory)
    {
        this._registrations[typeof(T)] = () => factory();

        return factory();
    }

    public void Register<T, TImplementation>() where TImplementation : T, new()
    {
        _registrations[typeof(T)] = () => new TImplementation();
    }

    public T Resolve<T>()
    {
        if (this._registrations.TryGetValue(typeof(T), out var factory))
            return (T)factory();

        throw new InvalidOperationException("Service of type {typeof(T)} not found");
    }
}