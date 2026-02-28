using API.DTOs.Request;
using API.DTOs.Response;
using Application.Commands;
using Domain.Enums;
using Mapster;
using Microsoft.AspNetCore.Http;

namespace API.MappingConfiguration;

public static class DtoMappingConfiguration
{
    public static void Configure()
    {
        TypeAdapterConfig<SlotItemDto, Slot>.NewConfig()
            .Map(dest => dest.SkillCategory, src => (SkillCategory)src.SkillCategory)
            .Map(dest => dest.SkillLevel, src => (SkillLevel)src.SkillLevel);

        TypeAdapterConfig<Slot, SlotResponseDto>.NewConfig()
            .Map(dest => dest.SkillCategory, src => (int)src.SkillCategory)
            .Map(dest => dest.SkillLevel, src => (int)src.SkillLevel);

        TypeAdapterConfig<Event, EventResponseDto>.NewConfig()
            .Map(dest => dest.Slots, src => src.Slots.Adapt<List<SlotResponseDto>>());

        TypeAdapterConfig<CreateEventRequestDto, Application.Commands.CreateEventCommand>.NewConfig()
            .Map(dest => dest.Slots, src => src.Slots.Adapt<List<Slot>>());

        TypeAdapterConfig<UpdateEventRequestDto, Application.Commands.UpdateEventCommand>.NewConfig()
            .Map(dest => dest.Slots, src => src.Slots == null ? null : src.Slots.Adapt<List<Slot>>());

        TypeAdapterConfig<IFormFile, UploadFileItem>.NewConfig()
            .Map(dest => dest.FileName, src => src.FileName)
            .Map(dest => dest.ContentType, src => src.ContentType)
            .Map(dest => dest.Content, src => src.OpenReadStream())
            .Map(dest => dest.Length, src => src.Length);
    }
}
