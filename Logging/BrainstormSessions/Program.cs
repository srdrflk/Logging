using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Email;
using System;
using System.Net;

namespace BrainstormSessions
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("Logs/myapp-log-.txt", rollingInterval: RollingInterval.Day)
            .WriteTo.Email(new EmailConnectionInfo
            {
                // fill in case with your credentials
                FromEmail = "your-email@example.com",
                ToEmail = "destination-email@example.com",
                MailServer = "smtp.example.com",
                NetworkCredentials = new NetworkCredential("username", "password"),
                EnableSsl = true,
                Port = 587,
                EmailSubject = "Brainstorm Sessions Error Alert"
            },
            restrictedToMinimumLevel: LogEventLevel.Error)
            .CreateLogger();

            try
            {
                Log.Information("Starting the application");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "The application failed to start");
            }
            finally
            {
                Log.CloseAndFlush();
            }

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}
