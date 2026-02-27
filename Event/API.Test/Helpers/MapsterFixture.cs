using API.MappingConfiguration;
using Domain.MappingConfiguration;

namespace API.Test.Helpers;

public class MapsterFixture : IDisposable
{
    public MapsterFixture()
    {
        EventEntityMapping.Configure();
        DtoMappingConfiguration.Configure();
    }

    public void Dispose()
    {
    }
}

[CollectionDefinition("Mapster")]
public class MapsterCollection : ICollectionFixture<MapsterFixture>
{
}
