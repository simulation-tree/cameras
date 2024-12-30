using Cameras.Components;
using Collections;
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
        private readonly Operation operation;
        private readonly List<uint> perspectiveCameras;

        public CameraSystem()
        {
            operation = new();
            perspectiveCameras = new();
        }

        void ISystem.Start(in SystemContainer systemContainer, in World world)
        {
            if (systemContainer.World == world)
            {
                systemContainer.Write(new CameraSystem());
            }
        }

        void ISystem.Update(in SystemContainer systemContainer, in World world, in TimeSpan delta)
        {
            ComponentQuery<IsCamera> camerasWithoutMatricesQuery = new(world);
            camerasWithoutMatricesQuery.ExcludeComponent<CameraMatrices>();
            foreach (var r in camerasWithoutMatricesQuery)
            {
                operation.SelectEntity(r.entity);
            }

            if (operation.Count > 0)
            {
                operation.AddComponent<CameraMatrices>(world.Schema);
                world.Perform(operation);
                operation.Clear();
            }

            perspectiveCameras.Clear();
            ComponentQuery<IsCamera, CameraMatrices, IsViewport, CameraFieldOfView> perspectiveQuery = new(world);
            foreach (var r in perspectiveQuery)
            {
                CalculatePerspective(world, r);
                perspectiveCameras.Add(r.entity);
            }

            ComponentQuery<IsCamera, CameraMatrices, IsViewport, CameraOrthographicSize> orthographicQuery = new(world);
            foreach (var r in orthographicQuery)
            {
                if (perspectiveCameras.Contains(r.entity))
                {
                    throw new InvalidOperationException($"Camera `{r.entity}` cannot have both `{nameof(CameraFieldOfView)}` and `{nameof(CameraOrthographicSize)}` components");
                }

                CalculateOrthographic(world, r);
            }
        }

        private static void CalculatePerspective(World world, Chunk.Entity<IsCamera, CameraMatrices, IsViewport, CameraFieldOfView> r)
        {
            uint destinationEntity = world.GetReference(r.entity, r.component3.destinationReference);
            if (destinationEntity == default || !world.ContainsEntity(destinationEntity))
            {
                return;
            }

            LocalToWorld ltw = world.GetComponent(r.entity, LocalToWorld.Default);
            (Vector3 position, Quaternion rotation, Vector3 scale) = ltw.Decomposed;
            Vector3 forward = ltw.Forward;
            Vector3 up = ltw.Up;
            Vector3 target = position + forward;
            Matrix4x4 view = Matrix4x4.CreateLookAt(position, target, up);
            Matrix4x4 projection = Matrix4x4.Identity;
            ref CameraFieldOfView fov = ref r.component4;
            float aspect = world.GetComponent<IsDestination>(destinationEntity).AspectRatio;
            (float min, float max) = r.component1.Depth;
            projection = Matrix4x4.CreatePerspectiveFieldOfView(fov.value, aspect, min + 0.1f, max);
            projection.M43 += 0.1f;
            projection.M11 *= -1; //flip x axis
            r.component2 = new(projection, view);
        }

        private readonly void CalculateOrthographic(World world, Chunk.Entity<IsCamera, CameraMatrices, IsViewport, CameraOrthographicSize> r)
        {
            uint destinationEntity = world.GetReference(r.entity, r.component3.destinationReference);
            if (destinationEntity == default || !world.ContainsEntity(destinationEntity))
            {
                return;
            }

            LocalToWorld ltw = world.GetComponent(r.entity, LocalToWorld.Default);
            (Vector3 position, Quaternion rotation, Vector3 scale) = ltw.Decomposed;
            Vector3 forward = ltw.Forward;
            Vector3 up = ltw.Up;
            Vector3 target = position + forward;
            Matrix4x4 view = Matrix4x4.CreateLookAt(position, target, up);
            Matrix4x4 projection = Matrix4x4.Identity;
            ref CameraOrthographicSize orthographicSize = ref r.component4;
            Vector2 size = world.GetComponent<IsDestination>(destinationEntity).SizeAsVector2();
            (float min, float max) = r.component1.Depth;
            projection = Matrix4x4.CreateOrthographicOffCenter(0, orthographicSize.value * size.X, 0, orthographicSize.value * size.Y, min + 0.1f, max);
            projection.M43 += 0.1f;
            view = Matrix4x4.CreateTranslation(-position);
            r.component2 = new(projection, view);
        }

        void ISystem.Finish(in SystemContainer systemContainer, in World world)
        {
            if (systemContainer.World == world)
            {
                perspectiveCameras.Dispose();
                operation.Dispose();
            }
        }
    }
}
