using UnityEngine;
namespace Pinpoint.Globe
{
  public class GlobeFace
  {
    private Vector3 LocalUp;

    public WindMesh WindMesh { get; protected set; }
    public ClimateMesh ClimateMesh { get; protected set; }
    public HeightMesh HeightMesh { get; protected set; }

    public GlobeFace(Vector3 localUp, int windResolution, int climateResolution, int heightResolution)
    {
      LocalUp = localUp;
      WindMesh = new WindMesh(windResolution, localUp);
      ClimateMesh = new ClimateMesh(climateResolution, localUp);
      HeightMesh = new HeightMesh(heightResolution, localUp);
    }
  }
}