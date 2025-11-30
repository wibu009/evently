using System.Collections.Concurrent;
using System.Reflection;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Evently.Common.Infrastructure.Data;

public abstract class DocumentDataStore
{
    private readonly ConcurrentDictionary<Type, object> _collections = new();
    
    protected IMongoDatabase Database { get; }

    protected DocumentDataStore(IMongoClient client, string databaseName)
    {
        ArgumentNullException.ThrowIfNull(client);
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new ArgumentNullException(nameof(databaseName));
        }

        Database = client.GetDatabase(databaseName);
    }

    protected virtual Assembly[] GetAssembliesToScan() => [];
    
    /// <summary>
    /// Registers a document configuration for the specified type. If a configuration for the type already exists, it is ignored.
    /// </summary>
    /// <typeparam name="T">The type of the document being configured.</typeparam>
    /// <param name="configuration">The configuration for the document type.</param>
    protected void RegisterConfiguration<T>(IDocumentConfiguration<T> configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        
        if (_collections.ContainsKey(typeof(T))) 
        {
            return; 
        }

        var builder = new DocumentDataBuilder<T>();
        configuration.Configure(builder);
        
        if (builder.MappingAction != null)
        {
             RegisterClassMapIfNeeded(typeof(T), builder.MappingAction);
        }
        
        string collName = builder.CollectionName;
        IMongoCollection<T> collection = Database.GetCollection<T>(collName);
        
        _collections.TryAdd(typeof(T), collection);
        
        if (builder.Indexes?.Count > 0)
        {
            try
            {
                collection.Indexes.CreateMany(builder.Indexes);
            }
            catch (Exception) 
            {
                try 
                {
                    foreach (CreateIndexModel<T> idx in builder.Indexes)
                    {
                        collection.Indexes.CreateOne(idx);
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
    
    /// <summary>
    /// Registers all configurations from the given assemblies.
    /// It will scan through all types in the assemblies and register the configurations.
    /// </summary>
    /// <param name="assemblies">The assemblies to scan for configurations.</param>
    protected void RegisterConfigurationsFromAssemblies(Assembly[] assemblies)
    {
        if (assemblies is null || assemblies.Length == 0)
        {
            return;
        }

        var configTypeDetails = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .Select(t => new
            {
                ConfigClassType = t,
                InterfaceType = t.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDocumentConfiguration<>))
            })
            .Where(x => x.InterfaceType != null)
            .ToArray();
        
        MethodInfo? registerMethodDefinition = typeof(DocumentDataStore)
#pragma warning disable S3011
            .GetMethod(nameof(RegisterConfiguration), BindingFlags.Instance | BindingFlags.NonPublic);
#pragma warning restore S3011

        foreach (var item in configTypeDetails)
        {
            try 
            {
                object? configInstance = Activator.CreateInstance(item.ConfigClassType);
                
                Type docType = item.InterfaceType!.GetGenericArguments()[0];
                
                MethodInfo typedRegisterMethod = registerMethodDefinition!.MakeGenericMethod(docType);
                
                typedRegisterMethod.Invoke(this, [configInstance]);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to register configuration for {item.ConfigClassType.Name}", ex);
            }
        }
    }
    
    /// <summary>
    /// Registers a class map for the specified document type, if it has not been registered already.
    /// If a mapping action is provided, it is used to configure the class map. If no mapping action is provided,
    /// the method will attempt to use <see cref="BsonClassMap.AutoMap"/> as a fallback.
    /// </summary>
    /// <param name="docType">The type of the document being registered.</param>
    /// <param name="mappingAction">An optional action used to configure the class map.</param>
    protected void RegisterClassMapIfNeeded(Type docType, object mappingAction)
    {
        MethodInfo? isRegisteredMethod = typeof(BsonClassMap).GetMethod(
            "IsClassMapRegistered", 
            BindingFlags.Public | BindingFlags.Static, 
            [typeof(Type)]
        );

        if (isRegisteredMethod == null)
        {
            return;
        }

        bool alreadyRegistered = (bool)isRegisteredMethod.Invoke(null, [docType])!;

        if (alreadyRegistered)
        {
            return;
        }

        if (mappingAction != null)
        {
            MethodInfo registerMethod = typeof(BsonClassMap)
                .GetMethods()
                .First(m => m is { Name: "RegisterClassMap", IsGenericMethod: true } && m.GetParameters().Length == 1)
                .MakeGenericMethod(docType);

            registerMethod.Invoke(null, [mappingAction]);
        }
        else
        {
            // AutoMap fallback
            MethodInfo registerMethod = typeof(BsonClassMap)
                .GetMethods()
                .First(m => m is { Name: "RegisterClassMap", IsGenericMethod: true } && m.GetParameters().Length == 1)
                .MakeGenericMethod(docType);
            
            MethodInfo helper = GetType()
#pragma warning disable S3011
                .GetMethod(nameof(RegisterAutoMapHelper), BindingFlags.NonPublic | BindingFlags.Static)!
#pragma warning restore S3011
                .MakeGenericMethod(docType);
            
            helper.Invoke(null, [registerMethod]);
        }
    }
    
    private static void RegisterAutoMapHelper<TDoc>(MethodInfo registerMethod)
    {
        Action<BsonClassMap<TDoc>> auto = cm => cm.AutoMap();
        registerMethod.Invoke(null, [auto]);
    }

    /// <summary>
    /// Returns the <see cref="IMongoCollection{T}"/> associated with the specified document type.
    /// If the collection has not been registered yet, it will be created using the name of the document type.
    /// </summary>
    /// <typeparam name="T">The type of the documents stored in the collection.</typeparam>
    /// <returns>The <see cref="IMongoCollection{T}"/> associated with the specified document type.</returns>
    public IMongoCollection<T> GetCollection<T>()
    {
        if (_collections.TryGetValue(typeof(T), out object? coll))
        {
            return (IMongoCollection<T>)coll;
        }
        
        IMongoCollection<T> c = Database.GetCollection<T>(typeof(T).Name);
        _collections.TryAdd(typeof(T), c);
        return c;
    }
}
