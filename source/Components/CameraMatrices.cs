using System;
using System.Numerics;
using Worlds;

namespace Cameras.Components
{
    [Component]
    public struct CameraMatrices
    {
        public Matrix4x4 projection;
        public Matrix4x4 view;

        public readonly Vector3 Position
        {
            get
            {
                Matrix4x4.Invert(view, out Matrix4x4 worldTransform);
                return new Vector3(worldTransform.M41, worldTransform.M42, worldTransform.M43);
            }
        }

        public readonly Quaternion Rotation
        {
            get
            {
                Matrix4x4.Invert(view, out Matrix4x4 worldTransform);
                Matrix4x4 rotationMatrix = new(worldTransform.M11, worldTransform.M12, worldTransform.M13, 0f,
                    worldTransform.M21, worldTransform.M22, worldTransform.M23, 0f,
                    worldTransform.M31, worldTransform.M32, worldTransform.M33, 0f,
                    0f, 0f, 0f, 1f);
                return Quaternion.CreateFromRotationMatrix(rotationMatrix);
            }
        }

        public readonly Vector3 Right
        {
            get
            {
                Matrix4x4.Invert(view, out Matrix4x4 worldTransform);
                return new Vector3(worldTransform.M11, worldTransform.M12, worldTransform.M13);
            }
        }

        public readonly Vector3 Up
        {
            get
            {
                Matrix4x4.Invert(view, out Matrix4x4 worldTransform);
                return new Vector3(worldTransform.M21, worldTransform.M22, worldTransform.M23);
            }
        }

        public readonly Vector3 Forward
        {
            get
            {
                Matrix4x4.Invert(view, out Matrix4x4 worldTransform);
                return new Vector3(-worldTransform.M31, -worldTransform.M32, -worldTransform.M33);
            }
        }

        public readonly float FieldOfView
        {
            get
            {
                float yScale = 1f / projection.M22;
                return MathF.Atan(1f / yScale) * 2f;
            }
        }

        public readonly float NearClipPlane
        {
            get
            {
                return projection.M43 / (projection.M33 - 1f);
            }
        }

        public readonly float FarClipPlane
        {
            get
            {
                return projection.M43 / (projection.M33 + 1f);
            }
        }

        public readonly float AspectRatio
        {
            get
            {
                return projection.M22 / projection.M11;
            }
        }

        public CameraMatrices(Matrix4x4 projection, Matrix4x4 view)
        {
            this.projection = projection;
            this.view = view;
        }

        /// <summary>
        /// Calculates raycast inputs from the given positon on the destination surface.
        /// </summary>
        public readonly (Vector3 origin, Vector3 direction) GetRayFromScreenPoint(Vector2 screenPoint)
        {
            Vector2 normalizedMousePosition = screenPoint;
            normalizedMousePosition.X = normalizedMousePosition.X * 2 - 1;
            normalizedMousePosition.Y = normalizedMousePosition.Y * 2 - 1;

            Matrix4x4.Invert(projection, out Matrix4x4 invProjection);
            Matrix4x4.Invert(view, out Matrix4x4 invView);
            Vector4 nearPointNDC = new(normalizedMousePosition, 0f, 1f);
            Vector4 farPointNDC = new(normalizedMousePosition, 1f, 1f);
            Vector4 nearPointView = Vector4.Transform(nearPointNDC, invProjection);
            Vector4 farPointView = Vector4.Transform(farPointNDC, invProjection);
            nearPointView /= nearPointView.W;
            farPointView /= farPointView.W;
            Vector4 nearPointWorld = Vector4.Transform(nearPointView, invView);
            Vector4 farPointWorld = Vector4.Transform(farPointView, invView);
            Vector3 origin = new(nearPointWorld.X, nearPointWorld.Y, nearPointWorld.Z);
            Vector3 direction = Vector3.Normalize(new Vector3(farPointWorld.X, farPointWorld.Y, farPointWorld.Z) - origin);
            return (origin, direction);
        }

        /// <summary>
        /// Calculates position in world space from the given screen point,
        /// with an optional distance parameter.
        /// </summary>
        public readonly Vector3 GetWorldPositionFromScreenPoint(Vector2 screenPoint, float distance = 0f)
        {
            Matrix4x4.Invert(projection, out Matrix4x4 invProjection);
            Matrix4x4.Invert(view, out Matrix4x4 invView);
            Vector4 nearPointNDC = new(screenPoint, 0f, 1f);
            Vector4 farPointNDC = new(screenPoint, 1f, 1f);
            Vector4 nearPointView = Vector4.Transform(nearPointNDC, invProjection);
            Vector4 farPointView = Vector4.Transform(farPointNDC, invProjection);
            nearPointView /= nearPointView.W;
            farPointView /= farPointView.W;
            Vector4 nearPointWorld = Vector4.Transform(nearPointView, invView);
            Vector4 farPointWorld = Vector4.Transform(farPointView, invView);
            Vector3 origin = new(nearPointWorld.X, nearPointWorld.Y, nearPointWorld.Z);
            Vector3 direction = Vector3.Normalize(new Vector3(farPointWorld.X, farPointWorld.Y, farPointWorld.Z) - origin);
            return origin + direction * distance;
        }

        public static implicit operator (Matrix4x4 projection, Matrix4x4 view)(CameraMatrices cameraProjection)
        {
            return (cameraProjection.projection, cameraProjection.view);
        }
    }
}