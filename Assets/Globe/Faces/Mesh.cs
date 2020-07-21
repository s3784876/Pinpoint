using System;
using UnityEngine;
using Pinpoint.Globes.Vertexes;
using System.Collections.Generic;

namespace Pinpoint.Globes.Faces
{
  public class Mesh<T> where T : IInterpolatable<T>
  {
    public T[] Vertexes { get; private set; }
    public readonly Vector3 LocalUp;

    private readonly int Resolution;

    //Constructor
    public Mesh(Vector3 localUp, int resolution)
    {
      this.Vertexes = new T[resolution * resolution];
      LocalUp = localUp;
      Resolution = resolution;
    }

    //Returns the pont located at the given co-ordinates
    public T GetPoint(float longitude, float latitude)
    {
      MapPoint(ref longitude, ref latitude);
      return GetVertex((int)longitude, (int)(latitude));
    }

    public T GetVertex(int x, int y)
    {
      int index = x + y * Resolution;
      return GetPoint(index);
    }

    //Returns vertexes[i]
    public T GetPoint(int index)
    {
      return Vertexes[index];
    }

    public bool SetPoint(int index, T value)
    {
      try
      {
        Vertexes[index] = value;
      }
      catch (System.Exception)
      {
        return false;
      }
      return true;
    }

    public bool SetPoint(int x, int y, T value)
    {
      int index = x + y * Resolution;
      return SetPoint(index, value);
    }


    //Returns the aproximation of the conditions at the given lat and long by returning a weighted average of the points in the region
    public T GetInterpolatedPoint(float latitude, float longitude)
    {
      MapPoint(ref latitude, ref longitude);

      T[] items = new T[4];
      Vector2Int[] shifts = {
              new Vector2Int(0,0),
              new Vector2Int(1,0),
              new Vector2Int(0,1),
              new Vector2Int(1,0),
            };

      int baseIndex = (int)latitude + (int)(longitude * Resolution);

      for (int i = 0; i < items.Length; i++)
        items[i] = GetPoint(baseIndex + shifts[i].x + shifts[i].y * Resolution);

      //merge nodes along x direction
      for (int i = 0; i < items.Length; i += 2)
        items[i / 2] = items[i].Interpolate(items[i + 1], latitude % 1);

      //Merge nodes along y direction
      for (int i = 0; i < items.Length / 2; i += 2)
        items[i / 2] = items[i].Interpolate(items[i + 1], longitude % 1);

      return items[0];
    }

    //Formats the latitude and longitude to represent the percentage that the target is along the x and y of the face.
    private void MapPoint(ref float latitude, ref float longitude)
    {
      if (Direction(ref latitude, ref longitude) != LocalUp)
        throw new ArgumentOutOfRangeException();
    }


    public static Vector3 Direction(ref float latitude, ref float longitude)
    {
      latitude = (float)(Math.PI * latitude / 180);
      longitude = (float)(Math.PI * longitude / 180);

      float y = (float)Math.Sin(latitude),
      x = (float)Math.Cos(longitude) * y,
      z = (float)Math.Sin(longitude) * y;

      double fx, fy, fz;
      fx = Math.Abs(x);
      fy = Math.Abs(y);
      fz = Math.Abs(z);

      if (fy >= fx && fy >= fz)
      {
        // top or bottom face
        longitude = x;
        latitude = z;

        return new Vector3(0, (y > 0 ? 1 : -1), 0);
      }
      else if (fx >= fy && fx >= fz)
      {
        //Right or left face
        longitude = z;
        latitude = y;

        return new Vector3((x > 0 ? 1 : -1), 0, 0);
      }
      else
      {
        //Front or back face
        longitude = x;
        latitude = y;
        return new Vector3(0, 0, (z > 0 ? 1 : -1));
      }
    }

    public override bool Equals(object obj)
    {
      return obj is Mesh<T> mesh &&
             EqualityComparer<T[]>.Default.Equals(Vertexes, mesh.Vertexes) &&
             LocalUp.Equals(mesh.LocalUp) &&
             Resolution == mesh.Resolution;
    }

    public override int GetHashCode()
    {
      int hashCode = -183254997;
      hashCode = hashCode * -1521134295 + EqualityComparer<T[]>.Default.GetHashCode(Vertexes);
      hashCode = hashCode * -1521134295 + LocalUp.GetHashCode();
      hashCode = hashCode * -1521134295 + Resolution.GetHashCode();
      return hashCode;
    }
  }
}