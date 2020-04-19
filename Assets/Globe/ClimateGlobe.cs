using Pinpoint.Globe.Vertexes;

namespace Pinpoint.Globe
{
    public class ClimateGlobe : AttributeGlobe<ClimateVertex>
    {
        public ClimateGlobe(int resolution) : base(resolution)
        { }

        public override void Simulate()
        {
            throw new System.NotImplementedException();
        }
    }
}