using System.Numerics;

namespace RayTracer.Materials;

internal class RedMaterial : Material
{
    public RedMaterial()
    {
        Color = new Vector3(1, 0.2f, 0.2f);
        Diffuse = 0.8f;
        Reflectivity = 0.2f;
        Shininess = 50;
    }
}
