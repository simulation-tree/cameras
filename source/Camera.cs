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

        public readonly ref LayerMask RenderMask => ref viewport.RenderMask;

        public readonly ref float FieldOfView
        {
            get
            {
                ThrowIfOrthographic();

                return ref viewport.AsEntity().GetComponent<CameraSettings>().size;
            }
        }

        public readonly ref float OrthographicSize
        {
            get
            {
                ThrowIfPerspective();

                return ref viewport.AsEntity().GetComponent<CameraSettings>().size;
            }
        }

        public readonly bool IsOrthographic => viewport.AsEntity().GetComponent<CameraSettings>().orthographic;
        public readonly bool IsPerspective => !IsOrthographic;

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

        public Camera(World world, Destination destination, bool orthographic, float size, LayerMask renderMask, float minDepth = 0.1f, float maxDepth = 1000f)
        {
            viewport = new(world, destination, renderMask);
            viewport.AddComponent(new CameraSettings(size, orthographic));
            viewport.AddComponent(new IsCamera(minDepth, maxDepth));
        }

        public Camera(World world, Destination destination, CameraSettings settings, LayerMask renderMask, float minDepth = 0.1f, float maxDepth = 1000f) :
            this(world, destination, settings.orthographic, settings.size, renderMask, minDepth, maxDepth)
        {
        }

        public Camera(World world, Destination destination, CameraSettings settings, float minDepth = 0.1f, float maxDepth = 1000f) :
            this(world, destination, settings.orthographic, settings.size, LayerMask.All, minDepth, maxDepth)
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

        public static Camera CreatePerspectiveFromDegrees(World world, Destination destination, float fieldOfView, LayerMask renderMask, float minDepth = 0.1f, float maxDepth = 1000f)
        {
            return new(world, destination, CameraSettings.PerspectiveFromDegrees(fieldOfView), renderMask, minDepth, maxDepth);
        }

        public static Camera CreatePerspectiveFromRadians(World world, Destination destination, float fieldOfView, LayerMask renderMask, float minDepth = 0.1f, float maxDepth = 1000f)
        {
            return new(world, destination, CameraSettings.PerspectiveFromRadians(fieldOfView), renderMask, minDepth, maxDepth);
        }

        public static Camera CreateOrthographic(World world, Destination destination, float orthographicSize, LayerMask renderMask, float minDepth = 0.1f, float maxDepth = 1000f)
        {
            return new(world, destination, CameraSettings.Orthographic(orthographicSize), renderMask, minDepth, maxDepth);
        }

        public static Camera CreatePerspectiveFromDegrees(World world, Destination destination, float fieldOfView, float minDepth = 0.1f, float maxDepth = 1000f)
        {
            return new(world, destination, CameraSettings.PerspectiveFromDegrees(fieldOfView), LayerMask.All, minDepth, maxDepth);
        }

        public static Camera CreatePerspectiveFromRadians(World world, Destination destination, float fieldOfView, float minDepth = 0.1f, float maxDepth = 1000f)
        {
            return new(world, destination, CameraSettings.PerspectiveFromRadians(fieldOfView), LayerMask.All, minDepth, maxDepth);
        }

        public static Camera CreateOrthographic(World world, Destination destination, float orthographicSize, float minDepth = 0.1f, float maxDepth = 1000f)
        {
            return new(world, destination, CameraSettings.Orthographic(orthographicSize), LayerMask.All, minDepth, maxDepth);
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