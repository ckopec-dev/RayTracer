using System.Numerics;

namespace RayTracer.Materials;

internal class GreenMaterial : Material
{
    public GreenMaterial()
    {
        Color = new Vector3(0.2f, 0.2f, 1f);
        Diffuse = 0.9f;
        Reflectivity = 0.1f;
        Shininess = 25;
    }
}
