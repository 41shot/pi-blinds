using FourOneShot.Pi.Devices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace FourOneShot.Pi.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddBlindRemote();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseLogging();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }

    public static class ServicesExtensions
    {
        public static IServiceCollection AddBlindRemote(this IServiceCollection services)
        {
            var processArchitecture = RuntimeInformation.ProcessArchitecture;
            var useStubHardware = !processArchitecture.ToString().StartsWith("ARM", StringComparison.OrdinalIgnoreCase);

            var gpioController = useStubHardware
                    ? new GpioController(PinNumberingScheme.Logical, new StubGpioDriver())
                : new GpioController();

            var blindRemote = new DD2702H(gpioController);
            var resetTask = blindRemote.Reset();

            services.AddSingleton(sp =>
            {
                // Ensure reset task has finished before allowing service instance to be used.
                resetTask.Wait();

                return blindRemote;
            });

            return services;
        }
    }

    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseLogging(this IApplicationBuilder app)
        {
            app.UseMiddleware<LoggingMiddleware>(); // Custom logging middleware

            return app;
        }
    }

    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            _logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path} [{context.Connection.RemoteIpAddress}]");

            await _next(context);

            //_logger.LogInformation($"Response: {context.Response.StatusCode}");
        }
    }
}
