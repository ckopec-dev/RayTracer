using System.Numerics;

namespace RayTracer.Primitives
{
    public class Box(Vector3 center, Vector3 size, Material material) : SceneObject(material)
    {
        private readonly Vector3 _center = center;

        public override HitInfo? Intersect(Ray ray)
        {
            var min = _center - size * 0.5f;
            var max = _center + size * 0.5f;

            var invDir = new Vector3(1.0f / ray.Direction.X, 1.0f / ray.Direction.Y, 1.0f / ray.Direction.Z);

            var t1 = (min - ray.Origin) * invDir;
            var t2 = (max - ray.Origin) * invDir;

            var tMin = Vector3.Min(t1, t2);
            var tMax = Vector3.Max(t1, t2);

            var tNear = MathF.Max(MathF.Max(tMin.X, tMin.Y), tMin.Z);
            var tFar = MathF.Min(MathF.Min(tMax.X, tMax.Y), tMax.Z);

            if (tNear > tFar || tFar < 0.001f)
                return null;

            var t = tNear > 0.001f ? tNear : tFar;
            var point = ray.PointAt(t);

            // Calculate normal based on which face was hit
            //_ = Vector3.Zero;
            var center = point - _center;
            var absCenter = Vector3.Abs(center);
            Vector3 normal;
            if (absCenter.X > absCenter.Y && absCenter.X > absCenter.Z)
                normal = new Vector3(MathF.Sign(center.X), 0, 0);
            else if (absCenter.Y > absCenter.Z)
                normal = new Vector3(0, MathF.Sign(center.Y), 0);
            else
                normal = new Vector3(0, 0, MathF.Sign(center.Z));

            return new HitInfo(point, normal, t, Material, ray);
        }
    }
}
