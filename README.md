# Cameras
Implements projection calculations for cameras.

### Projection data
Extending from the `rendering` project, an extension method is available to fetch
the projection and view matrices from a `Camera`:
```cs
using World world = new World();
Camera mainCamera = new(world, CameraFieldOfView.FromDegrees(90f));
CameraProjection cameraProjection = mainCamera.GetProjection();
(Matrix4x4 projection, Matrix4x4 view) = cameraProjection;
```

### Ray from screen point
When a screen point is available, a ray can be calculated from the camera's perspective:
```cs
Vector2 mousePosition = ...
Vector2 screenPoint = camera.Destination.GetScreenPointFromPosition(mousePosition);
(Vector3 origin, Vector3 direction) = cameraProjection.GetRayFromScreenPoint(screenPoint);
```