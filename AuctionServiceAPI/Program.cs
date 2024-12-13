using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.Commons;
using AuctionServiceAPI.Models;
using AuctionServiceAPI.Services;
using AuctionServiceAPI.Interfaces;
using NLog;
using NLog.Web;
using System;


var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings()
.GetCurrentClassLogger();
logger.Debug("init main");

try
{
    var builder = WebApplication.CreateBuilder(args);

    var configuration = builder.Configuration;

    var vaultService = new VaultService(configuration);

    string mySecret = await vaultService.GetSecretAsync("secrets", "SecretKey") ?? "????";
    string myIssuer = await vaultService.GetSecretAsync("secrets", "IssuerKey") ?? "????";
    string myConnectionString = await vaultService.GetSecretAsync("secrets", "MongoConnectionString") ?? "????";

    // Set secrets, issuer, and connection string in the configuration
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

    // Add services to the container
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddControllers();
    builder.Services.AddTransient<VaultService>();
    builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(myConnectionString));
    builder.Services.AddSingleton<IMongoDatabase>(sp =>
    {
        var client = sp.GetRequiredService<IMongoClient>();
        return client.GetDatabase(configuration["DatabaseName"]);
    });

    builder.Services.AddSingleton<IAuctionService, AuctionMongoDBService>();
    builder.Services.AddSingleton<ICatalogService, CatalogServiceClient>();
    builder.Services.AddHostedService<BidReceiver>();
    builder.Services.AddHostedService<AuctionCompletionService>();

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
        client.BaseAddress = new Uri(catalogServiceUrl);
    });

    // Configure JWT Authentication
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = myIssuer,
            ValidAudience = "http://localhost",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(mySecret)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
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

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("UserRolePolicy", policy => policy.RequireRole("1")); // Brugerrolle-politik
        options.AddPolicy("AdminRolePolicy", policy => policy.RequireRole("2")); // Adminrolle-politik
    });


    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.MapControllers();
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Stopped program because of exception");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}