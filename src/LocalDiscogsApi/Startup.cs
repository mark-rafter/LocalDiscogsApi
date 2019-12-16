using System;
using LocalDiscogsApi.Config;
using LocalDiscogsApi.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LocalDiscogsApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<DiscogsApiOptions>(Configuration.GetSection(nameof(DiscogsApiOptions)));

            services.AddControllers()
                .AddNewtonsoftJson(options => options.UseMemberCasing());

            services
                .AddHttpClient<IDiscogsClient, DiscogsClient>(c =>
                {
                    c.BaseAddress = new Uri(Configuration["DiscogsApi:Url"]);
                    c.DefaultRequestHeaders.Add("User-Agent", Configuration["DiscogsApi:UserAgent"]);
                }).AddHttpMessageHandler<PreventRateLimiterHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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
            });
        }
    }
}
