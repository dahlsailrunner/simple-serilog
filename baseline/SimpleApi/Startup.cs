using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Simple.Serilog;
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
            services.AddControllers();
            
            services.AddAuthorization();
            services.AddAuthentication("Bearer")
                .AddJwtBearer(options =>
                {
                    options.Authority = "https://demo.identityserver.io";
                    options.Audience = "api";
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = AuthFailed
                    };
                });
        }

        public void Configure(IApplicationBuilder app)
        {           
            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseApiExceptionHandler();  // from custom helper assembly
            //app.UseApiExceptionHandler(opts => { opts.AddResponseDetails = AddCustomErrorInfo; });            
            app.UseRouting();
            app.UseSimpleSerilogRequestLogging();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void AddCustomErrorInfo(HttpContext ctx, Exception ex, ApiError error)
        {
            // set below values based on context (route, params, etc) or exception details.
            //error.Detail = "";
            //error.Links = "";
            //error.Code = "";
        }
        private Task AuthFailed(AuthenticationFailedContext ctx)
        {
            Log.Warning(ctx.Exception, "API authentication failure occurred.");
            return Task.CompletedTask;
        }
    }
}
