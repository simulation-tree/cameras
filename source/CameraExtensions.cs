using Cameras.Components;
using Rendering;
using System.Diagnostics;

public static class CameraExtensions
{
    public static CameraMatrices GetMatrices(this Camera camera)
    {
        ThrowIfProjectionIsMissing(camera);
        CameraMatrices component = camera.entity.GetComponentRef<CameraMatrices>();
        return component;
    }

    [Conditional("DEBUG")]
    private static void ThrowIfProjectionIsMissing(Camera camera)
    {
        if (!camera.entity.ContainsComponent<CameraMatrices>())
        {
            throw new System.InvalidOperationException("Camera does not have a CameraProjection component.");
        }
    }
}