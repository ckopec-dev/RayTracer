using RayTracer.Primitives;

namespace RayTracer;

public class Scene
{
    public List<SceneObject> Objects { get; } = [];
    public List<Light> Lights { get; } = [];
}
