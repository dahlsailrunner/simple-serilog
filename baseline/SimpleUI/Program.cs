using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using System;
using Serilog.Enrichers.AspnetcoreHttpcontext;
using Simple.Serilog;

namespace SimpleUI
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                CreateWebHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Host terminated unexpectedly");
                Console.Write(ex.ToString());
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }            
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog((provider, context, loggerConfig) =>
                {
                    loggerConfig.WithSimpleConfiguration(provider, "SimpleUI");                    
                });
        }       
    }
}
