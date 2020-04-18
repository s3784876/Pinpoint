using UnityEngine;
using Pinpoint.Globe;

namespace Pinpoint.Globe.Wind
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