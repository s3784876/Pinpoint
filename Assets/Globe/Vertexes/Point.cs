using System;
using UnityEngine;

using Pinpoint.Globes.Faces;

using static Pinpoint.Globes.GeometricMath;

namespace Pinpoint.Globes.Vertexes
{
  public class Point<T> where T : IInterpolatable<T>
  {
    private bool longLatCalculated = false;
    private float _Latitude;
    private float _Longitude;

    public float Latitude
    {
      get
      {
        CalculateLongLat();
        return _Latitude;
      }
      set
      {
        _Latitude = value;
        xyzCalculated = false;
      }
    }
    public float Longitude
    {
      get
      {
        CalculateLongLat();
        return _Latitude;
      }
      set
      {
        _Longitude = value;
        xyzCalculated = false;
      }
    }

    private void CalculateLongLat()
    {
      if (!longLatCalculated)
      {
        (_Longitude, _Latitude) = GeometricMath.GetLatLong(xFloat, yFloat, zFloat);
        longLatCalculated = true;
      }
    }


    private bool xyzCalculated = false;
    //Between 0 and resolution
    private int _x;
    private int _y;
    private int _z;

    public int CubeX
    {
      get
      {
        CalculateXYZ();
        return _x;
      }
      set
      {
        _x = value;
        ValidateCoordinantValue(value);
      }
    }
    public int CubeY
    {
      get
      {
        CalculateXYZ();
        return _y;
      }
      set
      {
        _y = value;
        ValidateCoordinantValue(value);
      }
    }
    public int CubeZ
    {
      get
      {
        CalculateXYZ();
        return _z;
      }
      set
      {
        _z = value;
        ValidateCoordinantValue(value);
      }
    }

    private void ValidateCoordinantValue(int value)
    {

      if (value < 0 || value > Globe.Resolution)
        Recalculate();

      longLatCalculated = false;
    }

    private void CalculateXYZ()
    {
      if (!xyzCalculated)
      {
        (xFloat, yFloat, zFloat) = GetXYZ(Latitude, Longitude);

        xyzCalculated = true;
      }
    }

    public (int, int) XY
    {
      get
      {
        if (IsEdge(CubeX))
        {
          return (CubeZ, CubeY);
        }
        else if (IsEdge(CubeZ))
        {
          return (CubeX, CubeY);
        }
        else
        {
          int _y = (int)(Globe.Resolution *
                  EuclidianDistance(xFloat, zFloat));

          int _x = (int)(Globe.Resolution * Math.Tanh(xFloat / zFloat) / (2 * Math.PI));

          return (_x, _y);
        }
      }
    }

    public (int, int) AbsoluteXY
    {
      get
      {
        var (x, y) = XY;
        int xOffset = 0, yOffset = 0;

        var up = GetLocalUp();

        if (up == Vector3.forward || up == Vector3.left || up == Vector3.right)
        {

        }
        else if (up == Vector3.back)
        {
          xOffset = -Globe.Resolution;
        }
        else if (up == Vector3.up || up == Vector3.down)
        {

          yOffset = Globe.Resolution;
          yOffset /= (up == Vector3.up) ? -2 : 1;

          /* Locate wedge
               BACK
              \ 2 /
              1 x 4
              / 3 \
              FRONT
            */

          //Case wedge 1 or 2
          if (CubeX < CubeZ)
          {
            //Wedge 1
            if (CubeX + CubeZ < Globe.Resolution)
            {
              x = CubeZ;
              y = CubeX;
            }
            else //Wedge 2
            {
              x = CubeX;
              y = CubeZ;
            }
          }
          else
          {
            //Wedge 3
            if (CubeX + CubeZ < Globe.Resolution)
            {
              x = CubeZ;
              y = CubeX;
            }
            else //Wedge 4
            {
              x = CubeX;
              y = CubeZ;
            }
          }
          //Compress x into the range [0,Resolution)
          //Or the range (-resolution,0] when x is negitive
          x = (y - x) * Globe.Resolution / (-2 * y + 10);
        }
        else
        {
          throw new IndexOutOfRangeException();
        }

        return (xOffset + x, yOffset + y);
      }
    }


    //<summary>
    //In Range [-1,1]
    //</summary>
    private float xFloat
    {
      get
      {
        return CompressPoint(CubeX);
      }
      set
      {
        CubeX = ExpandPoint(value);
      }
    }
    private float yFloat
    {
      get
      {
        return CompressPoint(CubeY);
      }
      set
      {
        CubeY = ExpandPoint(value);
      }
    }
    private float zFloat
    {
      get
      {
        return CompressPoint(CubeZ);
      }
      set
      {
        CubeZ = ExpandPoint(value);
      }
    }

    //Normalizes an integer in [0,Resolution) to [-1,1]
    public float CompressPoint(int x)
    {
      return x * 2 / (float)Globe.Resolution - 1;
    }

