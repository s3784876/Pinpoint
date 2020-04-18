using Pinpoint.Globe.Vertexes;

namespace Pinpoint.Globe
{
    public class HeightGlobe : AAttributeGlobe<HeightVertex>
    {
        public override void Simulate()
        {
            System.Console.WriteLine("Finished building height mesh");
        }
    }
}