using System.Numerics;

namespace RayTracer.Primitives
{
    public class Plane(string name, Vector3 normal, float distance, Material material) : SceneObject(name, material)
    {
        private readonly Vector3 _normal = Vector3.Normalize(normal);

        public override HitInfo? Intersect(Ray ray)
        {
            var denom = Vector3.Dot(_normal, ray.Direction);
            if (MathF.Abs(denom) < 0.0001f) // Ray parallel to plane
                return null;

            var t = (distance - Vector3.Dot(_normal, ray.Origin)) / denom;
            if (t <= 0.001f)
                return null;

            var point = ray.PointAt(t);
            return new HitInfo(point, _normal, t, Material, ray);
        }
    }
}
