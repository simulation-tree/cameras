using Cameras.Components;
using Rendering.Components;
using Simulation;
using System;
using System.Numerics;
using Transforms.Components;
using Worlds;

namespace Cameras.Systems
{
    public readonly partial struct CameraSystem : ISystem
    {
        void ISystem.Start(in SystemContainer systemContainer, in World world)
        {
        }

        void ISystem.Update(in SystemContainer systemContainer, in World world, in TimeSpan delta)
        {
            using Operation operation = new();
            ComponentQuery<IsCamera> camerasWithoutMatricesQuery = new(world, ComponentType.GetBitSet<CameraMatrices>());
            foreach (var r in camerasWithoutMatricesQuery)
            {
                operation.SelectEntity(r.entity);
            }

            if (operation.Count > 0)
            {
                operation.AddComponent<CameraMatrices>();
                world.Perform(operation);
            }

            ComponentQuery<IsCamera, CameraMatrices, IsViewport> cameraQuery = new(world);
            foreach (var r in cameraQuery)
            {
                CalculateProjection(world, r.entity, ref r.component1, ref r.component2, ref r.component3);
            }
        }

        void ISystem.Finish(in SystemContainer systemContainer, in World world)
        {
        }

        void IDisposable. Dispose()
        {
        }

        //todo: efficiency: split this into two separate queries, one for perspective cameras and the other
        //for orthographic cameras
        private readonly void CalculateProjection(World world, uint entity, ref IsCamera camera, ref CameraMatrices matrices, ref IsViewport viewport)
        {
            //destination may be gone if a window is destroyed
            uint destinationEntity = default;
            destinationEntity = world.GetReference(entity, viewport.destinationReference);
            if (destinationEntity == default || !world.ContainsEntity(destinationEntity))
            {
                return;
            }

            LocalToWorld ltw = world.GetComponent(entity, LocalToWorld.Default);
            (Vector3 position, Quaternion rotation, Vector3 scale) = ltw.Decomposed;
            Vector3 forward = ltw.Forward;
            Vector3 up = ltw.Up;
            Vector3 target = position + forward;
            Matrix4x4 view = Matrix4x4.CreateLookAt(position, target, up);
            Matrix4x4 projection = Matrix4x4.Identity;
            ref CameraOrthographicSize orthographicSize = ref world.TryGetComponent<CameraOrthographicSize>(entity, out bool isOrtho);
            if (isOrtho)
            {
                if (world.ContainsComponent<CameraFieldOfView>(entity))
                {
                    throw new InvalidOperationException($"Camera cannot have both `{nameof(CameraOrthographicSize)}` and `{nameof(CameraFieldOfView)}` components");
                }

                Vector2 size = world.GetComponent<IsDestination>(destinationEntity).SizeAsVector2();
                (float min, float max) = camera.Depth;
                projection = Matrix4x4.CreateOrthographicOffCenter(0, orthographicSize.value * size.X, 0, orthographicSize.value * size.Y, min + 0.1f, max);
                projection.M43 += 0.1f;
                view = Matrix4x4.CreateTranslation(-position);
            }
            else
            {
                ref CameraFieldOfView fov = ref world.TryGetComponent<CameraFieldOfView>(entity, out bool isPerspective);
                if (isPerspective)
                {
                    if (world.ContainsComponent<CameraOrthographicSize>(entity))
                    {
                        throw new InvalidOperationException($"Camera cannot have both `{nameof(CameraOrthographicSize)}` and `{nameof(CameraFieldOfView)}` components");
                    }

                    float aspect = world.GetComponent<IsDestination>(destinationEntity).AspectRatio;
                    (float min, float max) = camera.Depth;
                    projection = Matrix4x4.CreatePerspectiveFieldOfView(fov.value, aspect, min + 0.1f, max);
                    projection.M43 += 0.1f;
                    projection.M11 *= -1; //flip x axis
                }
                else
                {
                    throw new InvalidOperationException($"Camera does not have either a `{nameof(CameraOrthographicSize)}` or a `{nameof(CameraFieldOfView)}` component");
                }
            }

            matrices = new(projection, view);
        }
    }
}
