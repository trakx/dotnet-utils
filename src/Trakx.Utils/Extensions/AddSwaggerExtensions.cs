using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Trakx.Utils.Extensions
{
    public static class AddSwaggerExtensions
    {
        public static void AddSwaggerJsonAndUi(this IServiceCollection services, string apiName, string apiVersion)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(apiVersion, new OpenApiInfo { Title = apiName, Version = apiVersion });
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        public static void UseSwaggerUiWithForwardedHost(this IApplicationBuilder app,
            string apiName, 
            string apiVersion,
            bool setSwaggerUiAsDefaultPage)
        {
            app.UseSwagger(options =>
            {
                options.PreSerializeFilters.Add((swaggerDoc, httpRequest) =>
                {
                    if (!httpRequest.Headers.ContainsKey("X-Forwarded-Host")) return;

                    var serverUrl = $"{httpRequest.Headers["X-Forwarded-Proto"]}://" +
                                    $"{httpRequest.Headers["X-Forwarded-Host"]}/" +
                                    $"{httpRequest.Headers["X-Forwarded-Prefix"]}";

                    swaggerDoc.Servers = new List<OpenApiServer> { new() { Url = serverUrl } };
                });
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/{apiVersion}/swagger.json", apiName);
                c.InjectJavascript("public/index.js");
            });

            if(setSwaggerUiAsDefaultPage)
                app.UseRewriter(new RewriteOptions().AddRedirect("$^", "swagger"));
        }
    }
}
