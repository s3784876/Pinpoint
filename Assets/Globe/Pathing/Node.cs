using Pinpoint.Globe.Vertexes;

namespace Pinpoint.Globe.Pathing
{
  class Node
  {
    public float Weight;
    public Point<WindVertex> Point;

    public int x;
    public int y;

    public bool Exhausted;

    public Node(float weight, Point<WindVertex> point, int x, int y)
    {
      Weight = weight;
      Point = point;
      this.x = x;
      this.y = y;

      Exhausted = false;
    }
  }
}