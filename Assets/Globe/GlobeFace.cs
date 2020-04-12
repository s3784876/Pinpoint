using System.Numerics;
namespace Pinpoint.Globe
{
    public class GlobeFace
    {
        Vector3 LocalUp;
        int Resolution;

        WindMesh WindMesh = new WindMesh();
        ClimateMesh ClimateMesh = new ClimateMesh();
        HeightMesh HeightMesh = new HeightMesh();
    }
}