    //De-normilizes a point form [-1,1] to [0,resolution) 
    public int ExpandPoint(float x)
    {
      return (int)Math.Round(x / 2 + 0.5 * Globe.Resolution, MidpointRounding.AwayFromZero);
    }

    GlobeVertexes<T> Globe;

    public Point(GlobeVertexes<T> globe)
    {
      this.Globe = globe;
    }

    public Point(Point<T> point) : this(point._x, point._y, point._z, point.Globe)
    { }

    public Point(int x, int y, int z, GlobeVertexes<T> globe) : this(globe)
    {
      this.CubeX = x;
      this.CubeY = y;
      this.CubeZ = z;
    }

    public Point(float latitude, float longitude, GlobeVertexes<T> globe) : this(globe)
    {
      this._Latitude = latitude;
      this._Longitude = longitude;
    }

    public Point(Vector3 coords, GlobeVertexes<T> globe) : this((int)coords.x, (int)coords.y, (int)coords.z, globe)
    {

    }

    private void Recalculate()
    {
      int resolution = Globe.Resolution;

      int overflow = 0;

      int[] coords = {
                CubeX,
                CubeY,
                CubeZ
            };

      int edgeCount = 0;

      foreach (var item in coords)
      {
        overflow += Math.Max(0, item - resolution);
        overflow -= Math.Min(0, item);
        if (IsEdge(item))
          edgeCount++;
      }

      for (int i = 0; i < coords.Length; i++)
      {
        //Decrease for edges at the upperbound
        if (coords[i] == resolution)
          coords[i] -= overflow / edgeCount;

        //Increase for edges at the lower bound
        else if (coords[i] == 0)
          coords[i] += overflow / edgeCount;

        //Clamp between 0 and Resolution
        else
          coords[i] = Math.Max(0,
          Math.Min(resolution, coords[0])
          );
      }

      if (IsEdge(coords[0]))
        CubeX -= overflow / edgeCount;
      if (IsEdge(coords[1]))
        CubeY -= overflow / edgeCount;
      if (IsEdge(coords[1]))
        CubeZ -= overflow / edgeCount;
    }

    private bool IsEdge(int i)
    {
      return i == 0 || i == Globe.Resolution;
    }

    private Vector3 GetLocalUp()
    {
      if (IsEdge(CubeX))
      {
        if (CubeX == 0)
          return Vector3.left;
        else
          return Vector3.right;

      }
      else if (IsEdge(CubeY))
      {
        if (CubeY == 0)
          return Vector3.down;
        else
          return Vector3.up;
      }
      else if (IsEdge(CubeZ))
      {
        if (CubeZ == 0)
          return Vector3.back;
        else
          return Vector3.forward;
      }
      else
        throw new System.Exception();
    }

    public void StepXY(int x, int y)
    {
      StepY(y);
      StepX(x);
    }

    public void StepX(int xOffset)
    {
      if (!IsEdge(CubeY))
      {
        CubeX += xOffset;
      }
      else
      {
        //Is top or bottom face
        double r = GeometricMath.EuclidianDistance(xFloat, zFloat);

        //Get the base angle from the pole to the point
        double theta = Math.Atan(zFloat / xFloat);

        //Rotate by an arc length of xOffset
        theta += GeometricMath.GetTheta(r, xOffset);

        CubeZ = (int)(r * Math.Sin(theta));
        CubeX = (int)(r * Math.Cos(theta));
      }
    }

    public void StepY(int yOffset)
    {
      if (!IsEdge(CubeY))
      {
        //Is the front/right/left/back face
        CubeY += yOffset;
      }
      else
      {

        //Is top or bottom face
        double scale = yOffset / GeometricMath.EuclidianDistance(xFloat, zFloat);

        //If oversteps the pole
        if (scale > 1)
        {
          xFloat = 0;
          zFloat = 0;
        }
        else
        {
          CubeX -= (int)(CubeX * scale);
          CubeZ -= (int)(CubeZ * scale);
        }
      }
    }

    public void StepX()
    {
      //Logic for incrimenting x by 1 were more bloated than this method
      StepX(1);
    }

    public void StepY()
    {
      if (!IsEdge(CubeY))
      {
        //Is the front/right/left/back face
        CubeY++;
      }
      else
      {

        //Is top or bottom face
        double scale = 1 / Math.Sqrt(xFloat * xFloat + zFloat * zFloat);

        if (scale > 1)
        {
          xFloat = 0;
          zFloat = 0;
        }
        else
        {
          CubeX -= (int)(CubeX * scale);
          CubeZ -= (int)(CubeZ * scale);
        }
      }

    }

    public T GetPoint()
    {
      return Globe.GetVertex(CubeX, CubeY, CubeZ);
    }

    public Mesh<T> GetMesh()
    {
      return Globe.GetFace(Latitude, Longitude);
    }
  }
}