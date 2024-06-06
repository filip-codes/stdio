namespace Studio.Foundation;

public class Container
{
    public readonly Dictionary<Type, Func<object>> Registrations = new();
    
    public T Register<T>(Func<T> factory) where T : class
    {
        if (factory is null)
            throw new ArgumentNullException(nameof(factory));
        
        this.Registrations[typeof(T)] = () => factory()!;

        return factory();
    }
    
    public void Register<T>(T objectValue) where T : class
    {
        this.Registrations[typeof(T)] = () => objectValue;
    }

    public void Register<T, TImplementation>() where TImplementation : T, new()
    {
        Registrations[typeof(T)] = () => new TImplementation();
    }

    public T Resolve<T>()
    {
        return (T)this.Resolve(typeof(T));
    }
    
    public object Resolve(Type type)
    {
        if (this.Registrations.TryGetValue(type, out var factory))
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