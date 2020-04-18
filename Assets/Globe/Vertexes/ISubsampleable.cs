using System;

namespace Pinpoint.Globe
{
  public interface ISupersampleable<T>
  {
    T Interpolate(T opponent, float weight);
    T Scale(float weight);
  }
}