
namespace RayTracer.Primitives
{
    public abstract class SceneObject(string name, Material material)
    {
        public string Name { get; set; } = name;
        public Material Material { get; } = material;
        public abstract HitInfo? Intersect(Ray ray);
    }
}
