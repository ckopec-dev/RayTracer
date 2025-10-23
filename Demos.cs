using RayTracer.Primitives;
using System.Numerics;

namespace RayTracer
{
    internal class Demos
    {
        public static Scene ThreeSpheres()
        {
            var scene = new Scene();

            // Add spheres with different materials
            scene.Objects.Add(new Sphere(new Vector3(0, 0, 3), 1.0f,
                new Material(new Vector3(1, 0.2f, 0.2f), 0.8f, 0.2f, 50)));

            scene.Objects.Add(new Sphere(new Vector3(-2, 0, 4), 1.0f,
                new Material(new Vector3(0.2f, 1, 0.2f), 0.6f, 0.4f, 100)));

            scene.Objects.Add(new Sphere(new Vector3(2, 0, 4), 1.0f,
                new Material(new Vector3(0.2f, 0.2f, 1), 0.9f, 0.1f, 25)));

            // Ground plane (large sphere)
            scene.Objects.Add(new Sphere(new Vector3(0, -1001, 0), 1000f,
                new Material(new Vector3(0.5f, 0.5f, 0.5f), 0.8f, 0.2f, 10)));

            // Add lights
            scene.Lights.Add(new Light(new Vector3(-5, 5, -3), new Vector3(1, 1, 1), 1.0f));
            scene.Lights.Add(new Light(new Vector3(5, 3, -1), new Vector3(0.8f, 0.8f, 1), 0.7f));

            return scene;
        }
    }
}
