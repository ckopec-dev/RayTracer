using System.Numerics;

namespace RayTracer;

public readonly record struct HitInfo(
   Vector3 Point,
   Vector3 Normal,
   float Distance,
   Material Material,
   Ray Ray);
