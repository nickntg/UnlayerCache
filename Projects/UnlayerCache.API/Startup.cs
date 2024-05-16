using System;
using System.IO;
using System.Reflection;
using Amazon.DynamoDBv2;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using UnlayerCache.API.Models;
using UnlayerCache.API.Services;

namespace UnlayerCache.API;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        var appSettingsSection = Configuration.GetSection("AppSettings");
        services.Configure<AppSettings>(appSettingsSection);

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
	        options.SwaggerDoc("v1", new OpenApiInfo
	        {
		        Version = "v1",
		        Title = "Unlayer Cache",
		        Description = "A caching proxy for some methods of the Unlayer API."
			});

	        var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
	        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename), true);
        });
		services.AddHealthChecks();
		var options = Configuration.GetAWSOptions();
        services.AddScoped(_ => options.CreateServiceClient<IAmazonDynamoDB>());
        services.AddScoped<IDynamoService, DynamoService>();
        services.AddScoped<IUnlayerService, UnlayerService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
	    app.UseSwagger();
	    app.UseSwaggerUI();

		if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Unlayer Cache API");
            });
        });

        app.UseHealthChecks("/health");
	}
}