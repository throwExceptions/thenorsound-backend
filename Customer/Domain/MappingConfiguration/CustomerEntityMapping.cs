using Domain.Entities;
using Domain.Enums;
using Domain.Models;
using Mapster;

namespace Domain.MappingConfiguration;

/// <summary>
/// Configures Mapster mappings between entities and models.
/// </summary>
public static class CustomerEntityMapping
{
    public static void Configure()
    {
        // CustomerEntity → Customer
        TypeAdapterConfig<CustomerEntity, Customer>
            .NewConfig()
            .Map(dest => dest.CustomerType, src => (CustomerType)src.CustomerType)
            .Map(dest => dest.Tariffs, src => src.Tariffs.Adapt<List<Tariff>>());

        // Customer → CustomerEntity
        TypeAdapterConfig<Customer, CustomerEntity>
            .NewConfig()
            .Map(dest => dest.CustomerType, src => (int)src.CustomerType)
            .Map(dest => dest.Tariffs, src => src.Tariffs.Adapt<List<TariffEntity>>());

        // TariffEntity → Tariff
        TypeAdapterConfig<TariffEntity, Tariff>
            .NewConfig()
            .Map(dest => dest.Category, src => (TariffCategory)src.Category)
            .Map(dest => dest.Skill, src => (TariffSkill)src.Skill)
            .Map(dest => dest.TimeType, src => (TariffTimeType)src.TimeType)
            .Map(dest => dest.TariffValue, src => src.Tariff);

        // Tariff → TariffEntity
        TypeAdapterConfig<Tariff, TariffEntity>
            .NewConfig()
            .Map(dest => dest.Category, src => (int)src.Category)
            .Map(dest => dest.Skill, src => (int)src.Skill)
            .Map(dest => dest.TimeType, src => (int)src.TimeType)
            .Map(dest => dest.Tariff, src => src.TariffValue);
    }
}