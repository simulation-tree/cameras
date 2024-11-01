using Cameras.Components;
using Rendering.Components;
using Simulation;
using Simulation.Functions;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Transforms.Components;

namespace Cameras.Systems
{
    public readonly struct CameraSystem : ISystem
    {
        private readonly ComponentQuery<IsCamera> cameraQuery;

        readonly unsafe InitializeFunction ISystem.Initialize => new(&Initialize);
        readonly unsafe IterateFunction ISystem.Iterate => new(&Update);
        readonly unsafe FinalizeFunction ISystem.Finalize => new(&Finalize);

        [UnmanagedCallersOnly]
        private static void Initialize(SystemContainer container, World world)
        {
        }

        [UnmanagedCallersOnly]
        private static void Update(SystemContainer container, World world, TimeSpan delta)
        {
            ref CameraSystem system = ref container.Read<CameraSystem>();
            system.Update(world);
        }

        [UnmanagedCallersOnly]
        private static void Finalize(SystemContainer container, World world)
        {
            if (container.World == world)
            {
                ref CameraSystem system = ref container.Read<CameraSystem>();
                system.CleanUp();
            }
        }

        public CameraSystem()
        {
            cameraQuery = new();
        }

        private void CleanUp()
        {
            cameraQuery.Dispose();
        }

        private void Update(World world)
        {
            cameraQuery.Update(world);
            foreach (var x in cameraQuery)
            {
                uint cameraEntity = x.entity;
                ref CameraMatrices matrices = ref world.TryGetComponentRef<CameraMatrices>(cameraEntity, out bool has);
                if (!has)
                {
                    matrices = ref world.AddComponentRef<CameraMatrices>(cameraEntity);
                }

                CalculateProjection(world, cameraEntity, ref matrices);
            }
        }

        private void CalculateProjection(World world, uint cameraEntity, ref CameraMatrices matrices)
        {
            uint destinationEntity = default;
            if (world.TryGetComponent(cameraEntity, out CameraOutput cameraOutput))
            {
                //destination may be gone if a window is destroyed
                destinationEntity = world.GetReference(cameraEntity, cameraOutput.destinationReference);
                if (destinationEntity == default || !world.ContainsEntity(destinationEntity))
                {
                    return;
                }
            }
            else
            {
                return;
            }

            LocalToWorld ltw = world.GetComponent(cameraEntity, LocalToWorld.Default);
            (Vector3 position, Quaternion rotation, Vector3 scale) = ltw.Decomposed;
            Vector3 forward = ltw.Forward;
            Vector3 up = ltw.Up;
            Vector3 target = position + forward;
            Matrix4x4 view = Matrix4x4.CreateLookAt(position, target, up);
            Matrix4x4 projection = Matrix4x4.Identity;
            if (world.TryGetComponent(cameraEntity, out CameraOrthographicSize orthographicSize))
            {
                if (world.ContainsComponent<CameraFieldOfView>(cameraEntity))
                {
                    throw new InvalidOperationException($"Camera cannot have both {nameof(CameraOrthographicSize)} and {nameof(CameraFieldOfView)} components");
                }

                Vector2 size = world.GetComponent<IsDestination>(destinationEntity).SizeAsVector2();
                (float min, float max) = world.GetComponent<IsCamera>(cameraEntity).Depth;
                projection = Matrix4x4.CreateOrthographicOffCenter(0, orthographicSize.value * size.X, 0, orthographicSize.value * size.Y, min + 0.1f, max);
                projection.M43 += 0.1f;
                view = Matrix4x4.CreateTranslation(-position);
            }
            else if (world.TryGetComponent(cameraEntity, out CameraFieldOfView fov))
            {
                if (world.ContainsComponent<CameraOrthographicSize>(cameraEntity))
                {
                    throw new InvalidOperationException($"Camera cannot have both {nameof(CameraOrthographicSize)} and {nameof(CameraFieldOfView)} components");
                }

                float aspect = world.GetComponent<IsDestination>(destinationEntity).AspectRatio;
                (float min, float max) = world.GetComponent<IsCamera>(cameraEntity).Depth;
                projection = Matrix4x4.CreatePerspectiveFieldOfView(fov.value, aspect, min + 0.1f, max);
                projection.M43 += 0.1f;
                projection.M11 *= -1; //flip x axis
            }
            else
            {
                throw new InvalidOperationException($"Camera does not have either {nameof(CameraOrthographicSize)} or {nameof(CameraFieldOfView)} component");
            }

            matrices = new(projection, view);
        }
    }
}
