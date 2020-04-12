
using UnityEngine;

namespace Pinpoint.Globe
{
  public class HeightMesh : AMesh<HeightVertex>
  {
    public HeightMesh(int resolution, Vector3 localUp)
        : base(resolution, localUp)
    { }

    #region Inheritance
    public override void Simulate()
    { }
    protected override void Step()
    { }

    #endregion
  }
}