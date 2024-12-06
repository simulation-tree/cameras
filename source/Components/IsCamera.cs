using Worlds;

namespace Cameras.Components
{
    [Component]
    public struct IsCamera
    {
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

        public IsCamera(float minDepth, float maxDepth)
        {
            this.minDepth = minDepth;
            this.maxDepth = maxDepth;
        }
    }
}
