using Domain.Entities;
using Domain.Enums;
using Mapster;

namespace Domain.MappingConfiguration;

public static class EventEntityMapping
{
    public static void Configure()
    {
        TypeAdapterConfig<EventEntity, Event>
            .NewConfig()
            .Map(dest => dest.Slots, src => src.Slots.Adapt<List<Slot>>());

        TypeAdapterConfig<Event, EventEntity>
            .NewConfig()
            .Map(dest => dest.Slots, src => src.Slots.Adapt<List<SlotEntity>>());

        TypeAdapterConfig<SlotEntity, Slot>
            .NewConfig()
            .Map(dest => dest.SkillCategory, src => (SkillCategory)src.SkillCategory)
            .Map(dest => dest.SkillLevel, src => (SkillLevel)src.SkillLevel);

        TypeAdapterConfig<Slot, SlotEntity>
            .NewConfig()
            .Map(dest => dest.SkillCategory, src => (int)src.SkillCategory)
            .Map(dest => dest.SkillLevel, src => (int)src.SkillLevel);
    }
}
