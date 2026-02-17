using Domain.Entities;
using Domain.Enums;
using Mapster;

namespace Domain.MappingConfiguration;

public static class UserEntityMapping
{
    public static void Configure()
    {
        // UserEntity → User
        TypeAdapterConfig<UserEntity, User>
            .NewConfig()
            .Map(dest => dest.Role, src => (Role)src.Role)
            .Map(dest => dest.UserType, src => (UserType)src.UserType)
            .Map(dest => dest.Skills, src => src.Skills != null ? src.Skills.Adapt<List<Skill>>() : null);

        // User → UserEntity
        TypeAdapterConfig<User, UserEntity>
            .NewConfig()
            .Map(dest => dest.Role, src => (int)src.Role)
            .Map(dest => dest.UserType, src => (int)src.UserType)
            .Map(dest => dest.Skills, src => src.Skills != null ? src.Skills.Adapt<List<SkillEntity>>() : null);

        // SkillEntity → Skill
        TypeAdapterConfig<SkillEntity, Skill>
            .NewConfig()
            .Map(dest => dest.Category, src => (SkillCategory)src.Category)
            .Map(dest => dest.Level, src => (SkillLevel)src.Skill);

        // Skill → SkillEntity
        TypeAdapterConfig<Skill, SkillEntity>
            .NewConfig()
            .Map(dest => dest.Category, src => (int)src.Category)
            .Map(dest => dest.Skill, src => (int)src.Level);
    }
}
