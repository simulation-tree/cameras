using Cameras.Components;
using Cameras.Systems;
using Rendering;
using Rendering.Components;
using Simulation.Tests;
using System;
using System.Numerics;
using Transforms;
using Transforms.Components;
using Transforms.Systems;
using Worlds;

namespace Cameras.Tests
{
    public class ProjectionTests : SimulationTests
    {
        static ProjectionTests()
        {
            TypeLayout.Register<IsCamera>("IsCamera");
            TypeLayout.Register<CameraOrthographicSize>("CameraOrthographicSize");
            TypeLayout.Register<CameraFieldOfView>("CameraFieldOfView");
            TypeLayout.Register<CameraMatrices>("CameraMatrices");
            TypeLayout.Register<IsDestination>("IsDestination");
            TypeLayout.Register<IsViewport>("IsViewport");
            TypeLayout.Register<IsTransform>("IsTransform");
            TypeLayout.Register<Position>("Position");
            TypeLayout.Register<Rotation>("Rotation");
            TypeLayout.Register<WorldRotation>("WorldRotation");
            TypeLayout.Register<EulerAngles>("EulerAngles");
            TypeLayout.Register<Scale>("Scale");
            TypeLayout.Register<Anchor>("Anchor");
            TypeLayout.Register<Pivot>("Pivot");
            TypeLayout.Register<LocalToWorld>("LocalToWorld");
            TypeLayout.Register<DestinationExtension>("DestinationExtension");
        }

        protected override void SetUp()
        {
            base.SetUp();
            world.Schema.RegisterComponent<IsCamera>();
            world.Schema.RegisterComponent<CameraOrthographicSize>();
            world.Schema.RegisterComponent<CameraFieldOfView>();
            world.Schema.RegisterComponent<CameraMatrices>();
            world.Schema.RegisterComponent<IsDestination>();
            world.Schema.RegisterComponent<IsViewport>();
            world.Schema.RegisterComponent<IsTransform>();
            world.Schema.RegisterComponent<Position>();
            world.Schema.RegisterComponent<Rotation>();
            world.Schema.RegisterComponent<WorldRotation>();
            world.Schema.RegisterComponent<EulerAngles>();
            world.Schema.RegisterComponent<Scale>();
            world.Schema.RegisterComponent<Anchor>();
            world.Schema.RegisterComponent<Pivot>();
            world.Schema.RegisterComponent<LocalToWorld>();
            world.Schema.RegisterArrayElement<DestinationExtension>();
            simulator.AddSystem<TransformSystem>();
            simulator.AddSystem<CameraSystem>();
        }

        [Test]
        public void CheckValuesOfViewMatrix()
        {
            Destination destination = new(world, new(1980, 1080), "dummy");
            Camera camera = new(world, destination, CameraFieldOfView.FromDegrees(90), 0f, 1000f);
            Transform cameraTransform = camera.AsEntity().Become<Transform>();

            simulator.Update(TimeSpan.FromSeconds(0.01f));

            CameraMatrices matrices = camera.GetMatrices();
            Assert.That(matrices.Position.X, Is.EqualTo(0).Within(0.1f));
            Assert.That(matrices.Position.Y, Is.EqualTo(0).Within(0.1f));
            Assert.That(matrices.Position.Z, Is.EqualTo(0).Within(0.1f));

            cameraTransform.WorldPosition = new(2, 1, 0);
            cameraTransform.WorldRotation = Rotation.FromDirection(Vector3.UnitZ).value;

            simulator.Update(TimeSpan.FromSeconds(0.01f));

            matrices = camera.GetMatrices();
            Assert.That(matrices.Position.X, Is.EqualTo(2).Within(0.1f));
            Assert.That(matrices.Position.Y, Is.EqualTo(1).Within(0.1f));
            Assert.That(matrices.Position.Z, Is.EqualTo(0).Within(0.1f));
            Assert.That(matrices.Forward.X, Is.EqualTo(0).Within(0.1f));
            Assert.That(matrices.Forward.Y, Is.EqualTo(0).Within(0.1f));
            Assert.That(matrices.Forward.Z, Is.EqualTo(1).Within(0.1f));

            cameraTransform.WorldRotation = Rotation.FromDirection(Vector3.UnitX).value;

            simulator.Update(TimeSpan.FromSeconds(0.01f));

            matrices = camera.GetMatrices();
            Assert.That(matrices.Position.X, Is.EqualTo(2).Within(0.1f));
            Assert.That(matrices.Position.Y, Is.EqualTo(1).Within(0.1f));
            Assert.That(matrices.Position.Z, Is.EqualTo(0).Within(0.1f));
            Assert.That(matrices.Forward.X, Is.EqualTo(1).Within(0.1f));
            Assert.That(matrices.Forward.Y, Is.EqualTo(0).Within(0.1f));
            Assert.That(matrices.Forward.Z, Is.EqualTo(0).Within(0.1f));
        }

        [Test]
        public void CheckPerspectiveRay()
        {
            Destination destination = new(world, new(1980, 1080), "dummy");
            Camera camera = new(world, destination, CameraFieldOfView.FromDegrees(90), 0f, 1000f);
            Transform cameraTransform = camera.AsEntity().Become<Transform>();

            simulator.Update(TimeSpan.FromSeconds(0.01f));

            CameraMatrices projection = camera.GetMatrices();
            (Vector3 origin, Vector3 direction) ray = projection.GetRayFromScreenPoint(new(0.5f, 0.5f));

            Assert.That(ray.origin.X, Is.EqualTo(0).Within(0.1));
            Assert.That(ray.origin.Y, Is.EqualTo(0).Within(0.1));
            Assert.That(ray.origin.Z, Is.EqualTo(0).Within(0.1));
            Assert.That(ray.direction.X, Is.EqualTo(0).Within(0.1));
            Assert.That(ray.direction.Y, Is.EqualTo(0).Within(0.1));
            Assert.That(ray.direction.Z, Is.EqualTo(1).Within(0.1));

            cameraTransform.WorldPosition = new(2, 1, 0);
            cameraTransform.WorldRotation = Rotation.FromDirection(Vector3.UnitX).value;

            simulator.Update(TimeSpan.FromSeconds(0.01f));

            projection = camera.GetMatrices();
            ray = projection.GetRayFromScreenPoint(new(0.5f, 0.5f));

            Assert.That(ray.origin.X, Is.EqualTo(2).Within(0.1));
            Assert.That(ray.origin.Y, Is.EqualTo(1).Within(0.1));
            Assert.That(ray.origin.Z, Is.EqualTo(0).Within(0.1));

            Assert.That(ray.direction.X, Is.EqualTo(1).Within(0.1));
            Assert.That(ray.direction.Y, Is.EqualTo(0).Within(0.1));
            Assert.That(ray.direction.Z, Is.EqualTo(0).Within(0.1));
        }
    }
}
