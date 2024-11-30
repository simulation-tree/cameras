using Worlds;

namespace Cameras.Components
{
    [Component]
    public struct CameraOrthographicSize
    {
        public float value;

        public CameraOrthographicSize(float value)
        {
            this.value = value;
        }
    }
}
