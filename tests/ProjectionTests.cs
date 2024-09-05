using Cameras.Components;
using Cameras.Systems;
using Rendering;
using Rendering.Components;
using Rendering.Events;
using Simulation;
using System.Numerics;
using Transforms;
using Transforms.Components;
using Transforms.Events;
using Transforms.Systems;

namespace Cameras.Tests
{
    public class ProjectionTests
    {
        private void Simulate(World world)
        {
            world.Submit(new TransformUpdate());
            world.Submit(new CameraUpdate());
            world.Poll();
        }

        [Test]
        public void CheckPerspectiveRay()
        {
            using World world = new();
            using TransformSystem transforms = new(world);
            using CameraSystem cameras = new(world);

            Destination destination = new(world, new(1980, 1080), "dummy");
            Camera camera = new(world, destination, CameraFieldOfView.FromDegrees(90), 0f, 1000f);
            Transform cameraTransform = camera.entity.Become<Transform>();

            Simulate(world);

            CameraProjection projection = camera.GetProjection();
            (Vector3 origin, Vector3 direction) ray = projection.GetRayFromScreenPoint(new(0.5f, 0.5f));
            Assert.That(ray.origin.X, Is.EqualTo(0).Within(0.0001));
            Assert.That(ray.origin.Y, Is.EqualTo(0).Within(0.0001));
            Assert.That(ray.origin.Z, Is.EqualTo(0).Within(0.0001));
            Assert.That(ray.direction.X, Is.EqualTo(0).Within(0.0001));
            Assert.That(ray.direction.Y, Is.EqualTo(0).Within(0.0001));
            Assert.That(ray.direction.Z, Is.EqualTo(1).Within(0.0001));

            cameraTransform.WorldPosition = new(2, 1, 0);
            cameraTransform.WorldRotation = Rotation.FromDirection(Vector3.UnitX).value;

            Simulate(world);

            projection = camera.GetProjection();
            ray = projection.GetRayFromScreenPoint(new(0.5f, 0.5f));
            Assert.That(ray.origin.X, Is.EqualTo(2).Within(0.0001));
            Assert.That(ray.origin.Y, Is.EqualTo(1).Within(0.0001));
            Assert.That(ray.origin.Z, Is.EqualTo(0).Within(0.0001));
            Assert.That(ray.direction.X, Is.EqualTo(1).Within(0.0001));
            Assert.That(ray.direction.Y, Is.EqualTo(0).Within(0.0001));
            Assert.That(ray.direction.Z, Is.EqualTo(0).Within(0.0001));
        }
    }
}
