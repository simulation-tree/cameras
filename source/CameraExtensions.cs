using Cameras.Components;
using Rendering;
using System.Diagnostics;

public static class CameraExtensions
{
    public static CameraProjection GetProjection(this Camera camera)
    {
        ThrowIfProjectionIsMissing(camera);
        CameraProjection component = camera.entity.GetComponentRef<CameraProjection>();
        return component;
    }

    [Conditional("DEBUG")]
    private static void ThrowIfProjectionIsMissing(Camera camera)
    {
        if (!camera.entity.ContainsComponent<CameraProjection>())
        {
            throw new System.InvalidOperationException("Camera does not have a CameraProjection component.");
        }
    }
}