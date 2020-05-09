using System;

namespace Pinpoint.Globes.Vertexes
{
    public interface IInterpolatable<T>
    {

        T Interpolate(T opponent, float weight);
        T CloneScale(float weight);
    }
}