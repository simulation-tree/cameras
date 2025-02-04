using System;
using Worlds;

namespace Cameras.Components
{
    [Component]
    public struct CameraSettings
    {
        public const float DefaultMinDepth = 0.01f;
        public const float DefaultMaxDepth = 1000f;

        public float size;
        public bool orthographic;
        public float minDepth;
        public float maxDepth;

        public (float min, float max) Depth
        {
            readonly get => (minDepth, maxDepth);
            set
            {
                minDepth = value.min;
                maxDepth = value.max;
            }
        }

        public CameraSettings(float size, bool orthographic, float minDepth, float maxDepth)
        {
            this.size = size;
            this.orthographic = orthographic;
            this.minDepth = minDepth;
            this.maxDepth = maxDepth;
        }

        public static CameraSettings CreateOrthographic(float size, float minDepth = DefaultMinDepth, float maxDepth = DefaultMaxDepth)
        {
            return new CameraSettings(size, true, minDepth, maxDepth);
        }

        public static CameraSettings CreatePerspectiveDegrees(float fieldOfView, float minDepth = DefaultMinDepth, float maxDepth = DefaultMaxDepth)
        {
            float radians = fieldOfView * MathF.PI / 180f;
            return new CameraSettings(radians, false, minDepth, maxDepth);
        }

        public static CameraSettings CreatePerspectiveRadians(float fieldOfView, float minDepth = DefaultMinDepth, float maxDepth = DefaultMaxDepth)
        {
            return new CameraSettings(fieldOfView, false, minDepth, maxDepth);
        }
    }
}
