using RayTracer.Primitives;
using System.Numerics;

namespace RayTracer;

public class Scene()
{
    public List<SceneObject> Objects { get; } = [];
    public List<Light> Lights { get; } = [];
    public Camera Camera { get; set; } = new Camera(new Vector3(0, 0, -5), Vector3.UnitZ, Vector3.UnitY, 60);
}
