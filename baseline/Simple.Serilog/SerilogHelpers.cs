using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Formatting.Compact;

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
                .WriteTo.File(new CompactJsonFormatter(),
                    $@"C:\temp\Logs\{applicationName}.json");
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


        //private static UserInfo AddCustomContextDetails(IHttpContextAccessor ctx)
        //{
        //    var context = ctx.HttpContext;
        //    var user = context?.User.Identity;
        //    if (user == null || !user.IsAuthenticated) return null;

        //    var i = 0;

        //    var userInfo = new UserInfo
        //    {
        //        Name = user.Name,
        //        Claims = context.User.Claims.ToDictionary(x => $"{x.Type} ({i++})", y => y.Value)
        //    };
        //    return userInfo;
        //}
    }
}
