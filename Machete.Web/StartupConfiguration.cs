using System;
using System.Linq;
using System.Reflection;
using Machete.Data;
using Machete.Data.Infrastructure;
using Machete.Data.Initialize;
using Machete.Data.Repositories;
using Machete.Service;
using Machete.Web.Controllers.Api;
using Machete.Web.Helpers;
using Machete.Web.Helpers.Api;
using Machete.Web.Helpers.Api.Identity;
using Machete.Web.ViewModel.Api.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
// ReSharper disable ArrangeStaticMemberQualifier
// ReSharper disable InvalidXmlDocComment

namespace Machete.Web
{
    /// <summary>
    /// A class containing WebHost and RouteBuilder extension methods.
    /// </summary>
    public static class StartupConfiguration
    {
        /// <summary>
        /// <para>Part of the WebHost extension method pipeline. Calls the Entity Framework Core Migrate() method for the WebHost.</para>
        /// <para>Demonstrates the use of the `using` statement with a context provided by the .NET Core DI container.</para>
        /// </summary>
        public static IWebHost CreateOrMigrateDatabase(this IWebHost webhost)
        {
            using (var scope = webhost.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<MacheteContext>();
                context.Database.Migrate();
                MacheteConfiguration.Seed(context, webhost.Services);
            }

            return webhost;
        }

