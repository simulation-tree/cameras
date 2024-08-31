using Cameras.Components;
using Rendering;
using Simulation;

public static class CameraExtensions
{
    public static CameraProjection GetProjection(this Camera camera)
    {
        Entity cameraEntity = camera;
        CameraProjection component = cameraEntity.GetComponent<CameraProjection>();
        return component;
    }
}