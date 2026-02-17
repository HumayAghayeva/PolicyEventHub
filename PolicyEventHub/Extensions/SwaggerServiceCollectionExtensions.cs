using Microsoft.OpenApi.Models;
using PolicyEventHub.Swagger.Examples;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PolicyEventHub.Extensions
{
    public static class SwaggerServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationSwagger(
           this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.AddApiInfo();
                options.AddSecurity();
                options.AddXmlComments();
                options.ConfigureSchemas();
                options.OperationFilter<DynamicRequestExamplesOperationFilter>();
            });

            return services;
        }

        private static void AddApiInfo(this SwaggerGenOptions options)
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "OSAGO API",
                Version = "v1",
                Description = """
        OSAGO (Compulsory Motor Third-Party Insurance) API for managing
        cancelled policies.

        🔹 Search policies by PIN, plate number, certificate number, or insured full name  
        🔹 Filter results by date range  
        🔹 Supports pagination for large datasets  
        🔹 Designed for secure internal integration

        This API is intended for internal systems and authorized partners only.
        """
            });
        }

        private static void AddSecurity(this SwaggerGenOptions options)
        {
            const string schemeName = "Bearer";

            options.AddSecurityDefinition(schemeName, new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = schemeName,
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Please enter a valid JWT token"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = schemeName
                        }
                    },
                    Array.Empty<string>()
                }
            });
        }
        private static void AddXmlComments(this SwaggerGenOptions options)
        {
            var basePath = AppContext.BaseDirectory;

            foreach (var xmlFile in Directory.GetFiles(basePath, "*.xml"))
            {
                options.IncludeXmlComments(xmlFile, includeControllerXmlComments: true);
            }
        }
        private static void ConfigureSchemas(this SwaggerGenOptions options)
        {
            options.UseAllOfToExtendReferenceSchemas();
        }
    }
}
