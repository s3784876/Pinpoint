using System.Linq;
using System;
using System.Collections.Generic;
using Pinpoint.Globe.Vertexes;

namespace Pinpoint.Globe.Pathing
{
  public class Head
  {

    private readonly Point<WindVertex> Point;
    private List<Node> Nodes;
    private List<Point<WindVertex>> Path;

    private AtmosphereCell Cell;
    private bool isSummer;
    private byte Strike;
    private readonly byte Accuracy;
    private readonly bool IsSummer;


    public Head(Point<WindVertex> point, AtmosphereCell local, byte accuracy, bool isSummer)
    {
      this.Point = point;
      this.Cell = local;

      Strike = 0;
      Accuracy = accuracy;
      IsSummer = isSummer;

      Nodes = new List<Node>();
    }

    public void Start()
    {
      Step(Point.CubeX, Point.CubeY);
    }

    public void Step(int sx, int sy)
    {
      var p = new Point<WindVertex>(Point);
      p.StepXY(sx, sy);

      if (Math.Abs(Cell.EndLat - p.Latitude) < 127 / Accuracy)
      {
        return;
      }

      //Get the reverse of the wind since we are back propigating
      float wind = (float)(Cell.GetHeading(Point.Latitude, IsSummer) + Math.PI);

      for (int dx = -1; dx < 2; dx++)
      {
        for (int dy = -1; dy < 2; dy++)
        {
          Populate(dx, dy, wind, p);
        }
      }
      Search(sx, sy).Exhausted = true;

      var node = GetNextNode();

      Step(node.x, node.y);
    }

    private Node GetNextNode()
    {
      var fresh = Nodes.FindAll((n) => !n.Exhausted);

      var nextWeight = fresh.AsParallel().Min(n => n.Weight);

      return fresh.Find(n => n.Weight == nextWeight);
    }

    private void Populate(int x, int y, float wind, Point<WindVertex> lastPoint)
    {

      var nextPoint = new Point<WindVertex>(lastPoint);
      nextPoint.StepXY(x, y);

      var heading = x == 0 ? Math.PI + Math.PI / 2 * y : Math.Tanh(y / x);

      var deltaHeight = nextPoint.GetPoint().AverageElevation()
                        - lastPoint.GetPoint().AverageElevation();
      var weight = GetCost(deltaHeight, (float)heading, wind);


      if (Contains(x, y))
      {
        var n = Search(x, y);
        n.Weight = Math.Min(n.Weight, weight);
      }
      else
        Nodes.Add(new Node(weight, nextPoint, x, y));
    }

    private bool Contains(int x, int y)
    {
      return Nodes.AsParallel().Any((n) => n.y == y && n.x == x);
    }

    private Node Search(int x, int y)
    {
      return Nodes.Find((n) => n.y == y && n.x == x);
    }

    private float GetCost(float deltaAltitude, float heading, float wind)
    {
      var diff = heading - wind;

      //if heading is in the first quadrant and wind is in the third
      if (diff < -Math.PI)
        heading += (float)(Math.PI * 2);

      //If wind is in the first quadrant and heading is in the thrid
      else if (diff > Math.PI)
        wind += (float)(Math.PI * 2);


      //Get the variance between the point's heading and the wind heading
      //Acts as hypotenuse length
      float result = heading - (heading + wind) / 2;
      result *= result;


      //Weight the prevailing wind more important than the incline
      return result + deltaAltitude / 1000;
    }
  }
}