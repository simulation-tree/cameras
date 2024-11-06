namespace Cameras.Components
{
    public struct IsCamera
    {
        public float minDepth;
        public float maxDepth;
        public uint mask;

        public (float min, float max) Depth
        {
            readonly get => (minDepth, maxDepth);
            set
            {
                minDepth = value.min;
                maxDepth = value.max;
            }
        }

        public IsCamera(float minDepth, float maxDepth, uint mask)
        {
            this.minDepth = minDepth;
            this.maxDepth = maxDepth;
            this.mask = mask;
        }
    }
}
