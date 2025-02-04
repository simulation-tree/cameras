using Cameras.Components;
using Rendering;
using Rendering.Components;
using System;
using System.Diagnostics;
using System.Numerics;
using Worlds;

namespace Cameras
{
    public readonly partial struct Camera : IEntity
    {
        public readonly ref Vector4 Region => ref As<Viewport>().Region;
        public readonly ref sbyte Order => ref As<Viewport>().Order;
        public readonly ref LayerMask RenderMask => ref As<Viewport>().RenderMask;

        public readonly (float min, float max) Depth
        {
            get
            {
                CameraSettings component = GetComponent<CameraSettings>();
                return (component.minDepth, component.maxDepth);
            }
            set
            {
                ref CameraSettings component = ref GetComponent<CameraSettings>();
                component.minDepth = value.min;
                component.maxDepth = value.max;
            }
        }

        public readonly ref float FieldOfView
        {
            get
            {
                ThrowIfOrthographic();

                return ref GetComponent<CameraSettings>().size;
            }
        }

        public readonly ref float OrthographicSize
        {
            get
            {
                ThrowIfPerspective();

                return ref GetComponent<CameraSettings>().size;
            }
        }

        public readonly bool IsOrthographic => GetComponent<CameraSettings>().orthographic;
        public readonly bool IsPerspective => !IsOrthographic;

        public readonly Destination Destination
        {
            get => As<Viewport>().Destination;
            set => As<Viewport>().Destination = value;
        }

        public readonly CameraMatrices Matrices
        {
            get
            {
                ThrowIfMatricesMissing();

                return GetComponent<CameraMatrices>();
            }
        }

        public Camera(World world, Destination destination, bool orthographic, float size, LayerMask renderMask, float minDepth, float maxDepth)
        {
            this.world = world;
            value = world.CreateEntity(new IsViewport((rint)1, new Vector4(0, 0, 1, 1), default, renderMask), new CameraSettings(size, orthographic, minDepth, maxDepth));
            AddReference(destination);
        }

        public Camera(World world, Destination destination, CameraSettings settings, LayerMask renderMask, float minDepth = CameraSettings.DefaultMinDepth, float maxDepth = CameraSettings.DefaultMaxDepth) :
            this(world, destination, settings.orthographic, settings.size, renderMask, minDepth, maxDepth)
        {
        }

        public Camera(World world, Destination destination, CameraSettings settings, float minDepth = CameraSettings.DefaultMinDepth, float maxDepth = CameraSettings.DefaultMaxDepth) :
            this(world, destination, settings.orthographic, settings.size, LayerMask.All, minDepth, maxDepth)
        {
        }

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsViewport>();
            archetype.AddComponentType<CameraSettings>();
        }

        public readonly override string ToString()
        {
            return value.ToString();
        }

        [Conditional("DEBUG")]
        private readonly void ThrowIfOrthographic()
        {
            if (IsOrthographic)
            {
                throw new InvalidOperationException($"Cannot get field of view for orthographic camera `{value}`");
            }
        }

        [Conditional("DEBUG")]
        private readonly void ThrowIfPerspective()
        {
            if (IsPerspective)
            {
                throw new InvalidOperationException($"Cannot get orthographic size for a perspective camera `{value}`");
            }
        }

        [Conditional("DEBUG")]
        private readonly void ThrowIfMatricesMissing()
        {
            if (!ContainsComponent<CameraMatrices>())
            {
                throw new InvalidOperationException($"Matrices are missing for camera `{value}`");
            }
        }

        public static Camera CreatePerspectiveDegrees(World world, Destination destination, float fieldOfView, LayerMask renderMask, float minDepth = CameraSettings.DefaultMinDepth, float maxDepth = CameraSettings.DefaultMaxDepth)
        {
            return new(world, destination, CameraSettings.CreatePerspectiveDegrees(fieldOfView), renderMask, minDepth, maxDepth);
        }

        public static Camera CreatePerspectiveRadians(World world, Destination destination, float fieldOfView, LayerMask renderMask, float minDepth = CameraSettings.DefaultMinDepth, float maxDepth = CameraSettings.DefaultMaxDepth)
        {
            return new(world, destination, CameraSettings.CreatePerspectiveRadians(fieldOfView), renderMask, minDepth, maxDepth);
        }

        public static Camera CreateOrthographic(World world, Destination destination, float orthographicSize, LayerMask renderMask, float minDepth = CameraSettings.DefaultMinDepth, float maxDepth = CameraSettings.DefaultMaxDepth)
        {
            return new(world, destination, CameraSettings.CreateOrthographic(orthographicSize), renderMask, minDepth, maxDepth);
        }

        public static Camera CreatePerspectiveDegrees(World world, Destination destination, float fieldOfView, float minDepth = CameraSettings.DefaultMinDepth, float maxDepth = CameraSettings.DefaultMaxDepth)
        {
            return new(world, destination, CameraSettings.CreatePerspectiveDegrees(fieldOfView), LayerMask.All, minDepth, maxDepth);
        }

        public static Camera CreatePerspectiveRadians(World world, Destination destination, float fieldOfView, float minDepth = CameraSettings.DefaultMinDepth, float maxDepth = CameraSettings.DefaultMaxDepth)
        {
            return new(world, destination, CameraSettings.CreatePerspectiveRadians(fieldOfView), LayerMask.All, minDepth, maxDepth);
        }

        public static Camera CreateOrthographic(World world, Destination destination, float orthographicSize, float minDepth = CameraSettings.DefaultMinDepth, float maxDepth = CameraSettings.DefaultMaxDepth)
        {
            return new(world, destination, CameraSettings.CreateOrthographic(orthographicSize), LayerMask.All, minDepth, maxDepth);
        }

        public static implicit operator Viewport(Camera camera)
        {
            return camera.As<Viewport>();
        }
    }
}