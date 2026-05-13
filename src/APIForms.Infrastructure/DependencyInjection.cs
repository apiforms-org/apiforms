using APIForms.Application.Interfaces;
using APIForms.Domain.Entities;
using APIForms.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace APIForms.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        RegisterMongoMappings();

        var mongo = configuration.GetSection(MongoOptions.SectionName).Get<MongoOptions>() ?? new MongoOptions();
        if (string.IsNullOrWhiteSpace(mongo.ConnectionString))
        {
            throw new InvalidOperationException("Mongo:ConnectionString is required");
        }

        services.AddSingleton<IMongoClient>(_ =>
        {
            var settings = MongoClientSettings.FromConnectionString(mongo.ConnectionString);
            settings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);
            settings.ConnectTimeout = TimeSpan.FromSeconds(5);
            settings.SocketTimeout = TimeSpan.FromSeconds(10);
            settings.MaxConnectionIdleTime = TimeSpan.FromMinutes(1);
            return new MongoClient(settings);
        });
        services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(mongo.Database);
        });

        services.AddScoped<IFormRepository, MongoFormRepository>();
        services.AddScoped<ISubmissionRepository, MongoSubmissionRepository>();
        services.AddScoped<IPermissionRepository, MongoPermissionRepository>();
        services.AddScoped<IApiKeyRepository, MongoApiKeyRepository>();
        services.AddScoped<IUserRepository, MongoUserRepository>();
        services.AddScoped<ISmartQlPolicyRepository, MongoSmartQlPolicyRepository>();
        return services;
    }

    private static void RegisterMongoMappings()
    {
        ConventionRegistry.Register(
            "IgnoreExtraElements",
            new ConventionPack { new IgnoreExtraElementsConvention(true) },
            _ => true);

        if (!BsonClassMap.IsClassMapRegistered(typeof(ApiFormPermission)))
        {
            BsonClassMap.RegisterClassMap<ApiFormPermission>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                cm.MapIdMember(x => x.Id)
                    .SetSerializer(new StringSerializer(BsonType.ObjectId))
                    .SetIgnoreIfNull(true);
            });
        }
    }
}
