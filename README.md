# RayTracer

A simple ray tracing implementation for educational purposes.

**Core Features:**
- **Ray-sphere intersection** with proper mathematical calculations
- **Phong lighting model** with diffuse and specular components
- **Shadow casting** to create realistic lighting effects  
- **Reflections** with configurable reflectivity per material
- **Multiple light sources** with different colors and intensities

**Key Components:**
- `Ray` class for representing rays with origin and direction
- `Camera` class with perspective projection and field of view
- `Material` class defining surface properties (color, reflectivity, shininess)
- `Sphere` class as the primary geometric primitive
- `Scene` class to organize objects and lights

**Rendering Process:**
1. For each pixel, cast a ray from the camera through the pixel
2. Find the closest intersection with scene objects
3. Calculate lighting using the Phong model (ambient + diffuse + specular)
4. Cast shadow rays to determine if surfaces are in shadow
5. Handle reflections recursively up to a maximum depth
6. Apply gamma correction for more realistic color output

The scene includes three colored spheres with different material properties and a ground plane, lit by two light sources. The rendering will show realistic shadows, reflections, and highlights based on the surface materials.

**Modern C# Features:**
- **Record structs** for immutable data types (Ray, Material, HitInfo, Light)
- **Nullable reference types** enabled for better null safety
- **Pattern matching** with `is null` and `is not null`
- **Collection expressions** with `[]` syntax
- **Top-level programs** and file-scoped namespaces
- **Expression-bodied members** for concise code

**Cross-Platform Compatibility:**
- **No System.Drawing dependency** - replaced with PPM image format
- **PPM format** is widely supported and can be viewed in most image viewers
- **Async I/O operations** for better performance
- **Parallel rendering** using `Parallel.For` for multi-core performance

**Performance Improvements:**
- **Parallel pixel processing** using all available CPU cores
- **MathF functions** instead of Math for better float performance
- **Struct-based design** to reduce garbage collection pressure
- **Efficient memory layout** with readonly structs

**Key improvements over the original:**
- Fully cross-platform (Windows, Linux, macOS)
- Better performance through parallelization
- Modern C# idioms and safety features
- No external dependencies beyond .NET 8
- Cleaner, more maintainable code structure


**For best performance**, use:
```bash
$ dotnet run -c Release
```

This will use all your CPU cores to render the image much faster.

The program outputs a `.ppm` image file that can be opened in most image viewers or converted to PNG/JPG. On Windows, IrfanView works well for viewing PPM files directly.

**To convert to a png**, use:

```bash 
$ convert raytraced_image.ppm output.png
```

