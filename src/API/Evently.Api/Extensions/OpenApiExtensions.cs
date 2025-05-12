using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace Evently.Api.Extensions;

internal static class OpenApiExtensions
{
    public static void AddOpenApiDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = "Evently API",
                    Version = "v1",
                    Description = "Evently API empowers seamless event management, connecting organizers and attendees with intuitive tools for creating, managing, and enjoying unforgettable events.",
                    Contact = new OpenApiContact
                    {
                        Name = "Evently Support",
                        Email = "support@evently.com",
                        Url = new Uri("https://evently.com/support")
                    },
                    TermsOfService = new Uri("https://evently.com/terms")
                };
                return Task.CompletedTask;
            });
            options.CustomSchemaIds(type => 
            {
                if (type is not { Name: "Request", DeclaringType: not null })
                {
                    return type.Name;
                }
                
                string cleanedName = type.DeclaringType.Name.Replace("Endpoint", "", StringComparison.OrdinalIgnoreCase);
                return $"{cleanedName}Request";
            });
        });
    }

    private static void CustomSchemaIds(this OpenApiOptions config,
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
