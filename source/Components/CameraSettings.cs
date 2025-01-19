using System;
using Worlds;

namespace Cameras.Components
{
    [Component]
    public struct CameraSettings
    {
        public float size;
        public bool orthographic;

        public CameraSettings(float size, bool orthographic)
        {
            this.size = size;
            this.orthographic = orthographic;
        }

        public static CameraSettings Orthographic(float size)
        {
            return new CameraSettings(size, true);
        }

        public static CameraSettings PerspectiveFromDegrees(float fieldOfView)
        {
            float radians = fieldOfView * MathF.PI / 180f;
            return new CameraSettings(radians, false);
        }

        public static CameraSettings PerspectiveFromRadians(float fieldOfView)
        {
            return new CameraSettings(fieldOfView, false);
        }
    }
}