using System.Numerics;

namespace RayTracer;

public class Program
{
    // Quality presets with anti-aliasing samples
    private static readonly Dictionary<string, (int width, int height, int maxDepth, int samples)> QualityPresets = new()
    {
        { "low", (640, 480, 3, 1) },
        { "medium", (1280, 720, 5, 2) },
        { "high", (1920, 1080, 6, 4) },
        { "ultra", (2560, 1440, 8, 8) },
        { "4k", (3840, 2160, 10, 16) }
    };

    private const float Epsilon = 0.001f;

    public static async Task Main(string[] args)
    {
        // Parse command line arguments
        string quality = args.Length > 0 ? args[0].ToLower() : "high";

        if (!QualityPresets.ContainsKey(quality))
        {
            Console.WriteLine("Available quality settings: low, medium, high, ultra, 4k");
            Console.WriteLine("Usage: dotnet run [quality]");
            Console.WriteLine("Example: dotnet run ultra");
            Console.WriteLine("Using default: high");
            quality = "high";
        }

        var (width, height, maxDepth, samples) = QualityPresets[quality];
        Console.WriteLine($"Rendering at {width}x{height} with {maxDepth} ray bounces and {samples}x anti-aliasing ({quality} quality)");

        await RenderAsync(width, height, maxDepth, samples);
    }

    public static async Task RenderAsync(int width, int height, int maxDepth, int samples)
    {
        var scene = CreateScene();
        var camera = new Camera(new Vector3(0, 0, -5), Vector3.UnitZ, Vector3.UnitY, 60);
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

                        var ray = camera.GetRay(x + offsetX, y + offsetY, width, height);
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
                            Console.WriteLine($"Progress: {progress:F1}% ({linesCompleted}/{height} lines) - ETA: {estimated:mm\\:ss}");
                        }
                    }
                }

                return localRandom;
            }, _ => { });
        });

        var totalTime = DateTime.Now - startTime;
        Console.WriteLine($"Rendering completed in {totalTime:mm\\:ss\\.ff}");

        var filename = $"raytraced_{width}x{height}_{samples}xAA.ppm";
        await SaveImageAsync(pixels, filename, width, height);
        Console.WriteLine($"Image saved as '{filename}'");
        Console.WriteLine($"File size: {new FileInfo(filename).Length / 1024.0 / 1024.0:F1} MB");
    }

    private static Scene CreateScene()
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
        using var writer = new StreamWriter(filename);

        // PPM header
        await writer.WriteLineAsync("P3");
        await writer.WriteLineAsync($"# Ray traced image {width}x{height}");
        await writer.WriteLineAsync($"{width} {height}");
        await writer.WriteLineAsync("255");

        // Write pixel data with better formatting for smaller files
        for (int y = 0; y < height; y++)
        {
            var line = new System.Text.StringBuilder();
            for (int x = 0; x < width; x++)
            {
                var color = pixels[y * width + x];

                // Gamma correction and tone mapping for better image quality
                var r = (int)(MathF.Pow(color.X, 1.0f / 2.2f) * 255);
                var g = (int)(MathF.Pow(color.Y, 1.0f / 2.2f) * 255);
                var b = (int)(MathF.Pow(color.Z, 1.0f / 2.2f) * 255);

                // Clamp values
                r = Math.Clamp(r, 0, 255);
                g = Math.Clamp(g, 0, 255);
                b = Math.Clamp(b, 0, 255);

                line.Append($"{r} {g} {b} ");
            }
            await writer.WriteLineAsync(line.ToString().TrimEnd());
        }
    }
}

public readonly record struct Ray(Vector3 Origin, Vector3 Direction)
{
    public Vector3 PointAt(float t) => Origin + Direction * t;

    public static Ray Create(Vector3 origin, Vector3 direction) =>
        new(origin, Vector3.Normalize(direction));
}

public class Camera
{
    private readonly Vector3 _position;
    private readonly Vector3 _forward;
    private readonly Vector3 _up;
    private readonly Vector3 _right;
    private readonly float _fov;

    public Camera(Vector3 position, Vector3 forward, Vector3 up, float fovDegrees)
    {
        _position = position;
        _forward = Vector3.Normalize(forward);
        _up = Vector3.Normalize(up);
        _right = Vector3.Cross(_forward, _up);
        _fov = fovDegrees * MathF.PI / 180.0f;
    }

    public Ray GetRay(float x, float y, int width, int height)
    {
        float aspect = (float)width / height;
        float scale = MathF.Tan(_fov * 0.5f);

        float px = (2.0f * x / width - 1.0f) * aspect * scale;
        float py = (1.0f - 2.0f * y / height) * scale;

        var direction = Vector3.Normalize(_forward + _right * px + _up * py);
        return new Ray(_position, direction);
    }
}

public readonly record struct Material(
    Vector3 Color,
    float Diffuse,
    float Reflectivity,
    float Shininess);

public readonly record struct HitInfo(
    Vector3 Point,
    Vector3 Normal,
    float Distance,
    Material Material,
    Ray Ray);

public abstract class SceneObject(Material material)
{
    public Material Material { get; } = material;
    public abstract HitInfo? Intersect(Ray ray);
}

public class Sphere(Vector3 center, float radius, Material material) : SceneObject(material)
{
    private readonly Vector3 _center = center;
    private readonly float _radius = radius;

    public override HitInfo? Intersect(Ray ray)
    {
        var oc = ray.Origin - _center;
        var a = Vector3.Dot(ray.Direction, ray.Direction);
        var b = 2.0f * Vector3.Dot(oc, ray.Direction);
        var c = Vector3.Dot(oc, oc) - _radius * _radius;
        var discriminant = b * b - 4 * a * c;

        if (discriminant < 0)
            return null;

        var sqrtDiscriminant = MathF.Sqrt(discriminant);
        var t1 = (-b - sqrtDiscriminant) / (2 * a);
        var t2 = (-b + sqrtDiscriminant) / (2 * a);
        var t = t1 > 0.001f ? t1 : t2;

        if (t <= 0.001f)
            return null;

        var point = ray.PointAt(t);
        var normal = Vector3.Normalize(point - _center);

        return new HitInfo(point, normal, t, Material, ray);
    }
}

public readonly record struct Light(
    Vector3 Position,
    Vector3 Color,
    float Intensity);

public class Scene
{
    public List<SceneObject> Objects { get; } = [];
    public List<Light> Lights { get; } = [];
}