﻿using System;
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
        /// <summary>
        /// Registers dependencies needed to provide a Swagger documentation to the API.
        /// </summary>
        /// <param name="services">THe collection on service collection on which registration should be
        /// performed</param>
        /// <param name="apiName">Name for the API</param>
        /// <param name="apiVersion">SemVer version of the API, usually starts with 'v' like v0.1</param>
        /// <param name="assemblyName">use Assembly.GetExecutingAssembly().GetName().Name</param>
        public static void AddSwaggerJsonAndUi(this IServiceCollection services, string apiName, string apiVersion, string? assemblyName = null)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(apiVersion, new OpenApiInfo { Title = apiName, Version = apiVersion });
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{assemblyName ?? Assembly.GetEntryAssembly()?.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        /// <summary>
        /// Sets up a web UI page to interact with the server via http requests.
        /// </summary>
        /// <param name="app">The web application builder</param>
        /// <param name="apiName">Name for the API</param>
        /// <param name="apiVersion">SemVer version of the API, usually starts with 'v' like v0.1</param>
        /// <param name="setSwaggerUiAsDefaultPage">Use the swaggerUI as default server page</param>
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
