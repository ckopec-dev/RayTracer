using System.Numerics;

namespace RayTracer;

//public readonly record struct Material(
//    Vector3 Color,
//    float Diffuse,
//    float Reflectivity,
//    float Shininess);

public abstract class Material()
{
    public Vector3 Color { get; set; } 
    public float Diffuse { get; set; }
    public float Reflectivity { get; set; }
    public float Shininess { get; set; }
}