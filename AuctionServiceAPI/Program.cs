using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using VaultSharp;
using AuctionServiceAPI.Models;
using AuctionServiceAPI.Services;
using AuctionServiceAPI.Interfaces;
using NLog;
using NLog.Web;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings()
    .GetCurrentClassLogger();
logger.Debug("init main");

try
{
    var builder = WebApplication.CreateBuilder(args);
    var configuration = builder.Configuration;

    // Vault Service: Hent hemmeligheder
    var vaultService = new VaultService(configuration);
    string mySecret = await vaultService.GetSecretAsync("secrets", "SecretKey") ?? "????";
    string myIssuer = await vaultService.GetSecretAsync("secrets", "IssuerKey") ?? "????";
    string myConnectionString = await vaultService.GetSecretAsync("secrets", "MongoConnectionString") ?? "????";

    // Tilføj hemmeligheder til applikationens konfiguration
    configuration["SecretKey"] = mySecret;
    configuration["IssuerKey"] = myIssuer;
    configuration["MongoConnectionString"] = myConnectionString;

    Console.WriteLine("Issuer: " + myIssuer);
    Console.WriteLine("Secret: " + mySecret);
    Console.WriteLine("MongoConnectionString: " + myConnectionString);

    if (string.IsNullOrEmpty(myConnectionString))
    {
        logger.Error("ConnectionString not found in environment variables");
        throw new Exception("ConnectionString not found in environment variables");
    }
    else
    {
        logger.Info("ConnectionString: {0}", myConnectionString);
    }

    // Tilføj services til containeren
    builder.Services.AddControllers(); // Registrerer API-controllers
    builder.Services.AddEndpointsApiExplorer(); // Swagger-endpoints
    builder.Services.AddSwaggerGen(); // Tilføjer Swagger til dokumentation
    builder.Services.AddTransient<VaultService>(); // Vault-service til håndtering af hemmeligheder

    // MongoDB-konfiguration
    builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(myConnectionString));
    builder.Services.AddSingleton<IMongoDatabase>(sp =>
    {
        var client = sp.GetRequiredService<IMongoClient>();
        return client.GetDatabase(configuration["DatabaseName"]); // Hent MongoDB-database
    });

    // Registrer AuctionService og CatalogService
    builder.Services.AddSingleton<IAuctionService, AuctionMongoDBService>();
    builder.Services.AddSingleton<ICatalogService, CatalogServiceClient>();

    // Hosted services til asynkrone opgaver
    builder.Services.AddHostedService<BidReceiver>(); // Modtag bud fra RabbitMQ
    builder.Services.AddHostedService<AuctionCompletionService>(); // Afslut auktioner automatisk

    // Hent URL for CatalogService fra miljøvariabler
    var catalogServiceUrl = Environment.GetEnvironmentVariable("catalogservicehost");
    if (string.IsNullOrEmpty(catalogServiceUrl))
    {
        logger.Error("CatalogServiceUrl not found in environment variables");
        throw new Exception("CatalogServiceUrl not found in environment variables");
    }
    else
    {
        logger.Info("CatalogServiceUrl: {0}", catalogServiceUrl);
    }

    builder.Services.AddHttpClient<ICatalogService, CatalogServiceClient>(client =>
    {
        client.BaseAddress = new Uri(catalogServiceUrl); // Sætter base-URL for CatalogService API
    });

    // JWT Authentication-konfiguration
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true, // Validerer udsteder
            ValidateAudience = true, // Validerer audience
            ValidateLifetime = true, // Validerer tokenets levetid
            ValidateIssuerSigningKey = true, // Validerer signeringsnøgle
            ValidIssuer = myIssuer,
            ValidAudience = "http://localhost",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(mySecret)),
            ClockSkew = TimeSpan.Zero // Ingen forsinkelse på udløb
        };

        options.Events = new JwtBearerEvents
        {
            // Logger fejl ved udløbne tokens
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.Headers.Add("Token-Expired", "true");
                    logger.Error("Token expired: {0}", context.Exception.Message);
                }
                return Task.CompletedTask;
            }
        };
    });

    // Autorisations-politikker
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("UserRolePolicy", policy => policy.RequireRole("1")); // Politik for brugerrolle
        options.AddPolicy("AdminRolePolicy", policy => policy.RequireRole("2")); // Politik for adminrolle
    });

    // Konfiguration af logging
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    var app = builder.Build();

    // HTTP-pipeline-konfiguration
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger(); // Aktiver Swagger i udviklingsmiljø
        app.UseSwaggerUI();
    }

    app.MapControllers(); // Map controllers til endpoints
    app.UseHttpsRedirection(); // Tving HTTPS
    app.UseAuthentication(); // Aktiver autentificering
    app.UseAuthorization(); // Aktiver autorisation

    app.Run(); // Start applikationen
}
catch (Exception ex)
{
    logger.Error(ex, "Stopped program because of exception"); // Logger fejl
    throw;
}
finally
{
    NLog.LogManager.Shutdown(); // Lukker NLog korrekt ned
}
