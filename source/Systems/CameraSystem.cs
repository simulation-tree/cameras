using Cameras.Components;
using Rendering.Components;
using Rendering.Events;
using Simulation;
using System;
using System.Numerics;
using Transforms.Components;

namespace Cameras.Systems
{
    public class CameraSystem : SystemBase
    {
        private readonly Query<IsCamera> cameraQuery;

        public CameraSystem(World world) : base(world)
        {
            cameraQuery = new(world);
            Subscribe<CameraUpdate>(Update);
        }

        public override void Dispose()
        {
            cameraQuery.Dispose();
            base.Dispose();
        }

        private void Update(CameraUpdate update)
        {
            cameraQuery.Update();
            foreach (var x in cameraQuery)
            {
                uint cameraEntity = x.entity;
                ref CameraProjection projection = ref world.TryGetComponentRef<CameraProjection>(cameraEntity, out bool has);
                if (!has)
                {
                    projection = ref world.AddComponentRef<CameraProjection>(cameraEntity);
                }

                CalculateProjection(cameraEntity, ref projection);
            }
        }

        private void CalculateProjection(uint cameraEntity, ref CameraProjection component)
        {
            if (world.TryGetComponent(cameraEntity, out CameraOutput cameraOutput))
            {
                //destination may be gone if a window is destroyed
                if (cameraOutput.destination == default || !world.ContainsEntity(cameraOutput.destination))
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

                (uint width, uint height) = world.GetComponent<IsDestination>(cameraOutput.destination).Size;
                (float min, float max) = world.GetComponent<IsCamera>(cameraEntity).Depth;
                projection = Matrix4x4.CreateOrthographicOffCenter(0, orthographicSize.value * width, 0, orthographicSize.value * height, -min, max);
                view = Matrix4x4.CreateTranslation(-position);
            }
            else if (world.TryGetComponent(cameraEntity, out CameraFieldOfView fov))
            {
                if (world.ContainsComponent<CameraOrthographicSize>(cameraEntity))
                {
                    throw new InvalidOperationException($"Camera cannot have both {nameof(CameraOrthographicSize)} and {nameof(CameraFieldOfView)} components");
                }

                float aspect = world.GetComponent<IsDestination>(cameraOutput.destination).AspectRatio;
                (float min, float max) = world.GetComponent<IsCamera>(cameraEntity).Depth;
                projection = Matrix4x4.CreatePerspectiveFieldOfView(fov.value, aspect, min, max);
                projection.M11 *= -1; //flip x axis
            }
            else
            {
                throw new InvalidOperationException($"Camera does not have either {nameof(CameraOrthographicSize)} or {nameof(CameraFieldOfView)} component");
            }

            component = new(projection, view);
        }
    }
}
