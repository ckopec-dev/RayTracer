using System.Numerics;

namespace RayTracer;

public class Program
{
    private const int Width = 800;
    private const int Height = 600;
    private const int MaxDepth = 5;
    private const float Epsilon = 0.001f;

    public static async Task Main()
    {
        await RenderAsync();
    }

    public static async Task RenderAsync()
    {
        var scene = CreateScene();
        var camera = new Camera(new Vector3(0, 0, -5), Vector3.UnitZ, Vector3.UnitY, 60);
        var pixels = new Vector3[Width * Height];

        Console.WriteLine("Rendering...");

        // Parallel rendering for better performance
        await Task.Run(() =>
        {
            Parallel.For(0, Height, y =>
            {
                for (int x = 0; x < Width; x++)
                {
                    var ray = camera.GetRay(x, y, Width, Height);
                    var color = TraceRay(ray, scene, MaxDepth);
                    pixels[y * Width + x] = color;
                }

                if (y % 50 == 0)
                    Console.WriteLine($"Progress: {y}/{Height} lines completed");
            });
        });

        await SaveImageAsync(pixels, "raytraced_image.ppm");
        Console.WriteLine("Image saved as 'raytraced_image.ppm'");
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

    private static async Task SaveImageAsync(Vector3[] pixels, string filename)
    {
        using var writer = new StreamWriter(filename);

        // PPM header
        await writer.WriteLineAsync("P3");
        await writer.WriteLineAsync($"{Width} {Height}");
        await writer.WriteLineAsync("255");

        // Write pixel data
        for (int i = 0; i < pixels.Length; i++)
        {
            var color = pixels[i];

            // Gamma correction
            var r = (int)(MathF.Sqrt(color.X) * 255);
            var g = (int)(MathF.Sqrt(color.Y) * 255);
            var b = (int)(MathF.Sqrt(color.Z) * 255);

            // Clamp values
            r = Math.Clamp(r, 0, 255);
            g = Math.Clamp(g, 0, 255);
            b = Math.Clamp(b, 0, 255);

            await writer.WriteLineAsync($"{r} {g} {b}");
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

    public Ray GetRay(int x, int y, int width, int height)
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