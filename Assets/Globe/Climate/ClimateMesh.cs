using UnityEngine;
using Pinpoint.Globe;

namespace Pinpoint.Globe.Climate
{
    public class ClimateMesh : AMesh<ClimateVertex>
    {
        public ClimateMesh(int resolution, Vector3 localUp)
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