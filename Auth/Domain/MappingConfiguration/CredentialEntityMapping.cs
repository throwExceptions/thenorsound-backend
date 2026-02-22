using Domain.Entities;
using Domain.Models;
using Mapster;

namespace Domain.MappingConfiguration;

public static class CredentialEntityMapping
{
    public static void Configure()
    {
        TypeAdapterConfig<CredentialEntity, Credential>.NewConfig();
        TypeAdapterConfig<Credential, CredentialEntity>.NewConfig();
    }
}
