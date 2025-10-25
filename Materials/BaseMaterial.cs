
using System.Numerics;

namespace RayTracer.Materials
{
    public class BaseMaterial : Material
    {
        public BaseMaterial(Vector3 color, float diffuse, float reflectivity, float shininess)
        {
            Color = color;
            Diffuse = diffuse;
            Reflectivity = reflectivity;
            Shininess = shininess;
        }
    }
}
