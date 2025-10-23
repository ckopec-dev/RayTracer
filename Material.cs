using System.Numerics;

namespace RayTracer;

public readonly record struct Material(
    Vector3 Color,
    float Diffuse,
    float Reflectivity,
    float Shininess);

