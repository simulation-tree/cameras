using Cameras;
using Cameras.Components;
using System.Diagnostics;
using Worlds;

public static class CameraExtensions
{
    public static CameraMatrices GetMatrices(this Camera camera)
    {
        ThrowIfProjectionIsMissing(camera);
        CameraMatrices component = camera.AsEntity().GetComponentRef<CameraMatrices>();
        return component;
    }

    [Conditional("DEBUG")]
    private static void ThrowIfProjectionIsMissing(Camera camera)
    {
        if (!camera.AsEntity().ContainsComponent<CameraMatrices>())
        {
            throw new System.InvalidOperationException("Camera does not have a CameraProjection component.");
        }
    }
}