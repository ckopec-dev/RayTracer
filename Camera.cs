using System.Numerics;

namespace RayTracer;

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

