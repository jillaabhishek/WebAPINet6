using static System.Net.WebRequestMethods;
using System.Reflection.PortableExecutable;
using System.Runtime.Intrinsics.X86;
using System.Threading.Channels;
using System;
using Contracts;
using LoggerService;
using Repository;
using Service.Contracts;
using Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Versioning;
using CompanyEmployees.Presentation.Controllers;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Identity;
using Entities.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.IdentityModel.Logging;
using System.Net;
using Entities;
using Microsoft.OpenApi.Models;

namespace WebAPI.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyMethod()
                               .WithExposedHeaders("X-Pagination"));
            });

            /*  
            We are using basic CORS policy settings because allowing any origin, method, and header is okay for now.But we should be more
            restrictive with those settings in the production environment.More precisely, as restrictive as possible.
            Instead of the AllowAnyOrigin() method which allows requests from any source, we can use the WithOrigins("https://example.com") 
            which will allow requests only from that concrete source.Also, instead of AllowAnyMethod() that allows all HTTP methods, 
            we can use WithMethods("POST", "GET") that will allow only specific HTTP methods.Furthermore, 
            you can make the same changes for the AllowAnyHeader() method by using, for example, 
            the WithHeaders("accept", "contenttype") method to allow only specific headers.
            */
        }

        public static void ConfigureIISIntegration(this IServiceCollection services) =>
            services.Configure<IISOptions>(options =>
            {
                //we are using default options.
                /*
                 1. AutomaticAuthentication -> default true
                 2. AuthenicationDisplayName -> default null
                 3. ForwardClientCertification -> default true
                 */
            });

        public static void ConfigureLoggerService(this IServiceCollection services) =>
                                        services.AddSingleton<ILoggerManager, LoggerManager>();

        public static void ConfigureRepositoryManager(this IServiceCollection services) =>
                services.AddScoped<IRepositoryManager, RepositoryManager>();

        public static void ConfigureServiceManager(this IServiceCollection services) =>
                services.AddScoped<IServiceManager, ServiceManager>();

        public static void ConfigureSqlContext(this IServiceCollection services, IConfiguration configuration) =>
                services.AddDbContext<RepositoryContext>(opts =>
                    opts.UseSqlServer(configuration.GetConnectionString("sqlConnection")));

        //Shortcut implementation for ConfigureSqlContext
        //But it doesn’t provide all of the features the AddDbContext method provides.So for more advanced
        //options, it is recommended to use AddDbContext. We will use it throughout the rest of the project.

        //public static void ConfigureSqlContext(this IServiceCollection services, IConfiguration configuration) =>
        //        services.AddSqlServer<RepositoryContext>(configuration.GetConnectionString("sqlConnection"));


        public static IMvcBuilder AddCustomCSVFormatter(this IMvcBuilder builder) =>
            builder.AddMvcOptions(config => config.OutputFormatters.Add(new CsvOutputFormatter()));

        public static void AddCustomMediaTypes(this IServiceCollection services)
        {
            services.Configure<MvcOptions>(config =>
            {
                var systemTextJsonOutputFormatter = config.OutputFormatters
                                                          .OfType<SystemTextJsonOutputFormatter>()?
                                                          .FirstOrDefault();

                if (systemTextJsonOutputFormatter != null)
                {
                    systemTextJsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.codemaze.hateoas+json");
                    systemTextJsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.codemaze.apiroot+json");
                }

                var xmlOutputFormatter = config.OutputFormatters
                                               .OfType<XmlDataContractSerializerOutputFormatter>()?
                                               .FirstOrDefault();

                if (xmlOutputFormatter != null)
                {
                    xmlOutputFormatter.SupportedMediaTypes.Add("application/vnd.codemaze.hateoas+xml");
                    xmlOutputFormatter.SupportedMediaTypes.Add("application/vnd.codemaze.apiroot+xml");
                }

            });
        }

        public static void ConfigureVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(opts =>
            {
                opts.ReportApiVersions = true;
                opts.AssumeDefaultVersionWhenUnspecified = true;
                opts.DefaultApiVersion = new ApiVersion(1, 0);
                opts.ApiVersionReader = new HeaderApiVersionReader("api-version");
                //opts.ApiVersionReader = new QueryStringApiVersionReader("api-version"); It's an by default selected

                //Below not using any library  
                //opts.Conventions.Controller<CompaniesController>()
                //                .HasApiVersion(new ApiVersion(1, 0));

                //opts.Conventions.Controller<CompaniesV2Controller>()
                //                .HasDeprecatedApiVersion(new ApiVersion(2, 0));
            });
        }

        public static void ConfigureResponseCaching(this IServiceCollection services) =>
                services.AddResponseCaching();

        public static void ConfigureHttpCacheHeaders(this IServiceCollection services) =>
            services.AddHttpCacheHeaders((expirationOpt) =>
            {
                expirationOpt.MaxAge = 65;
                expirationOpt.CacheLocation = Marvin.Cache.Headers.CacheLocation.Private;
            },
            (validationOpt) =>
            {
                validationOpt.MustRevalidate = true;
            }
            );

        public static void ConfigureRateLimitingOptions(this IServiceCollection services)
        {
            var rateLimitRuls = new List<RateLimitRule>
            {
                new RateLimitRule
                {
                    Endpoint = "*",
                    Limit = 30,
                    Period = "5m"
                }
            };

            services.Configure<IpRateLimitOptions>(opts => opts.GeneralRules = rateLimitRuls);
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
        }

        public static void ConfigureIdentity(this IServiceCollection services)
        {
            var builder = services.AddIdentity<User, IdentityRole>(x =>
            {
                x.Password.RequireDigit = true;
                x.Password.RequireLowercase = false;
                x.Password.RequireUppercase = false;
                x.Password.RequireNonAlphanumeric = false;
                x.Password.RequiredLength = 10;
                x.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<RepositoryContext>()
            .AddDefaultTokenProviders();
        }

        public static void ConfigureJWT(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtConfguration = new JwtConfiguration();
            configuration.Bind(jwtConfguration.Section, jwtConfguration);

            var secretKey = Environment.GetEnvironmentVariable("SECRET");

            services.AddAuthentication(opts =>
            {
                opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtConfguration.ValidIssuer,
                    ValidAudience = jwtConfguration.ValidAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))

                };
            });
        }

        public static void AddJwtConfiguration(this IServiceCollection services, IConfiguration configuration) =>
            services.Configure<JwtConfiguration>(configuration.GetSection("JwtSettings"));

        public static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(x =>
            {
                x.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Code Maze API",
                    Version = "v1",
                    Description = "CompanyEmployees API by CodeMaz",
                    TermsOfService = new Uri("https://example.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "John Doe",
                        Email = "John.Doe@gmail.com",
                        Url =  new Uri("https://twitter/com/johndoe")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "CompanyEmployees API LICX",
                        Url = new Uri("https://example.com/license")
                    }

                });

                x.SwaggerDoc("v2", new OpenApiInfo { Title = "Code Maze API", Version = "v2" });

                var xmlFile = $"{typeof(CompanyEmployees.Presentation.AssemblyReference).Assembly.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                x.IncludeXmlComments(xmlPath);

                x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Place to add JWT with Bearer",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                x.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id="Bearer"
                            },
                            Name = "Bearer"
                        },
                        new List<string>()
                    }
                });
            });

        }
    }
}
