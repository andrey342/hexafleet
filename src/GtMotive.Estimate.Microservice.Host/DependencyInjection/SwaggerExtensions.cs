using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace GtMotive.Estimate.Microservice.Host.DependencyInjection
{
    internal static class SwaggerExtensions
    {
        private static string AssemblyName => Assembly.GetEntryAssembly().GetName().Name;

        private static string AssemblyVersion => Assembly.GetEntryAssembly().GetName().Version.ToString();

        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(
                options =>
                {
                    options.CustomSchemaIds(type => type.ToString());
                    options.SwaggerDoc($"v{AssemblyVersion}", new OpenApiInfo
                    {
                        Title = $"{AssemblyName} API",
                        Version = $"v{AssemblyVersion}",
                    });
                });

            return services;
        }

        public static IApplicationBuilder UseSwaggerInApplication(
            this IApplicationBuilder app,
            PathBase pathBase,
            IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(pathBase);

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger(options =>
            {
                if (!pathBase.IsDefault)
                {
                    options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;
                    options.RouteTemplate = "swagger/{documentName}/swagger.json";
                    options.PreSerializeFilters.Add((document, request) =>
                    {
                        document.Servers =
                        [
                            new OpenApiServer
                            {
                                Url = $"{request.Scheme}://{request.Host.Value}{pathBase.CurrentWithoutTrailingSlash}"
                            }

                        ];
                    });
                }
            });

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            var url = pathBase.IsDefault
                ? $"/swagger/v{AssemblyVersion}/swagger.json"
                : $"{pathBase.CurrentWithoutTrailingSlash}/swagger/v{AssemblyVersion}/swagger.json";

            app.UseSwaggerUI(
                options =>
                {
                    options.SwaggerEndpoint(url, $"{AssemblyName} API V{AssemblyVersion}");

                    if (configuration.GetValue<string>("Swagger:EnableTryIt") == "No")
                    {
                        options.SupportedSubmitMethods();
                    }
                });

            return app;
        }
    }
}
