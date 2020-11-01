using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Simple.Serilog;

namespace SimpleUI
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "Cookies";
                    options.DefaultChallengeScheme = "oidc";
                })
                .AddCookie("Cookies")
                .AddOpenIdConnect("oidc", options =>
                {
                    options.SignInScheme = "Cookies";
                    options.Authority = "https://demo.identityserver.io";

                    options.ClientId = "interactive.confidential";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code";
                    options.Scope.Add("email");
                    options.Scope.Add("api");
                    options.Scope.Add("offline_access");
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;                    
                    options.Events.OnTicketReceived = e =>
                    {
                        e.Principal = TransformClaims(e.Principal);
                        return Task.CompletedTask;
                    };
                });

            services.AddAccessTokenManagement();
            services.AddHttpClient<SimpleApiClient>()
                .AddUserAccessTokenHandler();

            services.AddAuthorization();

            services.AddControllersWithViews();
        }

        private ClaimsPrincipal TransformClaims(ClaimsPrincipal principal)
        {            
            var claims = new List<Claim>();
            claims.AddRange(principal.Claims);  // retain any claims from originally authenticated user
            claims.Add(new Claim("junk", "garbage"));

            var newIdentity = new ClaimsIdentity(claims, principal.Identity.AuthenticationType, "name", "role");
            return new ClaimsPrincipal(newIdentity);
        }
        
        public void Configure(IApplicationBuilder app)
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();            
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseRouting();
            app.UseSimpleSerilogRequestLogging();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
