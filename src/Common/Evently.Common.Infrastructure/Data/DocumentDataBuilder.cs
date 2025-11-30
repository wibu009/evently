using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Evently.Common.Infrastructure.Data;

public class DocumentDataBuilder<T>
{
    public Action<BsonClassMap<T>> MappingAction { get; private set; } = _ => { };

    public string CollectionName { get; private set; }
    public List<CreateIndexModel<T>> Indexes { get; } = [];
    
#pragma warning disable S4487
#pragma warning disable CS0414
    private bool _useAutoMap = true;
#pragma warning restore CS0414
#pragma warning restore S4487

    public DocumentDataBuilder()
    {
        ResetToAutoMap();
    }
    
    /// <summary>
    /// Set the name of the collection where the documents of type <typeparamref name="T"/> will be stored.
    /// </summary>
    /// <param name="name">The name of the collection.</param>
    public DocumentDataBuilder<T> ToCollection(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        CollectionName = name;
        return this;
    }
    
    /// <summary>
    /// Disable the automatic mapping of the class properties to MongoDB fields.
    /// This will prevent the automatic mapping of properties to MongoDB fields.
    /// Use this when you want to manually configure the mapping.
    /// </summary>
    /// <returns>The <see cref="DocumentDataBuilder{T}"/> instance.</returns>
    public DocumentDataBuilder<T> DisableAutoMap()
    {
        _useAutoMap = false;
        MappingAction = _ => { }; 
        return this;
    }

    /// <summary>
    /// Configure the mapping of the class properties to MongoDB fields.
    /// Use this when you want to manually configure the mapping.
    /// </summary>
    /// <param name="configure">An action that takes a <see cref="BsonClassMap{T}"/> instance and configures the mapping.</param>
    /// <returns>The <see cref="DocumentDataBuilder{T}"/> instance.</returns>
    public DocumentDataBuilder<T> ConfigureMapping(Action<BsonClassMap<T>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        MappingAction += configure;
        return this;
    }

    /// <summary>
    /// Configure the mapping of the <paramref name="idExpr"/> property to the MongoDB "_id" field.
    /// Use this when you want to manually configure the mapping of the Id property.
    /// </summary>
    /// <param name="idExpr">An expression that identifies the Id property.</param>
    /// <returns>The <see cref="DocumentDataBuilder{T}"/> instance.</returns>
    public DocumentDataBuilder<T> MapId(Expression<Func<T, object>> idExpr)
    {
        return ConfigureMapping(cm =>
        {
            MemberInfo member = ReflectionHelper.GetMemberInfo(idExpr);
            cm.MapIdMember(member);
        });
    }

    /// <summary>
    /// Configure the mapping of the <paramref name="propExpr"/> property.
    /// Use this when you want to manually configure the mapping of a property.
    /// </summary>
    /// <param name="propExpr">An expression that identifies the property.</param>
    /// <param name="configure">An action that takes a <see cref="BsonMemberMap"/> instance and configures the mapping of the property.</param>
    /// <returns>The <see cref="DocumentDataBuilder{T}"/> instance.</returns>
    public DocumentDataBuilder<T> MapProperty(Expression<Func<T, object>> propExpr, Action<BsonMemberMap>? configure = null)
    {
        return ConfigureMapping(cm =>
        {
            MemberInfo member = ReflectionHelper.GetMemberInfo(propExpr);
            BsonMemberMap? mm = cm.MapMember(member);
            configure?.Invoke(mm);
        });
    }
    
    /// <summary>
    /// Configure the mapping to ignore the specified property.
    /// Use this when you want to explicitly ignore a property.
    /// </summary>
    /// <param name="propExpr">An expression that identifies the property.</param>
    /// <returns>The <see cref="DocumentDataBuilder{T}"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the property is "Id" or "_id".</exception>
    public DocumentDataBuilder<T> Ignore(Expression<Func<T, object>> propExpr)
    {
        return ConfigureMapping(cm =>
        {
            MemberInfo member = ReflectionHelper.GetMemberInfo(propExpr);
            if (member.Name is "Id" or "_id")
            {
                throw new InvalidOperationException("Cannot ignore Id member.");
            }
            cm.UnmapMember(member);
        });
    }
    
    
    /// <summary>
    /// Create an ascending index on the specified property.
    /// Use this when you want to create an index on a property.
    /// </summary>
    /// <param name="keyExpr">An expression that identifies the property.</param>
    /// <param name="options">Optional index options.</param>
    /// <returns>The <see cref="DocumentDataBuilder{T}"/> instance.</returns>
    public DocumentDataBuilder<T> IndexAscending(Expression<Func<T, object>> keyExpr, CreateIndexOptions? options = null)
    {
        IndexKeysDefinition<T>? key = Builders<T>.IndexKeys.Ascending(keyExpr);
        Indexes.Add(new CreateIndexModel<T>(key, options));
        return this;
    }

    /// <summary>
    /// Create a descending index on the specified property.
    /// Use this when you want to create an index on a property.
    /// </summary>
    /// <param name="keyExpr">An expression that identifies the property.</param>
    /// <param name="options">Optional index options.</param>
    /// <returns>The <see cref="DocumentDataBuilder{T}"/> instance.</returns>
    public DocumentDataBuilder<T> IndexDescending(Expression<Func<T, object>> keyExpr, CreateIndexOptions? options = null)
    {
        IndexKeysDefinition<T>? key = Builders<T>.IndexKeys.Descending(keyExpr);
        Indexes.Add(new CreateIndexModel<T>(key, options));
        return this;
    }
    
    /// <summary>
    /// Create a compound index on the specified keys.
    /// Use this when you want to create a compound index on multiple properties.
    /// </summary>
    /// <param name="keys">An object that contains the indexed properties.</param>
    /// <param name="options">Optional index options.</param>
    /// <returns>The <see cref="DocumentDataBuilder{T}"/> instance.</returns>
    public DocumentDataBuilder<T> IndexCompound(IndexKeysDefinition<T> keys, CreateIndexOptions? options = null)
    {
        Indexes.Add(new CreateIndexModel<T>(keys, options));
        return this;
    }
    

    private void ResetToAutoMap()
    {
        _useAutoMap = true;
        MappingAction = cm => cm.AutoMap();
    }

    private static class ReflectionHelper
    {
        public static MemberInfo GetMemberInfo<TObj>(Expression<Func<TObj, object>> expr)
        {
            Expression body = expr.Body;
            
            if (body is UnaryExpression { NodeType: ExpressionType.Convert } unary)
            {
                body = unary.Operand;
            }

            if (body is MemberExpression memberExpr)
            {
                switch (memberExpr.Member.MemberType)
                {
                    case MemberTypes.Property:
                        return memberExpr.Member;
                    case MemberTypes.Field:
                        return memberExpr.Member;
                }
            }

            throw new ArgumentException($"Expression '{expr}' must resolve to a Property or Field access.", nameof(expr));
        }
    }
}
