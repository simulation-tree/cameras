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
        protected override void SetUp()
        {
            base.SetUp();
            ComponentType.Register<IsCamera>();
            ComponentType.Register<CameraOrthographicSize>();
            ComponentType.Register<CameraFieldOfView>();
            ComponentType.Register<CameraMatrices>();
            ComponentType.Register<IsDestination>();
            ComponentType.Register<IsViewport>();
            ComponentType.Register<IsTransform>();
            ComponentType.Register<Position>();
            ComponentType.Register<Rotation>();
            ComponentType.Register<WorldRotation>();
            ComponentType.Register<EulerAngles>();
            ComponentType.Register<Scale>();
            ComponentType.Register<Anchor>();
            ComponentType.Register<Pivot>();
            ComponentType.Register<LocalToWorld>();
            ArrayType.Register<DestinationExtension>();
            Simulator.AddSystem(new TransformSystem());
            Simulator.AddSystem(new CameraSystem());
        }

        [Test]
        public void CheckValuesOfViewMatrix()
        {
            Destination destination = new(World, new(1980, 1080), "dummy");
            Camera camera = new(World, destination, CameraFieldOfView.FromDegrees(90), 0f, 1000f);
            Transform cameraTransform = camera.AsEntity().Become<Transform>();

            Simulator.Update(TimeSpan.FromSeconds(0.01f));

            CameraMatrices matrices = camera.GetMatrices();
            Assert.That(matrices.Position.X, Is.EqualTo(0).Within(0.1f));
            Assert.That(matrices.Position.Y, Is.EqualTo(0).Within(0.1f));
            Assert.That(matrices.Position.Z, Is.EqualTo(0).Within(0.1f));

            cameraTransform.WorldPosition = new(2, 1, 0);
            cameraTransform.WorldRotation = Rotation.FromDirection(Vector3.UnitZ).value;

            Simulator.Update(TimeSpan.FromSeconds(0.01f));

            matrices = camera.GetMatrices();
            Assert.That(matrices.Position.X, Is.EqualTo(2).Within(0.1f));
            Assert.That(matrices.Position.Y, Is.EqualTo(1).Within(0.1f));
            Assert.That(matrices.Position.Z, Is.EqualTo(0).Within(0.1f));
            Assert.That(matrices.Forward.X, Is.EqualTo(0).Within(0.1f));
            Assert.That(matrices.Forward.Y, Is.EqualTo(0).Within(0.1f));
            Assert.That(matrices.Forward.Z, Is.EqualTo(1).Within(0.1f));

            cameraTransform.WorldRotation = Rotation.FromDirection(Vector3.UnitX).value;

            Simulator.Update(TimeSpan.FromSeconds(0.01f));

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
            Destination destination = new(World, new(1980, 1080), "dummy");
            Camera camera = new(World, destination, CameraFieldOfView.FromDegrees(90), 0f, 1000f);
            Transform cameraTransform = camera.AsEntity().Become<Transform>();

            Simulator.Update(TimeSpan.FromSeconds(0.01f));

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

            Simulator.Update(TimeSpan.FromSeconds(0.01f));

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
