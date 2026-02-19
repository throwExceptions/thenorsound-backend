using API.DTOs.Request;
using API.DTOs.Response;
using Mapster;

namespace API.MappingConfiguration;

public static class DtoMappingConfiguration
{
    public static void Configure()
    {
        // SkillItemDto → Skill (request → domain)
        TypeAdapterConfig<SkillItemDto, Skill>.NewConfig()
            .Map(dest => dest.Category, src => (Domain.Enums.SkillCategory)src.Category)
            .Map(dest => dest.Level, src => (Domain.Enums.SkillLevel)src.Skill);

        // Skill → SkillResponseDto (domain → response)
        TypeAdapterConfig<Skill, SkillResponseDto>.NewConfig()
            .Map(dest => dest.Category, src => (int)src.Category)
            .Map(dest => dest.Skill, src => (int)src.Level);

        // User → UserResponseDto (domain → response)
        TypeAdapterConfig<User, UserResponseDto>.NewConfig()
            .Map(dest => dest.Role, src => (int)src.Role)
            .Map(dest => dest.Name, src => $"{src.FirstName} {src.LastName}".Trim())
            .Map(dest => dest.Skills, src => src.Skills != null ? src.Skills.Adapt<List<SkillResponseDto>>() : null);
    }
}
