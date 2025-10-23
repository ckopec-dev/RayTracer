using System.Numerics;

namespace RayTracer.Primitives
{
    public class Cylinder(string name, Vector3 center, float radius, float height, Material material) : SceneObject(name, material)
    {
        public override HitInfo? Intersect(Ray ray)
        {
            // Transform to cylinder space (assume cylinder is aligned with Y-axis)
            var oc = ray.Origin - center;

            // Check intersection with infinite cylinder (ignoring Y)
            var a = ray.Direction.X * ray.Direction.X + ray.Direction.Z * ray.Direction.Z;
            var b = 2.0f * (oc.X * ray.Direction.X + oc.Z * ray.Direction.Z);
            var c = oc.X * oc.X + oc.Z * oc.Z - radius * radius;

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
            var y = point.Y - center.Y;

            // Check if intersection is within cylinder height
            if (y < 0 || y > height)
                return null;

            // Calculate normal (pointing outward from cylinder axis)
            var normal = Vector3.Normalize(new Vector3(point.X - center.X, 0, point.Z - center.Z));

            return new HitInfo(point, normal, t, Material, ray);
        }
    }
}
