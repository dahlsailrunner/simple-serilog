using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Enrichers.AspnetcoreHttpcontext;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Simple.Serilog
{
    public static class SerilogHelpers
    {
        /// <summary>
        /// Provides standardized, centralized Serilog wire-up for a suite of applications.
        /// </summary>
        /// <param name="loggerConfig">Provide this value from the UseSerilog method param</param>
        /// <param name="provider">Provide this value from the UseSerilog method param as well</param>
        /// <param name="applicationName">Represents the name of YOUR APPLICATION and will be used to segregate your app
        /// from others in the logging sink(s).</param>
        public static void WithSimpleConfiguration(this LoggerConfiguration loggerConfig, 
            IServiceProvider provider, string applicationName)
        {
            var name = Assembly.GetExecutingAssembly().GetName();

            loggerConfig
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("IdentityServer4", LogEventLevel.Information)
                .Enrich.WithAspnetcoreHttpcontext(provider, AddCustomContextDetails)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProperty("Assembly", $"{name.Name}")
                .Enrich.WithProperty("Version", $"{name.Version}")
                .WriteTo.File(new CompactJsonFormatter(),
                    $@"C:\users\edahl\Source\Logs\{applicationName}.json");
        }

        private static UserInfo AddCustomContextDetails(IHttpContextAccessor ctx)
        {
            var context = ctx.HttpContext;
            var user = context?.User.Identity;
            if (user == null || !user.IsAuthenticated) return null;

            var i = 0;

            var userInfo = new UserInfo
            {
                Name = user.Name,
                Claims = context.User.Claims.ToDictionary(x => $"{x.Type} ({i++})", y => y.Value)
            };
            return userInfo;
        }
    }
}