        /// <summary>
        /// <para>Extends the IServiceCollection object contained in the ConfigureServices method called by the runtime,
        /// and configures authentication for Machete. We currently use ASP.NET Identity with cookie authentication.</para>
        /// <para>JWT: https://github.com/mmacneil/AngularASPNETCore2WebApiAuth/blob/master/src/Startup.cs</para>
        /// </summary>
        public static void ConfigureAuthentication(this IServiceCollection services)
        {
            services.AddIdentity<MacheteUser, IdentityRole>()
                .AddEntityFrameworkStores<MacheteContext>()
                .AddDefaultTokenProviders(); // <~ keep for JWT auth

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme);

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings; we are relying on validation
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 6;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;
            }); // <~ keep for JWT auth

            // Cookie settings
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true; // prevent JavaScript access: https://tools.ietf.org/html/rfc6265
                options.Cookie.Expiration = TimeSpan.FromHours(24);
                options.Cookie.SameSite = SameSiteMode.None;

                // these paths are the defaults, declared here explicitly:
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.SlidingExpiration = true;
            }); // <~ keep for JWT auth

            services.AddCors(options =>
            {
                options.AddPolicy(StartupConfiguration.AllowCredentials, builder =>
                {
                    // for JWT auth, this will have to be reconfigured for "AllowAllOrigins"
                    builder.WithOrigins("http://localhost:4213", "http://localhost:4200", "https://localhost") // TODO
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
        }
        
        /// <summary>
        /// Populate the DI container, which is part of the IServiceCollection. Extension method.
        /// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.2
        /// </summary>
        public static void ConfigureDependencyInjection(this IServiceCollection services)
        {
            services.AddScoped<IDatabaseFactory, DatabaseFactory>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IActivityRepository, ActivityRepository>();
            services.AddScoped<IActivitySigninRepository, ActivitySigninRepository>();
            services.AddScoped<IConfigRepository, ConfigRepository>();
            services.AddScoped<IEmailConfig, EmailConfig>();
            services.AddScoped<IEmailRepository, EmailRepository>();
            services.AddScoped<IEmployerRepository, EmployerRepository>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IImageRepository, ImageRepository>();
            services.AddScoped<ILookupRepository, LookupRepository>();
            services.AddScoped<IPersonRepository, PersonRepository>();
            services.AddScoped<IReportsRepository, ReportsRepository>();
            services.AddScoped<IWorkAssignmentRepository, WorkAssignmentRepository>();
            services.AddScoped<IWorkerRepository, WorkerRepository>();
            services.AddScoped<IWorkerRequestRepository, WorkerRequestRepository>();
            services.AddScoped<IWorkerSigninRepository, WorkerSigninRepository>();
            services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
            services.AddScoped<IScheduleRuleRepository, ScheduleRuleRepository>();
            services.AddScoped<ITransportRuleRepository, TransportRuleRepository>();
            services.AddScoped<ITransportProvidersRepository, TransportProvidersRepository>();
            services.AddScoped<ITransportProvidersAvailabilityRepository, TransportProvidersAvailabilityRepository>();

            services.AddScoped<IActivityService, ActivityService>();
            services.AddScoped<IActivitySigninService, ActivitySigninService>();
            services.AddScoped<IConfigService, ConfigService>();
            services.AddScoped<IEmployerService, EmployerService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<ILookupService, LookupService>();
            services.AddScoped<IOnlineOrdersService, OnlineOrdersService>();
            services.AddScoped<IPersonService, PersonService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IReportsV2Service, ReportsV2Service>();
            services.AddScoped<IScheduleRuleService, ScheduleRuleService>();
            services.AddScoped<ITransportRuleService, TransportRuleService>();
            services.AddScoped<ITransportProvidersService, TransportProvidersService>();
            services.AddScoped<ITransportProvidersAvailabilityService, TransportProvidersAvailabilityService>();
            services.AddScoped<IWorkAssignmentService, WorkAssignmentService>();
            services.AddScoped<IWorkerRequestService, WorkerRequestService>();
            services.AddScoped<IWorkerSigninService, WorkerSigninService>();
            services.AddScoped<IWorkerService, WorkerService>();
            services.AddScoped<IWorkOrderService, WorkOrderService>();

            services.AddScoped<IDefaults, Defaults>();
            services.AddScoped<IModelBindingAdaptor, ModelBindingAdaptor>();
        }

        /// <summary>
        /// Return the names of the Machete API controllers within the calling assembly (e.g., Machete.Web).
        /// </summary>
        private static string GetControllerNames()
        {
            var controllerNames = Assembly.GetCallingAssembly()
                .GetTypes()
                .Where(x =>
                    x.IsSubclassOf(typeof(MacheteApiController)) &&
                    x.FullName.StartsWith(MethodBase.GetCurrentMethod().DeclaringType.Namespace + ".Controllers"))
                .ToList()
                .Select(x => x.Name.Replace("Controller", ""));

            return string.Join("|", controllerNames);
        }
        
        /// <summary>
        /// IRouteBuilder extension method. Defines the routes for the legacy application.
        /// </summary>
        public static void MapLegacyMvcRoutes(this IRouteBuilder routes)
        {
            routes.MapRoute(
                name: "default",
                template: "{controller=Account}/{action=Login}/{id?}");
        }
        
        /// <summary>
        /// IRouteBuilder extension method. Defines the routes for the API.
        /// </summary>
        public static void MapApiRoutes(this IRouteBuilder routes)
        {
            var host = string.Empty;

            routes.MapRoute(
                name: "DefaultApi",
                template: "api/{controller}/{id?}", // {id?} == RouteParameter.Optional
                defaults: new { controller = "Home" },
                constraints: new { controller = GetControllerNames() }
            );
            routes.MapRoute(
                name: "LoginApi",
                template: $"{host.IdentityRoute()}{{action}}",
                defaults: new { controller = "Identity" },
                constraints: new { action = "accounts|login|authorize|logoff" }
            );
            routes.MapRoute(
                name: "WellKnownToken",
                template: $"{host.WellKnownRoute()}{{action}}",
                defaults: new { controller = "Identity" },
                constraints: new { action = "openid-configuration|jwks" }
            );
            routes.MapRoute(
                name: "NotFound",
                template: "{*path}",
                defaults: new { controller = "Error", action = "NotFound" }
            );
        }
        
        /// <summary>
        /// Extension method for the IServiceCollection object in the ConfigureServices method called by the runtime.
        /// Configures options for the issuance of JWTs.
        /// </summary>
        /// <param name="signingKey">An RSA encrypted signing key (https://tools.ietf.org/html/rfc3447#section-3.2).</param>
        /// <param name="configuration">The Configuration field of the Startup class initialized by the runtime.</param>
        public static void ConfigureJwt(
            this IServiceCollection services,
            RsaSecurityKey signingKey,
            IConfiguration configuration
        )
        {
            var configurationSection = configuration.GetSection(nameof(JwtIssuerOptions));
            
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.RsaSha256);
            services.AddSingleton<IJwtFactory, JwtFactory>();
            services.Configure<JwtIssuerOptions>(options =>
            {
                options.Issuer = configurationSection[nameof(JwtIssuerOptions.Issuer)];
                options.Audience = configurationSection[nameof(JwtIssuerOptions.Audience)];
                options.SigningCredentials = credentials;
            });
            
            #region future use
            // Do the following if you want to switch to JWT auth. We currently only generate a token for future use.
            //
                                                             // // PLEASE DO NOT REMOVE // //            
//            var tokenValidationParameters = new TokenValidationParameters
//            {
//                ValidateIssuer = true,
//                ValidIssuer = configurationSection[nameof(JwtIssuerOptions.Issuer)],
//
//                ValidateAudience = true,
//                ValidAudience = configurationSection[nameof(JwtIssuerOptions.Audience)],
//
//                ValidateIssuerSigningKey = true,
//                IssuerSigningKey = signingKey,
//
//                RequireExpirationTime = true,
//                ValidateLifetime = true,
//
//                ClockSkew = TimeSpan.Zero
//            };
//            services.AddAuthentication(options => {
//                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//            }).AddJwtBearer(configureOptions => {
//                configureOptions.ClaimsIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
//                configureOptions.TokenValidationParameters = tokenValidationParameters;
//                configureOptions.SaveToken = true;
//            });
//            services.AddAuthorization(options =>
//            {
//                // TODO put roles here, if using JWT for auth; this will require refactoring attribute methods:
//                options.AddPolicy("ApiUser", policy =>
//                    policy.RequireClaim("role", "api_access"));
//            });
            #endregion future use

            services.AddScoped<JwtIssuerOptions>();
        }

        /// <summary>
        /// A static configuration object representing the computed string value "AllowCredentials".
        /// </summary>
        public static string AllowCredentials => "AllowCredentials";
        
        /// <summary>
        /// A static configuration object representing the computed string value "DefaultConnection".
        /// </summary>
        public static string DefaultConnection => "DefaultConnection";

        /// <summary>
        /// A static configuration object representing the computed string value "Resources".
        /// </summary>
        public static string ResourcesFolder => "Resources";
    }
}