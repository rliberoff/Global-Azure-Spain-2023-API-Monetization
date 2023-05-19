using System.Diagnostics;

using GlobalAzureSpain2023.Demo.ApimMonetization.Web.Abstractions;
using GlobalAzureSpain2023.Demo.ApimMonetization.Web.Options;
using GlobalAzureSpain2023.Demo.ApimMonetization.Web.Services;

using Microsoft.Extensions.Logging.ApplicationInsights;

// **********************
// * Load Configuration *
// **********************

var programType = typeof(Program);

var applicationName = programType.Assembly.FullName;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
{
    ApplicationName = applicationName,
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory(),
    WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot"),
});

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());

if (Debugger.IsAttached)
{
    builder.Configuration.AddJsonFile(@"appsettings.debug.json", optional: true, reloadOnChange: true);
}

builder.Configuration.AddJsonFile($@"appsettings.{Environment.UserName}.json", optional: true, reloadOnChange: true)
                     .AddEnvironmentVariables();

var isDevelopment = builder.Environment.IsDevelopment();

// *************************
// * Logging Configuration *
// *************************

if (isDevelopment)
{
    builder.Logging.AddConsole();

    if (Debugger.IsAttached)
    {
        builder.Logging.AddDebug();
    }
}

var applicationInsightsConnectionString = builder.Configuration[@"ApplicationInsights:ConnectionString"];

builder.Logging.AddApplicationInsights((telemetryConfiguration) => telemetryConfiguration.ConnectionString = applicationInsightsConnectionString, (_) => { })
               .AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, LogLevel.Trace)
               ;

// ********************
// * Services Options *
// ********************

builder.Services.AddOptions<ApimServiceOptions>()
                .Bind(builder.Configuration.GetSection(nameof(ApimServiceOptions)))
                .ValidateDataAnnotations()
                .ValidateOnStart()
                ;

builder.Services.AddOptions<MonetizationOptions>()
                .Bind(builder.Configuration.GetSection(nameof(MonetizationOptions)))
                .ValidateDataAnnotations()
                .ValidateOnStart()
                ;

builder.Services.AddOptions<StripeOptions>()
                .Bind(builder.Configuration.GetSection(nameof(StripeOptions)))
                .ValidateDataAnnotations()
                .ValidateOnStart()
                ;

// **************************
// * Services Configuration *
// **************************

// Add application services...
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration)
                .AddHttpContextAccessor()
                .AddHttpClient()
                .AddRouting()
                ;

builder.Services.AddSingleton<IApimService, ApimService>()
                .AddSingleton<IMonetizationService, MonetizationService>()
                ;

builder.Services.AddRazorPages();

// ****************************************
// * Application Middleware Configuration *
// ****************************************

var app = builder.Build();

app.UseHsts()
   .UseStaticFiles()
   .UseRouting()
   .UseAuthentication()
   .UseAuthorization()
   .UseEndpoints(endpoints =>
   {
       endpoints.MapRazorPages();
   })
   ;

app.Run();
