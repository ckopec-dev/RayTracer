using System.Numerics;

namespace RayTracer.Materials;

internal class BlueMaterial : Material
{
    public BlueMaterial()
    {
        Color = new Vector3(0.2f, 1.0f, 0.2f);
        Diffuse = 0.6f;
        Reflectivity = 0.4f;
        Shininess = 100;
    }
}
