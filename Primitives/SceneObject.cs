
namespace RayTracer.Primitives
{
    public abstract class SceneObject(Material material)
    {
        public Material Material { get; } = material;
        public abstract HitInfo? Intersect(Ray ray);
    }
}
