using System.Numerics;

namespace RayTracer;

public readonly record struct Light(
    Vector3 Position,
    Vector3 Color,
    float Intensity);
