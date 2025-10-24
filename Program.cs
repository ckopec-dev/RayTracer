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

        await Demos.ThreeSpheres(width, height, maxDepth, samples);     
        await Demos.BallsOnSurface(width, height, maxDepth, samples);
        
        // Move lights around
        //for (decimal f = 5; f > 0; f -= 0.1m)
        //{
        //    Scene balls = Demos.BallsOnSurface();
        //    balls.Filename = new string($"moving_light_{f}.png");
        //    //balls.Camera.Position = new((float)f, balls.Camera.Position.Y, balls.Camera.Position.Z);
        //    balls.Lights[0].Position = new((float)f, (float)f, (float)f);
        //    await RenderAsync(balls, width, height, maxDepth, samples);
        //}
    }
}