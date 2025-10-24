using RayTracer.Primitives;
using System.Numerics;

namespace RayTracer
{
    internal class Demos
    {
        public static async Task ThreeSpheres(int width, int height, int maxDepth, int samples)
        {
            var scene = new Scene("three_spheres.png");

            // Add spheres with different materials
            scene.Objects.Add(new Sphere("Red Ball", new Vector3(0, 0, 3), 1.0f,
                new Material(new Vector3(1, 0.2f, 0.2f), 0.8f, 0.2f, 50)));

            scene.Objects.Add(new Sphere("Blue Ball", new Vector3(-2, 0, 4), 1.0f,
                new Material(new Vector3(0.2f, 1, 0.2f), 0.6f, 0.4f, 100)));

            scene.Objects.Add(new Sphere("Green Ball", new Vector3(2, 0, 4), 1.0f,
                new Material(new Vector3(0.2f, 0.2f, 1), 0.9f, 0.1f, 25)));

            // Ground plane (large sphere)
            scene.Objects.Add(new Sphere("Ground Ball", new Vector3(0, -1001, 0), 1000f,
                new Material(new Vector3(0.5f, 0.5f, 0.5f), 0.8f, 0.2f, 10)));

            // Add lights
            scene.Lights.Add(new Light(new Vector3(-5, 5, -3), new Vector3(1, 1, 1), 1.0f));
            scene.Lights.Add(new Light(new Vector3(5, 3, -1), new Vector3(0.8f, 0.8f, 1), 0.7f));

            // Add camera
            scene.Camera = new Camera(new Vector3(0, 0, -5), Vector3.UnitZ, Vector3.UnitY, 60);

            await Engine.RenderAsync(scene, width, height, maxDepth, samples);
        }

        public static async Task BallsOnSurface(int width, int height, int maxDepth, int samples)
        {
            var scene = new Scene("balls_on_surface.png");

            // Add spheres with different materials
            scene.Objects.Add(new Sphere("Red Ball",new Vector3(0, 0, 3), 1.0f,
                new Material(new Vector3(1, 0.2f, 0.2f), 0.8f, 0.2f, 50)));

            scene.Objects.Add(new Sphere("Blue Ball", new Vector3(-2, 0, 4), 1.0f,
                new Material(new Vector3(0.2f, 1, 0.2f), 0.6f, 0.4f, 100)));

            scene.Objects.Add(new Sphere("Green Ball", new Vector3(2, 0, 4), 1.0f,
                new Material(new Vector3(0.2f, 0.2f, 1), 0.9f, 0.1f, 25)));

            scene.Objects.Add(new Box("Ground", new Vector3(0, -600f, 0), new Vector3(1000f, 1000f, 1000f),
                new Material(new Vector3(1, 0.2f, 0.2f), 0.8f, 0.2f, 50)));

            // Add lights
            scene.Lights.Add(new Light(new Vector3(-5, 5, -3), new Vector3(1, 1, 1), 1.0f));
            scene.Lights.Add(new Light(new Vector3(5, 3, -1), new Vector3(0.8f, 0.8f, 1), 0.7f));

            // Add camera
            scene.Camera = new Camera(new Vector3(0, 0, -5), Vector3.UnitZ, Vector3.UnitY, 60);

            await Engine.RenderAsync(scene, width, height, maxDepth, samples);
        }
    }
}
