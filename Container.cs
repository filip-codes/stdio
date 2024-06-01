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
        return (T)this.Resolve(typeof(T));
    }
    
    public object Resolve(Type type)
    {
        if (this._registrations.TryGetValue(type, out var factory))
            return factory();

        throw new InvalidOperationException($"Service of type {type} not found");
    }

    public object[] ResolveMultiple(Type[] types)
    {
        var parameters = new object[types.Length];
        
        for (int i = 0; i < types.Length; i++)
        {
            parameters[i] = this.Resolve(types[i]);
        }

        return parameters;
    }
}