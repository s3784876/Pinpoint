using System.Diagnostics;
using Pinpoint.Globe.Vertexes;
using System;
namespace Pinpoint.Globe
{
    public class Globe
    {
        private AttributeGlobe<HeightVertex> HeightGlobe;
        private AttributeGlobe<ClimateVertex> ClimateGlobe;
        private AttributeGlobe<WindVertex> WindGlobe;

        public Globe()
        {
            long memoryAvalable = 0;

            using (Process proc = Process.GetCurrentProcess())
            {
                memoryAvalable = proc.PrivateMemorySize64;
            }

            //Fraction of memory to use * number of bytes of each vertex
            float[] memAllocations = {
                0.5f * 21,
                0.25f * 12,
                0.125f * 130
            };

            for (int i = 0; i < memAllocations.Length; i++)
            {
                memAllocations[i] *= memoryAvalable / 6;
                memAllocations[i] = (int)Math.Sqrt(memAllocations[i]);
            }

            HeightGlobe = new HeightGlobe((int)memAllocations[0]);
            ClimateGlobe = new ClimateGlobe((int)memAllocations[1]);
            WindGlobe = new WindGlobe((int)memAllocations[2]);
        }

        public void SimulateGlobes()
        {
            Console.WriteLine("Working on Height map");
            HeightGlobe.Simulate();

            Console.WriteLine("Working On wind map");
            WindGlobe.Simulate();

            Console.WriteLine("Working On climate Map");
            ClimateGlobe.Simulate();
        }
    }
}