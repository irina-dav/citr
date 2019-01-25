using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using citr.Models;
using citr.Repositories;
using citr.Infrastructure;
using citr.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using Microsoft.AspNetCore.HttpOverrides;

namespace citr
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<LdapConfig>(Configuration.GetSection("ldap"));
            services.Configure<MailConfig>(Configuration.GetSection("mail"));

         
            services.AddDbContext<ApplicationDbContext>(options => 
                options.UseMySql(Configuration.GetConnectionString("DefaultConnection")));

            services.AddTransient<IResourceRepository, EFResourceRepository>();
            services.AddTransient<IEmployeeRepository, EFEmployeesRepository>();
            services.AddTransient<IRequestRepository, EFRequestRepository>();
            services.AddTransient<IResourceCategoryRepository, EFResourceCategoryRepository>();
            services.AddTransient<IAccessRoleRepository, EFAccessRoleRepository>();

            services.AddScoped<IMailService, MailService>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddScoped<UserManager<AppUser>>();
            services.AddScoped<CategoryTree>();
            services.AddScoped<OTRSService>();
            services.AddHostedService<TicketUpdateService>();

            // services.AddIdentity<AppUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();

            /*services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });*/

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
           .AddCookie(options =>
           {
               options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/Account/Login");
               options.AccessDeniedPath = new Microsoft.AspNetCore.Http.PathString("/Account/AccessDeniedPath");
           });


            services.AddScoped<ILdapService, LdapService>();

            services.AddScoped<HistoryService>();

            services.AddScoped<IViewRenderService, ViewRenderService>();
            services.AddScoped<NotificationService>();

            services.AddMvc();
            services.AddHttpContextAccessor();
            services.AddMemoryCache();
            services.AddSession();
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddFile(Configuration.GetSection("Logging"));
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            app.UseDeveloperExceptionPage();
            app.UseStatusCodePages();
            app.UseStaticFiles();
            app.UseSession();
            app.UseAuthentication();

            //app.UseHttpsRedirection();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "main",
                    template: "{controller=Home}/{action=Index}");
                routes.MapRoute(
                    name: "request",
                    template: "{controller=Request}/{action}/{requestId?}");
            });
        }
    }
}
