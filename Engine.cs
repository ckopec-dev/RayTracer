using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;

namespace RayTracer;

internal class Engine
{
    private const float Epsilon = 0.001f;

    public static async Task RenderAsync(Scene scene, int width, int height, int maxDepth, int samples)
    {
        var pixels = new Vector3[width * height];
        var random = new Random();

        Console.WriteLine($"Rendering {width * height:N0} pixels with {samples * width * height:N0} total rays...");
        var startTime = DateTime.Now;

        // Parallel rendering for better performance
        await Task.Run(() =>
        {
            var lockObject = new object();
            var linesCompleted = 0;

            Parallel.For(0, height, () => new Random(), (y, loop, localRandom) =>
            {
                for (int x = 0; x < width; x++)
                {
                    var color = Vector3.Zero;

                    // Anti-aliasing: sample multiple rays per pixel
                    for (int s = 0; s < samples; s++)
                    {
                        float offsetX = samples > 1 ? (float)localRandom.NextDouble() - 0.5f : 0;
                        float offsetY = samples > 1 ? (float)localRandom.NextDouble() - 0.5f : 0;

                        var ray = scene.Camera.GetRay(x + offsetX, y + offsetY, width, height);
                        color += TraceRay(ray, scene, maxDepth);
                    }

                    pixels[y * width + x] = color / samples;
                }

                lock (lockObject)
                {
                    linesCompleted++;
                    if (linesCompleted % Math.Max(1, height / 20) == 0)
                    {
                        var progress = (double)linesCompleted / height * 100;
                        var elapsed = DateTime.Now - startTime;
                        if (progress > 0)
                        {
                            var estimated = TimeSpan.FromTicks((long)(elapsed.Ticks / progress * 100));
                            var eta = estimated - elapsed;
                            Console.WriteLine($"Progress: {progress:F1}% ({linesCompleted}/{height} lines) - " +
                                //$"Estimated render time: {estimated:mm\\:ss} - " +
                                //$"Elapsed: {elapsed:mm\\:ss} - " +
                                $"ETA: {eta:mm\\:ss}"
                                );
                        }
                    }
                }

                return localRandom;
            }, _ => { });
        });

        var totalTime = DateTime.Now - startTime;
        Console.WriteLine($"Rendering completed in {totalTime:mm\\:ss\\.ff}");
        await SaveImageAsync(pixels, scene.Filename, width, height);
        Console.WriteLine($"Image saved as '{scene.Filename}'");
        Console.WriteLine($"File size: {new FileInfo(scene.Filename).Length / 1024.0 / 1024.0:F1} MB");
    }

    private static Vector3 TraceRay(Ray ray, Scene scene, int depth)
    {
        if (depth <= 0)
            return Vector3.Zero;

        var hit = FindClosestHit(ray, scene);
        if (hit is null)
            return new Vector3(0.2f, 0.2f, 0.3f); // Sky color

        var color = CalculateLighting((HitInfo)hit, scene);

        // Add reflection
        if (hit.Value.Material.Reflectivity > 0)
        {
            var reflectDir = Reflect(ray.Direction, hit.Value.Normal);
            var reflectRay = new Ray(hit.Value.Point + hit.Value.Normal * Epsilon, reflectDir);
            var reflectColor = TraceRay(reflectRay, scene, depth - 1);
            color = Vector3.Lerp(color, reflectColor, hit.Value.Material.Reflectivity);
        }

        return Vector3.Clamp(color, Vector3.Zero, Vector3.One);
    }

    private static HitInfo? FindClosestHit(Ray ray, Scene scene)
    {
        HitInfo? closestHit = null;
        float closestDistance = float.MaxValue;

        foreach (var obj in scene.Objects)
        {
            var hit = obj.Intersect(ray);
            if (hit is not null && hit.Value.Distance < closestDistance)
            {
                closestDistance = hit.Value.Distance;
                closestHit = hit;
            }
        }

        return closestHit;
    }

    private static Vector3 CalculateLighting(HitInfo hit, Scene scene)
    {
        var color = Vector3.Zero;

        foreach (var light in scene.Lights)
        {
            var lightDir = Vector3.Normalize(light.Position - hit.Point);
            var shadowRay = new Ray(hit.Point + hit.Normal * Epsilon, lightDir);

            // Check for shadows
            var shadowHit = FindClosestHit(shadowRay, scene);
            var lightDistance = Vector3.Distance(light.Position, hit.Point);

            if (shadowHit is null || shadowHit.Value.Distance > lightDistance)
            {
                // Diffuse lighting
                var diffuse = MathF.Max(0, Vector3.Dot(hit.Normal, lightDir));

                // Specular lighting
                var viewDir = Vector3.Normalize(-hit.Ray.Direction);
                var reflectDir = Reflect(-lightDir, hit.Normal);
                var specular = MathF.Pow(MathF.Max(0, Vector3.Dot(viewDir, reflectDir)), hit.Material.Shininess);

                var lightContrib = light.Color * light.Intensity *
                    (hit.Material.Color * hit.Material.Diffuse * diffuse +
                     Vector3.One * (1 - hit.Material.Diffuse) * specular);

                color += lightContrib;
            }
        }

        // Ambient lighting
        color += hit.Material.Color * 0.1f;

        return color;
    }

    private static Vector3 Reflect(Vector3 incident, Vector3 normal) =>
        incident - 2 * Vector3.Dot(incident, normal) * normal;

    private static async Task SaveImageAsync(Vector3[] pixels, string filename, int width, int height)
    {
        await Task.Run(() =>
        {
            using var image = new Image<Rgb24>(width, height);

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < height; y++)
                {
                    var rowSpan = accessor.GetRowSpan(y);

                    for (int x = 0; x < width; x++)
                    {
                        var color = pixels[y * width + x];

                        // Gamma correction and tone mapping for better image quality
                        var r = (byte)Math.Clamp((int)(MathF.Pow(color.X, 1.0f / 2.2f) * 255), 0, 255);
                        var g = (byte)Math.Clamp((int)(MathF.Pow(color.Y, 1.0f / 2.2f) * 255), 0, 255);
                        var b = (byte)Math.Clamp((int)(MathF.Pow(color.Z, 1.0f / 2.2f) * 255), 0, 255);

                        rowSpan[x] = new Rgb24(r, g, b);
                    }
                }
            });

            image.SaveAsPng(filename);
        });
    }
}
