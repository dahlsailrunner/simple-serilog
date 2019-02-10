using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Simple.Serilog.Filters;
using Simple.Serilog.Middleware;

namespace SimpleApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.Filters.Add(new TrackPerformanceFilter());

            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddAuthorization();

            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "https://demo.identityserver.io";
                    options.ApiName = "api";  // defines required scope in bearer token
                });

        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {           
            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseApiExceptionHandler();  // from custom helper assembly
            //app.UseApiExceptionHandler(opts => { opts.AddResponseDetails = AddCustomErrorInfo; });            
            app.UseMvc();
        }

        private void AddCustomErrorInfo(HttpContext ctx, Exception ex, ApiError error)
        {
            // set below values based on context (route, params, etc) or exception details.
            //error.Detail = "";
            //error.Links = "";
            //error.Code = "";
        }
    }
}
