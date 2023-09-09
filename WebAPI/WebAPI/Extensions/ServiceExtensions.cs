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

    }
}
