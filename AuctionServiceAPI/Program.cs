using AuctionServiceAPI.Services;
using AuctionServiceAPI.Interfaces;
using AuctionServiceAPI.Models;
using AuctionServiceAPI.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MongoDB.Driver;
using NLog.Web;
using NLog;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Configure NLog
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // Configuration for JWT
    var configuration = builder.Configuration;
    string secretKey = configuration["SecretKey"] ?? "default_secret";
    string issuer = configuration["Issuer"] ?? "default_issuer";

    // MongoDB Configuration
    builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(configuration["MongoConnectionString"]));
    builder.Services.AddSingleton<IMongoDatabase>(sp =>
    {
        var client = sp.GetRequiredService<IMongoClient>();
        return client.GetDatabase(configuration["DatabaseName"]);
    });

    // Add services to the container
    builder.Services.AddSingleton<IAuctionService, AuctionMongoDBService>();
    builder.Services.AddSingleton<ICatalogService, CatalogServiceClient>();
    builder.Services.AddControllers();

    // Add Authentication and Authorization
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = "http://localhost",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero
            };
        });
    builder.Services.AddAuthorization();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

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