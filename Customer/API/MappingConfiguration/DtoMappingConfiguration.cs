using API.DTOs.Request;
using API.DTOs.Response;
using Mapster;

namespace API.MappingConfiguration;

public static class DtoMappingConfiguration
{
    public static void Configure()
    {
        // TariffItemDto → Tariff (request → domain)
        TypeAdapterConfig<TariffItemDto, Tariff>.NewConfig()
            .Map(dest => dest.Category, src => (Domain.Enums.TariffCategory)src.Category)
            .Map(dest => dest.Skill, src => (Domain.Enums.TariffSkill)src.Skill)
            .Map(dest => dest.TimeType, src => (Domain.Enums.TariffTimeType)src.TimeType)
            .Map(dest => dest.TariffValue, src => src.Tariff);

        // Tariff → TariffResponseDto (domain → response)
        TypeAdapterConfig<Tariff, TariffResponseDto>.NewConfig()
            .Map(dest => dest.Category, src => (int)src.Category)
            .Map(dest => dest.Skill, src => (int)src.Skill)
            .Map(dest => dest.TimeType, src => (int)src.TimeType)
            .Map(dest => dest.Tariff, src => src.TariffValue);

        // Customer → CustomerResponseDto (domain → response)
        TypeAdapterConfig<Customer, CustomerResponseDto>.NewConfig()
            .Map(dest => dest.CustomerType, src => (int)src.CustomerType)
            .Map(dest => dest.Tariffs, src => src.Tariffs.Adapt<List<TariffResponseDto>>());
    }
}
