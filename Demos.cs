using RayTracer.Materials;
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
            scene.Objects.Add(new Sphere("Red Ball", new Vector3(0, 0, 3), 1.0f, new RedMaterial()));
            scene.Objects.Add(new Sphere("Blue Ball", new Vector3(-2, 0, 4), 1.0f, new BlueMaterial()));
            scene.Objects.Add(new Sphere("Green Ball", new Vector3(2, 0, 4), 1.0f, new GreenMaterial()));

            // Ground plane (large sphere)
            scene.Objects.Add(new Sphere("Ground Ball", new Vector3(0, -1001, 0), 1000f,
                new BaseMaterial(new Vector3(0.5f, 0.5f, 0.5f), 0.8f, 0.2f, 10)));

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
            scene.Objects.Add(new Sphere("Red Ball",new Vector3(0, 0, 3), 1.0f, new RedMaterial()));
            scene.Objects.Add(new Sphere("Blue Ball", new Vector3(-2, 0, 4), 1.0f, new BlueMaterial()));
            scene.Objects.Add(new Sphere("Green Ball", new Vector3(2, 0, 4), 1.0f, new GreenMaterial()));

            scene.Objects.Add(new Box("Ground", new Vector3(0, -600f, 0), new Vector3(1000f, 1000f, 1000f),
                new BaseMaterial(new Vector3(1, 0.2f, 0.2f), 0.8f, 0.2f, 50)));

            // Add lights
            scene.Lights.Add(new Light(new Vector3(-5, 5, -3), new Vector3(1, 1, 1), 1.0f));
            scene.Lights.Add(new Light(new Vector3(5, 3, -1), new Vector3(0.8f, 0.8f, 1), 0.7f));

            // Add camera
            scene.Camera = new Camera(new Vector3(0, 0, -5), Vector3.UnitZ, Vector3.UnitY, 60);

            await Engine.RenderAsync(scene, width, height, maxDepth, samples);
        }
    
        public static async Task MovingLight(int width, int height, int maxDepth, int samples)
        {
            // Move lights around
            int idx = 0;

            for (decimal x = -5; x < 5; x += 0.1m)
            {
                idx++;
                string name = String.Format($"moving_light_{idx:D3}.png");
                var scene = new Scene(name);

                // Add spheres with different materials
                scene.Objects.Add(new Sphere("Red Ball", new Vector3(0, 0, 3), 1.0f, new RedMaterial()));
                scene.Objects.Add(new Sphere("Blue Ball", new Vector3(-2, 0, 4), 1.0f, new BlueMaterial()));
                scene.Objects.Add(new Sphere("Green Ball", new Vector3(2, 0, 4), 1.0f, new GreenMaterial()));

                scene.Objects.Add(new Box("Ground", new Vector3(0, -600f, 0), new Vector3(1000f, 1000f, 1000f),
                    new BaseMaterial(new Vector3(1, 0.2f, 0.2f), 0.8f, 0.2f, 50)));

                // Add lights
                scene.Lights.Add(new Light(new Vector3((float)x, 5, -3), new Vector3(1, 1, 1), 1.0f));

                // Add camera
                scene.Camera = new Camera(new Vector3(0, 0, -5), Vector3.UnitZ, Vector3.UnitY, 60);

                await Engine.RenderAsync(scene, width, height, maxDepth, samples);
            }
        }
    }
}
