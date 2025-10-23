using System.Numerics;

namespace RayTracer.Primitives
{
    public class Triangle : SceneObject
    {
        private readonly Vector3 _v0, _v1, _v2;
        private readonly Vector3 _normal;

        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2, Material material) : base(material)
        {
            _v0 = v0;
            _v1 = v1;
            _v2 = v2;

            // Calculate normal using cross product
            var edge1 = _v1 - _v0;
            var edge2 = _v2 - _v0;
            _normal = Vector3.Normalize(Vector3.Cross(edge1, edge2));
        }

        public override HitInfo? Intersect(Ray ray)
        {
            // Möller-Trumbore intersection algorithm
            var edge1 = _v1 - _v0;
            var edge2 = _v2 - _v0;

            var h = Vector3.Cross(ray.Direction, edge2);
            var a = Vector3.Dot(edge1, h);

            if (MathF.Abs(a) < 0.0001f)
                return null; // Ray parallel to triangle

            var f = 1.0f / a;
            var s = ray.Origin - _v0;
            var u = f * Vector3.Dot(s, h);

            if (u < 0.0f || u > 1.0f)
                return null;

            var q = Vector3.Cross(s, edge1);
            var v = f * Vector3.Dot(ray.Direction, q);

            if (v < 0.0f || u + v > 1.0f)
                return null;

            var t = f * Vector3.Dot(edge2, q);

            if (t <= 0.001f)
                return null;

            var point = ray.PointAt(t);
            return new HitInfo(point, _normal, t, Material, ray);
        }
    }
}
