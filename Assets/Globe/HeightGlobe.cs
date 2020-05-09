using Pinpoint.Globes.Vertexes;

namespace Pinpoint.Globes
{
    public class HeightGlobe : AttributeGlobe<HeightVertex>
    {
        public HeightGlobe(int resolution) : base(resolution)
        { }

        public override void Simulate()
        {
            System.Console.WriteLine("Finished building height mesh");
        }
    }
}