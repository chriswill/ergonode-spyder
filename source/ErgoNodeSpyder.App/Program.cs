using ErgoNodeSharp.Data;
using ErgoNodeSharp.Data.Repositories.NodeInfo;
using Microsoft.Extensions.Hosting;
using ErgoNodeSharp.Models.Configuration;
using ErgoNodeSharp.Services.GeoIp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;

namespace ErgoNodeSpyder.App
{
    public class Program
    {
        private static ErgoConfiguration? ergoConfiguration;
        private static NetworkConfiguration? networkConfiguration;

        static void Main(string[] args)
        {
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception e)
            {
                Log.Error(e, "Error with application");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
        
        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .UseSerilog()
                .ConfigureServices((ctx, services) =>
                {
                    var logDirectory = ctx.Configuration["LogDirectory"] ?? "logs";

                    // Specifying the configuration for serilog
                    LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
                        .ReadFrom.Configuration(ctx.Configuration)
                        .Enrich.FromLogContext()
                        .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
                        .WriteTo.File($"{logDirectory}\\log.txt",
                            rollingInterval: RollingInterval.Day,
                            retainedFileCountLimit: 14,
                            buffered: true,
                            flushToDiskInterval: TimeSpan.FromSeconds(30));

                    Log.Logger = loggerConfiguration.CreateLogger();

                    ergoConfiguration = new ErgoConfiguration();

                    new ConfigureFromConfigurationOptions<ErgoConfiguration>(ctx.Configuration.GetSection("ergo"))
                        .Configure(ergoConfiguration);

                    services.AddSingleton(ergoConfiguration);

                    networkConfiguration = new NetworkConfiguration();
                    
                    new ConfigureFromConfigurationOptions<NetworkConfiguration>(ctx.Configuration.GetSection("scorex:network"))
                        .Configure(networkConfiguration);
                    services.AddSingleton(networkConfiguration);

                    ErgoNodeSpyderConfiguration nodeSpyderConfiguration = new ErgoNodeSpyderConfiguration();
                    new ConfigureFromConfigurationOptions<ErgoNodeSpyderConfiguration>(ctx.Configuration.GetSection("ErgoNodeSpyder"))
                        .Configure(nodeSpyderConfiguration);
                    nodeSpyderConfiguration.ConnectionString =
                        ctx.Configuration.GetConnectionString(nodeSpyderConfiguration.ConnectionStringName);

                    services.AddSingleton(nodeSpyderConfiguration);

                    switch (nodeSpyderConfiguration.DatabaseType)
                    {
                        case "MsSql":
                            services.AddTransient<INodeInfoRepository, SqlServerNodeInfoRepository>();
                            break;
                        default:
                            services.AddTransient<INodeInfoRepository, SqlServerNodeInfoRepository>();
                            break;
                    }

                    services.AddTransient<IGeoIpService, GeoIpService>();

                    services.AddHostedService<WorkerService>();

                });
        }
    }
}
