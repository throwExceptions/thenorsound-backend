using Application.Clients;
using Domain.Entities;
using Domain.Repositories;
//using Infra.Clients;
using Infra.Repositories;
using Infra.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.Text;

namespace Infra;

public static class Infrastructure
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MongoDbSettings>(
            configuration.GetSection("MongoDbSettings"));

        services.AddSingleton<IMongoClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            return new MongoClient(settings.ConnectionString);
        });

        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(settings.DatabaseName);
        });

        services.AddScoped<IMongoCollection<CustomerEntity>>(sp =>
        {
            var database = sp.GetRequiredService<IMongoDatabase>();
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            return database.GetCollection<CustomerEntity>(settings.CustomerCollectionName);
        });

        //services.Configure<SampleConfiguration>(
        //    configuration.GetSection("SampleConfiguration"));

        //// Registrera SampleClient med HttpClient
        //services.AddHttpClient<ISampleClient, SampleClient>((serviceProvider, client) =>
        //{
        //    var config = serviceProvider.GetRequiredService<IOptions<SampleConfiguration>>().Value;
        //    client.BaseAddress = new Uri(config.BaseUrl);
        //    client.DefaultRequestHeaders.Add("Accept", "application/json");

        //    if (!string.IsNullOrEmpty(config.ApiKey))
        //    {
        //        client.DefaultRequestHeaders.Add("X-API-Key", config.ApiKey);
        //    }
        //});
        services.AddScoped<ICustomerRepository, CustomerRepository>();

        services.Configure<JwtSettings>(
            configuration.GetSection("JwtSettings"));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtSettings>>((bearerOptions, jwtSettingsOptions) =>
            {
                var settings = jwtSettingsOptions.Value;
                if (string.IsNullOrEmpty(settings.SecretKey))
                    return;

                bearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = settings.Issuer,
                    ValidAudience = settings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(settings.SecretKey))
                };
            });

        return services;
    }
}
