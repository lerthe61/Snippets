// Problem: You need to register in Unity factory method that has single type parameters (a.k.a generic) to produce some generics
// var logger = ILoggerFactory.CreateLogger<T>()
// var logger = container.Resolve<ILogger<T>>();
public static class LoggerFactoryExtensions
{
    /// <summary>
    /// Creates a new ILogger instance using the full name of the given type.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="factory">The factory.</param>
    public static ILogger<T> CreateLogger<T>(this ILoggerFactory factory)
    {
		// create something
	}
}

// register factory
container.RegisterType<LoggerFactory>(new SingletonLifetimeManager());

// lets capture method
/* Capture CreateLogger<T>() method extension */
var factoryMethod = typeof(LoggerFactoryExtensions)
	.GetMethods(BindingFlags.Static | BindingFlags.Public)
	.First(x => x.ContainsGenericParameters && x.Name == "CreateLogger");
// register factory method
container.RegisterType(typeof(ILogger<>), new InjectionFactory((c, t, s) =>
{
	var loggerFactory = c.Resolve<LoggerFactory>();
	/* Resolve all ILogger<> dependencies by creating new logger */
	var genFactoryMethod = factoryMethod.MakeGenericMethod(t.GetGenericArguments()[0]);
	return genFactoryMethod.Invoke(loggerFactory, new object[] { });
}));