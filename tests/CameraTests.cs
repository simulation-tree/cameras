using Rendering;
using Transforms;
using Types;
using Worlds;
using Worlds.Tests;

namespace Cameras.Tests
{
    public abstract class CameraTests : WorldTests
    {
        static CameraTests()
        {
            MetadataRegistry.Load<TransformsMetadataBank>();
            MetadataRegistry.Load<RenderingMetadataBank>();
            MetadataRegistry.Load<CamerasMetadataBank>();
        }

        protected override Schema CreateSchema()
        {
            Schema schema = base.CreateSchema();
            schema.Load<TransformsSchemaBank>();
            schema.Load<RenderingSchemaBank>();
            schema.Load<CamerasSchemaBank>();
            return schema;
        }
    }
}
