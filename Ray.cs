using System.Numerics;

namespace RayTracer;

public readonly record struct Ray(Vector3 Origin, Vector3 Direction)
{
    public Vector3 PointAt(float t) => Origin + Direction * t;

    public static Ray Create(Vector3 origin, Vector3 direction) =>
        new(origin, Vector3.Normalize(direction));
}
