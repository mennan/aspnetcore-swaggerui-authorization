using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace AspNetCore.SwaggerUI.Authorization
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc();

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new Info { Title = "Sample Web API", Version = "v1" });
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseSwagger(c =>
			{
				c.PreSerializeFilters.Add((document, httpRequest) =>
				{
					document.SecurityDefinitions = new ConcurrentDictionary<string, SecurityScheme>();

					document.SecurityDefinitions.Add("Client-Id", new ApiKeyScheme
					{
						Name = "Client-Id",
						Description = "Client Id",
						Type = "apiKey",
						In = "header"
					});
					document.SecurityDefinitions.Add("Client-Secret", new ApiKeyScheme
					{
						Name = "Client-Secret",
						Description = "Client Secret",
						Type = "apiKey",
						In = "header"
					});

					document.Security = new List<IDictionary<string, IEnumerable<string>>>
					{
						new Dictionary<string, IEnumerable<string>>
						{
							{"Client-Id", new List<string>()},
							{"Client-Secret", new List<string>()}
						}
					};
				});
			});

			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sample Web API v1");
				c.RoutePrefix = "help";
			});

			app.Use(async (context, next) =>
			{
				const string ClientIdKey = "Client-Id";
				const string ClientSecretKey = "Client-Secret";

				if (!context.Request.Headers.ContainsKey(ClientIdKey) || !context.Request.Headers.ContainsKey(ClientSecretKey))
				{
					context.Response.StatusCode = 401;
					return;
				}

				const string clientId = "0D09B83A-D7D4-46D9-A2DD-D45FD3CFAFBA";
				const string clientSecret = "FB9A0A9C-C910-483A-B1D7-23234592D735";
				var headerClientId = context.Request.Headers[ClientIdKey];
				var headerSecretId = context.Request.Headers[ClientSecretKey];

				if (!clientId.Equals(headerClientId) && !clientSecret.Equals(headerSecretId))
				{
					context.Response.StatusCode = 401;
					return;
				}

				await next.Invoke();
			});

			app.UseMvc();
		}
	}
}
