using System;
using ErgoNodeSharp.Data;
using ErgoNodeSharp.Data.Repositories.NodeReporting;
using ErgoNodeSharp.Models.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace ErgoNodeSpyder.Portal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateBootstrapLogger();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((ctx, services, configuration) =>
                    {
                        var logDirectory = ctx.Configuration["LogDirectory"] ?? "logs";

                        configuration.ReadFrom.Configuration(ctx.Configuration)
                            .ReadFrom.Services(services)
                            .Enrich.FromLogContext()
                            .WriteTo.Console()
                            .WriteTo.File($"{logDirectory}\\log.txt",
                                rollingInterval: RollingInterval.Day,
                                retainedFileCountLimit: 14,
                                buffered: true,
                                flushToDiskInterval: TimeSpan.FromSeconds(30)); ;
                    })
                .ConfigureServices((ctx, services) =>
                {
                    SpyderAppConnection connection = new SpyderAppConnection();
                    connection.ConnectionString =
                        ctx.Configuration.GetConnectionString("NodeSpyder");
                    services.AddSingleton(connection);

                    services.AddTransient<INodeReportingRepository, SqlServerNodeReportingRepository>();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
