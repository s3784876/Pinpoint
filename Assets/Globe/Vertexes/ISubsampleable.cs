using System;

namespace Pinpoint.Globe.Vertexes
{
    public interface IInterpolatable<T>
    {
        T Interpolate(T opponent, float weight);
        T Scale(float weight);
    }
}