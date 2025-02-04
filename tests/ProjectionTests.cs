using Cameras.Components;
using Rendering;
using Worlds;

namespace Cameras.Tests
{
    public class CameraEntityTests : CameraTests
    {
        [Test]
        public void VerifyCompliance()
        {
            using World world = CreateWorld();
            Destination destination = new(world, new(100, 100), "Renderer");
            Camera camera = new(world, destination, CameraSettings.CreateOrthographic(1f));

            Assert.That(camera.IsCompliant, Is.True);
        }
    }
}
