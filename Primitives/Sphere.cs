using System.Numerics;

namespace RayTracer.Primitives
{
    public class Sphere(string name, Vector3 center, float radius, Material material) : SceneObject(name, material)
    {
        private readonly Vector3 _center = center;
        private readonly float _radius = radius;

        public override HitInfo? Intersect(Ray ray)
        {
            var oc = ray.Origin - _center;
            var a = Vector3.Dot(ray.Direction, ray.Direction);
            var b = 2.0f * Vector3.Dot(oc, ray.Direction);
            var c = Vector3.Dot(oc, oc) - _radius * _radius;
            var discriminant = b * b - 4 * a * c;

            if (discriminant < 0)
                return null;

            var sqrtDiscriminant = MathF.Sqrt(discriminant);
            var t1 = (-b - sqrtDiscriminant) / (2 * a);
            var t2 = (-b + sqrtDiscriminant) / (2 * a);
            var t = t1 > 0.001f ? t1 : t2;

            if (t <= 0.001f)
                return null;

            var point = ray.PointAt(t);
            var normal = Vector3.Normalize(point - _center);

            return new HitInfo(point, normal, t, Material, ray);
        }
    }
}
