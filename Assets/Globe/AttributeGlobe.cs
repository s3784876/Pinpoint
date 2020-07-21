using System.IO.Compression;
using Pinpoint.Globes.Vertexes;
using Pinpoint.Globes.Faces;
using UnityEngine;
using System;

namespace Pinpoint.Globes
{
  public abstract class GlobeVertexes<T> where T : IInterpolatable<T>
  {
    public readonly int Resolution;

    public Faces.Mesh<T>[] Faces { get; protected set; }

    private enum FIndex { up, down, right, left, back, front }

    public GlobeVertexes(int resolution)
    {
      Vector3[] ups = {
              Vector3.up,
              Vector3.down,
              Vector3.right,
              Vector3.left,
              Vector3.back,
              Vector3.forward
            };

      Faces = new Faces.Mesh<T>[6];

      for (int i = 0; i < Faces.Length; i++)
      {
        Faces[i] = new Faces.Mesh<T>(ups[i], resolution);
      }
    }

    //Returns the point that a latitude and longitude are pointing to regardless of face
    protected T GetGlobalCoordinant(float latitude, float longitude)
    {
      Faces.Mesh<T> mesh = GetFace(latitude, longitude);

      return mesh.GetPoint(latitude, longitude);
    }

    //Known problems occur on the last row and last column of each face
    //TODO solve
    protected T GetGlobalInterpolatedCoordinant(float latitude, float longitude)
    {
      Faces.Mesh<T> mesh = GetFace(latitude, longitude);

      return mesh.GetInterpolatedPoint(latitude, longitude);
    }

    //Locates and returns the face responsible for the lat and long provided, does not normalise lat and long
    public Faces.Mesh<T> GetFace(float latitude, float longitude)
    {
      return GloballyMapCoordinants(ref latitude, ref longitude);
    }

    protected (Faces.Mesh<T>, int, int) FindFace(int x, int y, int z)
    {
      float nx, ny, nz;
      nx = NormalizeDimension(x);
      ny = NormalizeDimension(y);
      nz = NormalizeDimension(z);

      double ax, ay, az;
      ax = Math.Abs(nx);
      ay = Math.Abs(ny);
      az = Math.Abs(nz);

      Mesh<T> f;
      int returnX, returnY;

      if (ay >= ax && ay >= az)
      {
        //Top or bottom face
        f = Faces[(int)FIndex.up + (y < 0 ? 1 : 0)];
        returnX = x;
        returnY = z;
      }
      else if (ax >= ay && ax >= az)
      {
        f = Faces[(int)FIndex.left + (x < 0 ? 1 : 0)];
        returnX = z;
        returnY = y;
      }
      else
      {
        f = Faces[(int)FIndex.front + (z < 0 ? 1 : 0)];
        returnX = x;
        returnY = y;
      }

      return (f, returnX, returnY);
    }

    private float NormalizeDimension(int x)
    {
      return x * 2.0f / Resolution - 1;
    }

    //Returns the Faces.Mesh<T> responsible for the point, and returns the x,y co-ords of the point on that mesg
    protected Faces.Mesh<T> GloballyMapCoordinants(ref float latitude, ref float longitude)
    {
      Mesh<T> f;

      latitude = (float)(Math.PI * latitude / 180);
      longitude = (float)(Math.PI * longitude / 180);

      //Plot yo unit sphere
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

        f = Faces[(int)FIndex.up + (y < 0 ? 1 : 0)];
      }
      else if (fx >= fy && fx >= fz)
      {
        //Right or left face
        longitude = z;
        latitude = y;

        f = Faces[(int)FIndex.left + (x < 0 ? 1 : 0)];
      }
      else
      {
        //Front or back face
        longitude = x;
        latitude = y;

        f = Faces[(int)FIndex.front + (z < 0 ? 1 : 0)];
      }

      longitude = Clamp(longitude);
      latitude = Clamp(latitude);

      return f;
    }

    //maps value x [-1,1] to [0,resolution]
    private int Clamp(float x)
    {
      //Keep close to 0 to ensure accuracy before adding 0.5
      return (int)Math.Round((x / 2 + 0.5) * Resolution, MidpointRounding.AwayFromZero);
    }

    protected T GetVertex(int x, int y, Vector3 localUp)
    {
      foreach (var item in Faces)
      {
        if (item.LocalUp == localUp)
          return item.GetVertex(x, y);
      }
      throw new NullReferenceException();
    }

    protected Point<T> GetPoint(int x, int y, Vector3 localUp)
    {
      for (int i = 0; i < Faces.Length; i++)
      {
        if (Faces[i].LocalUp == localUp)
        {
          var V3 = GetCubePosition(i, Faces[i].LocalUp, x, y);
          return new Point<T>(V3, this);
        }
      }
      throw new NullReferenceException();

    }

    //x,y,z are all assumed to be in the range [0,Resolution)
    public T GetVertex(int x, int y, int z)
    {
      var (face, _x, _y) = FindFace(x, y, z);
      return face.GetVertex(_x, _y);
    }

    protected T GetVertex(Point<T> p)
    {
      return GetVertex(p.CubeX, p.CubeY, p.CubeZ);
    }

    protected Vector3 GetXYZ(T v)
    {
      for (int i = 0; i < Faces.Length; i++)
        for (int x = 0; x < Resolution; x++)
          for (int y = 0; y < Resolution; y++)
          {
            if (Faces[i].GetVertex(x, y).Equals(v))
            {
              return GetCubePosition(i, Faces[i].LocalUp, x, y);
            }
          }

      throw new IndexOutOfRangeException();
    }

    private static Vector3 GetCubePosition(int faceID, Vector3 localUp, int x, int y)
    {
      switch (faceID)
      {
        case ((int)FIndex.back):
        case ((int)FIndex.front):
          localUp.x = x;
          localUp.y = y;
          break;

        case ((int)FIndex.left):
        case ((int)FIndex.right):
          localUp.z = x;
          localUp.y = y;
          break;

        case ((int)FIndex.down):
        case ((int)FIndex.up):
          localUp.z = y;
          localUp.x = x;
          break;

        default:
          throw new IndexOutOfRangeException();
      }

      return localUp;
    }

    protected Point<T> GetPoint(T v)
    {
      Vector3 coords = GetXYZ(v);
      return new Point<T>(coords, this);
    }

    //Called to perform all the calculations to get the mesh to a completed state
    public abstract void Simulate();
  }
}