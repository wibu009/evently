using Microsoft.AspNetCore.OpenApi;

namespace Evently.Api.Extensions;

internal static class OpenApiExtensions
{
    public static void CustomSchemaIds(this OpenApiOptions config,
        Func<Type, string?> typeSchemaTransformer,
        bool includeValueTypes = false)
    {
        config.AddSchemaTransformer((schema, context, _) =>
        {
            if (!includeValueTypes && 
                (context.JsonTypeInfo.Type.IsValueType || 
                 context.JsonTypeInfo.Type == typeof(string)) ||
                schema.Annotations == null ||
                !schema.Annotations.TryGetValue("x-schema-id", out object? _))
            {
                return Task.CompletedTask;
            }
            
            string? transformedTypeName = typeSchemaTransformer(context.JsonTypeInfo.Type);
            
            schema.Annotations["x-schema-id"] = transformedTypeName;
            
            schema.Title = transformedTypeName;

            return Task.CompletedTask;
        });
    }
}
