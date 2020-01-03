using System;
using System.Net.Http;
using AutoMapper;
using LocalDiscogsApi.Clients;
using LocalDiscogsApi.Config;
using LocalDiscogsApi.Database;
using LocalDiscogsApi.Helpers;
using LocalDiscogsApi.Middleware;
using LocalDiscogsApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;

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
            services.AddAutoMapper(typeof(Helpers.MappingEntity));

            services.AddMemoryCache();

            services.AddCors();

            // config
            DiscogsApiOptions discogsApiOptions = Configuration.GetSection(nameof(DiscogsApiOptions)).Get<DiscogsApiOptions>();
            DatabaseOptions databaseOptions = Configuration.GetSection(nameof(DatabaseOptions)).Get<DatabaseOptions>();
            VinylHubApiOptions vinylHubApiOptions = Configuration.GetSection(nameof(VinylHubApiOptions)).Get<VinylHubApiOptions>();

            services.AddSingleton<IDiscogsApiOptions>(discogsApiOptions);
            services.AddSingleton<IDatabaseOptions>(databaseOptions);
            services.AddSingleton<IVinylHubApiOptions>(vinylHubApiOptions);

            // services
            services.AddSingleton<IDbContext, MongoDbContext>();
            services.AddTransient<ITimerService, TimerService>();
            services.AddTransient<IWantlistService, WantlistService>();
            services.AddTransient<IInventoryService, InventoryService>();
            services.AddTransient<IStoreService, StoreService>();

            services.AddControllers(options => options.Filters.Add(typeof(GlobalExceptionFilter)));

            services.AddTransient<PreventRateLimiterHandler>();

            // clients
            // todo: test that this works. need to read up on Polly / HttpClient logging
            Polly.Retry.AsyncRetryPolicy<HttpResponseMessage> ratelimitHitPolicy = Policy<HttpResponseMessage>
                .HandleResult(r => r.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(1, _ => TimeSpan.FromSeconds(discogsApiOptions.RatelimitTimeout));

            services
                .AddHttpClient<IDiscogsClient, DiscogsClient>(c =>
                {
                    c.BaseAddress = new Uri(discogsApiOptions.Url);
                    c.DefaultRequestHeaders.Add("User-Agent", discogsApiOptions.UserAgent);
                })
                .AddHttpMessageHandler<PreventRateLimiterHandler>()
                .AddPolicyHandler(ratelimitHitPolicy); /// extra precaution in case <see cref="PreventRateLimiterHandler"/> fails

            services.AddHttpClient<IVinylHubClient, VinylHubClient>(c => c.BaseAddress = new Uri(vinylHubApiOptions.Url));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // todo: prod??
                app.UseCors(builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
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
