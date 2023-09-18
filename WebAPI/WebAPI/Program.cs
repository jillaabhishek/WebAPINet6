using Contracts;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NLog;
using WebAPI.Extensions;
using Microsoft.AspNetCore.Mvc.Formatters;
using CompanyEmployees.Presentation.ActionFilters;
using System.Text.Json.Serialization;
using WebAPI.Utility;
using AspNetCoreRateLimit;
using Microsoft.IdentityModel.Logging;
using MediatR;
using FluentValidation;
using Application.Behaviors;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));

NewtonsoftJsonPatchInputFormatter GetJsonPathInputFormatter()
{
    return new ServiceCollection().AddLogging()
                    .AddMvc()                    
                    .AddNewtonsoftJson()
                    .Services.BuildServiceProvider()
                                  .GetRequiredService<IOptions<MvcOptions>>().Value.InputFormatters
                                  .OfType<NewtonsoftJsonPatchInputFormatter>().First();
}

IdentityModelEventSource.ShowPII = true;

builder.Services.ConfigureCors();
builder.Services.ConfigureIISIntegration();
builder.Services.ConfigureLoggerService();
builder.Services.ConfigureRepositoryManager();
//builder.Services.ConfigureServiceManager();
builder.Services.ConfigureSqlContext(builder.Configuration);
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.Configure<ApiBehaviorOptions>(opts => { opts.SuppressModelStateInvalidFilter = true; });
builder.Services.AddScoped<ValidationFilterAttribute>();
builder.Services.AddScoped<ValidateMediaTypeAttribute>();
//builder.Services.AddScoped<IEmployeeLinks, EmployeeLinks>();

builder.Services.AddControllers(config =>
            {
                config.RespectBrowserAcceptHeader = true;
                config.ReturnHttpNotAcceptable = true;
                config.InputFormatters.Insert(0, GetJsonPathInputFormatter());
                config.CacheProfiles.Add("120SecondDuration", new CacheProfile { Duration = 120 });
            })
                .AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles)
                .AddXmlDataContractSerializerFormatters()
                .AddApplicationPart(typeof(CompanyEmployees.Presentation.AssemblyReference).Assembly);

builder.Services.AddMediatR(typeof(Application.AssemblyReference).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddValidatorsFromAssembly(typeof(Application.AssemblyReference).Assembly);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.ConfigurationExceptionHandler(app.Services.GetRequiredService<ILoggerManager>());

if (app.Environment.IsProduction())
    app.UseHsts();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.All });
app.MapControllers();
//app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
//If we use MapControllerRoute app.UseRouting method to add the routing middleware in the application’s pipeline.



app.Run();
