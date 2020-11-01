using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Filters;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.MSSqlServer;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;

namespace Simple.Serilog
{
    public static class SerilogHelpers
    {
        /// <summary>
        /// Provides standardized, centralized Serilog wire-up for a suite of applications.
        /// </summary>
        /// <param name="loggerConfig">Provide this value from the UseSerilog method param</param>
        /// <param name="applicationName">Represents the name of YOUR APPLICATION and will be used to segregate your app
        /// from others in the logging sink(s).</param>
        /// <param name="config">IConfiguration settings -- generally read this from appsettings.json</param>
        public static void WithSimpleConfiguration(this LoggerConfiguration loggerConfig, 
            string applicationName, IConfiguration config)
        {
            var name = Assembly.GetExecutingAssembly().GetName();

            loggerConfig
                .ReadFrom.Configuration(config) // minimum levels defined per project in json files 
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProperty("Assembly", $"{name.Name}")
                .Enrich.WithProperty("Version", $"{name.Version}")
                //.WriteTo.File(new CompactJsonFormatter(),
                //    $@"C:\temp\Logs\{applicationName}.json");
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(Matching.WithProperty("UsageName"))
                    .WriteTo.MSSqlServer(
                        connectionString: @"Server=.\sqlexpress;Database=Logging;Trusted_Connection=True;",
                        sinkOptions: new SinkOptions {AutoCreateSqlTable = true, TableName = "UsageLog"},
                        columnOptions: GetSqlColumnOptions()))
                .WriteTo.Logger(lc => lc
                    .Filter.ByExcluding(Matching.WithProperty("UsageName"))
                    .WriteTo.Seq("http://localhost:5341"));
            //.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
            //    {
            //        AutoRegisterTemplate = true,
            //        AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
            //        IndexFormat = "log-{0:yyyy.MM.dd}"
            //    }
            //));
        }

        private static ColumnOptions GetSqlColumnOptions()
        {
            var options = new ColumnOptions();
            return options;
        }

        public static IApplicationBuilder UseSimpleSerilogRequestLogging(this IApplicationBuilder app)
        {
            return app.UseSerilogRequestLogging(opts =>
            {
                opts.EnrichDiagnosticContext = (diagCtx, httpCtx) =>
                {
                    diagCtx.Set("ClientIP", httpCtx.Connection.RemoteIpAddress);
                    diagCtx.Set("UserAgent", httpCtx.Request.Headers["User-Agent"]);
                    if (httpCtx.User.Identity.IsAuthenticated)
                    {
                        var i = 0;
                        var userInfo = new UserInfo
                        {
                            Name = httpCtx.User.Identity.Name,
                            Claims = httpCtx.User.Claims.ToDictionary(x => $"{x.Type} ({i++})", y => y.Value)
                        };
                        diagCtx.Set("UserInfo", userInfo, true);
                    }
                };
            });
        }
    }
}
