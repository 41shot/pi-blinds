using System;
using System.Threading.Tasks;
using FourOneShot.Pi.Blinds.Components;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using FourOneShot.Pi.Blinds.Http;

namespace FourOneShot.Pi.Blinds
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
          
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents(options =>
                {
                    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromHours(24);
                })
                .AddHubOptions(options =>
                {
                    options.ClientTimeoutInterval = TimeSpan.FromHours(24);
                    options.KeepAliveInterval = TimeSpan.FromSeconds(3);

                });

            builder.Services.AddBlindsApiClient();

            // This builds an extra service provider so that we can get the API client before building the app, which is not great.
            await builder.Configuration.AddExternalConfig(builder.Services.BuildServiceProvider());

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }

    public static class ConfigurationExtensions
    {
        public static async Task AddExternalConfig(this ConfigurationManager configuration, IServiceProvider serviceProvider)
        {
            var apiClient = serviceProvider.GetRequiredService<BlindsApiClient>();
            var externalConfig = await apiClient.GetConfigurationAsStream();
            configuration.AddJsonStream(externalConfig);
        }

    }

    public static class ServicesExtensions
    {
        public static IServiceCollection AddBlindsApiClient(this IServiceCollection services)
        {
            services.AddSingleton(sp =>
            {
                var configuration = sp.GetService<IConfiguration>();
                var logger = sp.GetService<ILogger<BlindsApiClient>>();

                return new BlindsApiClient(configuration, logger);
            });

            return services;
        }
    }
}
