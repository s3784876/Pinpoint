using UnityEngine;
namespace Pinpoint.Globe
{
  public class WindMesh : AMesh<AnualWindVertex>
  {
    public WindMesh(int resolution, Vector3 localUp)
   : base(resolution, localUp)
    { }

    #region inheritance
    public override void Simulate()
    { 
        //TODO
    }

    protected override void Step()
    { 
        //TODO
    }
    #endregion

  }
}