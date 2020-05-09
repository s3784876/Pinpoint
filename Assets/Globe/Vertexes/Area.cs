using System.Collections.Generic;

namespace Pinpoint.Globes.Vertexes
{
  public class Area<T> where T : IInterpolatable<T>
  {
    private T[,] elements;



    public Area(T[,] elements)
    {
      this.elements = elements;
    }

    public T[,] GetElements()
    {
      return elements;
    }

    public T Average()

    {
      for (int step = 2; step < elements.Length * 2; step *= step)
      {
        for (int x = 1; x < elements.Length; x += step)
        {
          for (int y = 0; y < elements.Length; y += step)
          {
            //Get the localized average of 4 points
            elements[x, y] = elements[x, y].Interpolate(elements[x + step / 2, y], 0.5f);

            elements[x, y + step / 2] = elements[x, y + step / 2].Interpolate(elements[x + step / 2, y + step / 2], 0.5f);

            elements[x, y] = elements[x, y].Interpolate(elements[x, y + step / 2], 0.5f);
          }
        }
      }

      return elements[0, 0];
    }

    public override bool Equals(object obj)
    {
      return obj is Area<T> area &&
             EqualityComparer<T[,]>.Default.Equals(elements, area.elements);
    }

    public override int GetHashCode()
    {
      return 272633004 + EqualityComparer<T[,]>.Default.GetHashCode(elements);
    }
  }


}