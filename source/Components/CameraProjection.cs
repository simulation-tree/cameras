using System.Numerics;

namespace Cameras.Components
{
    public struct CameraProjection
    {
        public Matrix4x4 projection;
        public Matrix4x4 view;

        public CameraProjection(Matrix4x4 projection, Matrix4x4 view)
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

        public static implicit operator (Matrix4x4 projection, Matrix4x4 view)(CameraProjection cameraProjection)
        {
            return (cameraProjection.projection, cameraProjection.view);
        }
    }
}