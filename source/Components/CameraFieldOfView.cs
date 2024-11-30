using System;
using Worlds;

namespace Cameras.Components
{
    [Component]
    public struct CameraFieldOfView
    {
        /// <summary>
        /// Value in radians.
        /// </summary>
        public float value;

        public float Degrees
        {
            readonly get => value * 180.0f / MathF.PI;
            set => this.value = MathF.PI * value / 180.0f;
        }

        public CameraFieldOfView(float value)
        {
            this.value = value;
        }

        public static CameraFieldOfView FromDegrees(float valueInDegrees)
        {
            return new CameraFieldOfView(MathF.PI * valueInDegrees / 180.0f);
        }
    }
}
