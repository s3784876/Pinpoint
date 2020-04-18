using UnityEngine;
using Pinpoint.Globe;

namespace Pinpoint.Globe.Vertexes
{
    public class ClimateMesh : AMesh<ClimateVertex>
    {
        public ClimateMesh(int resolution, Vector3 localUp)
       : base(resolution, localUp)
        { }

        #region Inheritance
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