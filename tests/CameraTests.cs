﻿using Cameras.Systems;
using Rendering;
using Simulation.Tests;
using Transforms;
using Transforms.Systems;
using Types;
using Worlds;

namespace Cameras.Tests
{
    public abstract class CameraTests : SimulationTests
    {
        static CameraTests()
        {
            TypeRegistry.Load<TransformsTypeBank>();
            TypeRegistry.Load<RenderingTypeBank>();
            TypeRegistry.Load<CamerasTypeBank>();
        }

        protected override Schema CreateSchema()
        {
            Schema schema = base.CreateSchema();
            schema.Load<TransformsSchemaBank>();
            schema.Load<RenderingSchemaBank>();
            schema.Load<CamerasSchemaBank>();
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
