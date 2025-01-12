using Cameras.Components;
using Rendering;
using Rendering.Components;
using System;
using System.Diagnostics;
using Worlds;

namespace Cameras
{
    public readonly struct Camera : ICamera
    {
        private readonly Viewport viewport;

        public readonly (float min, float max) Depth
        {
            get
            {
                IsCamera component = viewport.AsEntity().GetComponent<IsCamera>();
                return (component.minDepth, component.maxDepth);
            }
            set
            {
                ref IsCamera component = ref viewport.AsEntity().GetComponent<IsCamera>();
                component = new(value.min, value.max);
            }
        }

        public readonly ref uint Mask => ref viewport.Mask;

        public readonly ref float FieldOfView
        {
            get
            {
                ThrowIfOrthographic();
                return ref viewport.AsEntity().GetComponent<CameraFieldOfView>().value;
            }
        }

        public readonly ref float OrthographicSize
        {
            get
            {
                ThrowIfPerspective();
                return ref viewport.AsEntity().GetComponent<CameraOrthographicSize>().value;
            }
        }

        public readonly bool IsOrthographic => viewport.AsEntity().ContainsComponent<CameraOrthographicSize>();
        public readonly bool IsPerspective => viewport.AsEntity().ContainsComponent<CameraFieldOfView>();

        public readonly Destination Destination
        {
            get => viewport.Destination;
            set => viewport.Destination = value;
        }

        readonly uint IEntity.Value => viewport.GetEntityValue();
        readonly World IEntity.World => viewport.GetWorld();

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsViewport>();
            archetype.AddComponentType<IsCamera>();
        }

#if NET
        [Obsolete("Default constructor not available", true)]
        public Camera()
        {
            throw new InvalidOperationException("Cannot create a camera without a world.");
        }
#endif

        public Camera(World world, uint existingEntity)
        {
            viewport = new(world, existingEntity);
        }

        public Camera(World world, Destination destination, bool isOrthographic, float size, float minDepth = 0.1f, float maxDepth = 1000f, uint mask = uint.MaxValue)
        {
            viewport = new(world, destination, mask);
            if (isOrthographic)
            {
                viewport.AddComponent(new CameraOrthographicSize(size));
            }
            else
            {
                viewport.AddComponent(new CameraFieldOfView(size));
            }

            viewport.AddComponent(new IsCamera(minDepth, maxDepth));
        }

        public Camera(World world, Destination destination, CameraFieldOfView fieldOfView, float minDepth = 0.1f, float maxDepth = 1000f, uint mask = uint.MaxValue) :
            this(world, destination, false, fieldOfView.value, minDepth, maxDepth, mask)
        {
        }

        public Camera(World world, Destination destination, CameraOrthographicSize orthographicSize, float minDepth = 0.1f, float maxDepth = 1000f, uint mask = uint.MaxValue) :
            this(world, destination, true, orthographicSize.value, minDepth, maxDepth, mask)
        {

        }

        public readonly void Dispose()
        {
            viewport.Dispose();
        }

        public readonly override string ToString()
        {
            return viewport.ToString();
        }

        [Conditional("DEBUG")]
        private readonly void ThrowIfOrthographic()
        {
            if (IsOrthographic)
            {
                throw new InvalidOperationException($"Cannot get field of view for orthographic camera `{viewport}`");
            }
        }

        [Conditional("DEBUG")]
        private readonly void ThrowIfPerspective()
        {
            if (IsPerspective)
            {
                throw new InvalidOperationException($"Cannot get orthographic size for a perspective camera `{viewport}`");
            }
        }

        public static Camera CreatePerspective(World world, Destination destination, float fieldOfView, float minDepth = 0.1f, float maxDepth = 1000f, uint mask = uint.MaxValue)
        {
            return new(world, destination, new CameraFieldOfView(fieldOfView), minDepth, maxDepth, mask);
        }

        public static Camera CreateOrthographic(World world, Destination destination, float orthographicSize, float minDepth = 0.1f, float maxDepth = 1000f, uint mask = uint.MaxValue)
        {
            return new(world, destination, new CameraOrthographicSize(orthographicSize), minDepth, maxDepth, mask);
        }

        public static implicit operator Entity(Camera camera)
        {
            return camera.viewport;
        }

        public static implicit operator Viewport(Camera camera)
        {
            return camera.viewport;
        }
    }
}