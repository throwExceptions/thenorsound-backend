using Application.Clients;
using Domain.Entities;
using Domain.Repositories;
using Infra.Clients;
using Infra.Repositories;
using Infra.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Infra;

public static class Infrastructure
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
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

        services.AddScoped<IMongoCollection<UserEntity>>(sp =>
        {
            var database = sp.GetRequiredService<IMongoDatabase>();
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            return database.GetCollection<UserEntity>(settings.UserCollectionName);
        });

        services.Configure<CustomerClientConfiguration>(
            configuration.GetSection("CustomerClientConfiguration"));

        var httpClientBuilder = services.AddHttpClient<ICustomerClient, CustomerClient>((serviceProvider, client) =>
        {
            var config = serviceProvider.GetRequiredService<IOptions<CustomerClientConfiguration>>().Value;
            client.BaseAddress = new Uri(config.BaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        if (environment.IsDevelopment())
        {
            httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });
        }

        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
