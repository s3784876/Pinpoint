using System.Collections.Generic;
using Pinpoint.Globes.Vertexes;
using System;

namespace Pinpoint.Globes
{
  public class AtmosphereCell
  {
    public int StartLat;
    public int EndLat;
    private float StartHeading;
    private float EndHeading;

    private float Gradient;

    float Strength;

    private static List<AtmosphereCell> Cells = new List<AtmosphereCell>();

    public AtmosphereCell(int startLat, int endLat, float startHeading, float endHeading, float strength)
        : this(startLat, endLat, startHeading, endHeading)
    {
      Strength = strength;
    }
    public AtmosphereCell(int startLat, int endLat, float startHeading, float endHeading)
    {
      StartLat = Math.Min(startLat, endLat);
      EndLat = Math.Min(endLat, startLat);

      if (StartHeading < EndHeading - 180)
        startHeading += 360;


      StartHeading = startHeading;
      EndHeading = endHeading;

      Gradient = (endHeading - startHeading) / (endLat - startLat);

      Strength = 0.2f;

      Cells.Add(this);
    }

    private float GetHeading(float lat)
    {
      //Ensure that the point is in the range.
      if (!IsInRange(lat))
        throw new ArgumentOutOfRangeException();

      //Plot heading as a linear function of latitude, resulting in a parabolic path along a flat surface
      return ((lat - StartLat) * Gradient + StartHeading) % 360;
    }

    public float GetHeading(float lat, bool isSummer)
    {
      if (isSummer)
        return GetHeading(lat - 30);
      else
        return GetHeading(lat + 30);
    }

    //Will better model incresed air pressure in choke points
    public float GetProjectedHeading(float lat, float startingLong, bool isSummer)
    {
      throw new NotImplementedException();
    }

    public bool IsInRange(float latitude)
    {
      return EndLat > latitude && StartLat <= latitude;
    }

    public float DistanceToEnd(float latitude)
    {
      float terminator = Gradient > 0 ? EndLat : StartLat;

      return Math.Abs(latitude - terminator);
    }

    public static AtmosphereCell GetAtmosphereCell(float latitude)
    {
      foreach (var item in Cells)
        if (item.IsInRange(latitude))
          return item;

      throw new IndexOutOfRangeException();
    }


  }
}