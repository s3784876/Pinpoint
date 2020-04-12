using System;
using UnityEngine;

namespace Pinpoint.Globe
{
  public abstract class AMesh<T> where T : Pinpoint.Globe.ISupersampleable<T>
  {
    T[] Vertexes;
    static int Resolution;
    Vector3 LocalUp;

    private static AMesh<T>[] Faces = new AMesh<T>[6];

    private enum FIndex { up, down, right, left, back, front }

    protected AMesh(int resolution, Vector3 localUp)
    {
      this.Vertexes = new T[resolution * resolution];
      Resolution = resolution;


    }

    protected T GetPoint(float latitude, float longitude)
    {
      MapPoint(ref latitude, ref longitude);
      int index = (int)latitude + (int)(longitude * Resolution);

      return GetPoint(index);

    }

    public T GetPoint(int index)
    {
      return Vertexes[index];
    }

    protected T GetInterpolatedPoint(float latitude, float longitude)
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

    private void MapPoint(ref float latitude, ref float longitude)
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

        if (LocalUp != Vector3.up && LocalUp != Vector3.down)
          throw new IndexOutOfRangeException();
      }
      else if (fx >= fy && fx >= fz)
      {
        //Right or left face
        longitude = z;
        latitude = y;

        if (LocalUp != Vector3.right && LocalUp != Vector3.left)
          throw new IndexOutOfRangeException();
      }
      else
      {
        //Front or back face
        longitude = x;
        latitude = y;

        if (LocalUp != Vector3.forward && LocalUp != Vector3.back)
          throw new IndexOutOfRangeException();
      }
    }

    static protected T GetGlobalPoint(float latitude, float longitude)
    {
      AMesh<T> mesh = FindFace(latitude, longitude);

      return mesh.GetPoint(latitude, longitude);
    }

    //Known problems occur on the last row and last column of each face
    //TODO solve
    static protected T GetGlobalInterpolatedPoint(float latitude, float longitude)
    {
      AMesh<T> mesh = FindFace(latitude, longitude);

      return mesh.GetInterpolatedPoint(latitude, longitude);
    }

    public static AMesh<T> FindFace(float latitude, float longitude)
    {
      return GloballyMapPoints(ref latitude, ref longitude);
    }
    public static AMesh<T> GloballyMapPoints(ref float latitude, ref float longitude)
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

        return Faces[(int)FIndex.up + (y < 0 ? 1 : 0)];
      }
      else if (fx >= fy && fx >= fz)
      {
        //Right or left face
        longitude = z;
        latitude = y;

        return Faces[(int)FIndex.left + (x < 0 ? 1 : 0)];
      }
      else
      {
        //Front or back face
        longitude = x;
        latitude = y;

        return Faces[(int)FIndex.front + (z < 0 ? 1 : 0)];
      }
    }

    public abstract void simulate();
    public abstract void Step();
  }
}