using Cameras.Systems;
using Simulation.Tests;
using Transforms.Systems;
using Types;
using Worlds;

namespace Cameras.Tests
{
    public abstract class CameraTests : SimulationTests
    {
        static CameraTests()
        {
            TypeRegistry.Load<Transforms.TypeBank>();
            TypeRegistry.Load<Rendering.Core.TypeBank>();
            TypeRegistry.Load<Cameras.TypeBank>();
        }

        protected override Schema CreateSchema()
        {
            Schema schema = base.CreateSchema();
            schema.Load<Transforms.SchemaBank>();
            schema.Load<Rendering.Core.SchemaBank>();
            schema.Load<Cameras.SchemaBank>();
            return schema;
        }

        protected override void SetUp()
        {
            base.SetUp();
            simulator.AddSystem<TransformSystem>();
            simulator.AddSystem<CameraSystem>();
        }
    }
}
