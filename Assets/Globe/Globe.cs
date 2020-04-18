using System.Diagnostics;

using System;
namespace Pinpoint.Globe
{
    public class Globe
    {
        private AttributeGlobe[] AttributeGlobe;

        private readonly AttributeGlobe HeightGlobe
        {
            get
            {
                return AttributeGlobe[0];
            }
        }
        private readonly AttributeGlobe ClimateGlobe
        {
            get
            {
                return AttributeGlobe[1];
            }
        }

        private readonly AttributeGlobe WindGlobe
        {
            get
            {
                return AttributeGlobe[2];
            }
        }

        public Globe()
        {
            AttributeGlobe = new AttributeGlobe[3];
            uint memoryAvalable = 0;

            using (Process proc = Process.GetCurrentProcess())
            {
                memoryAvalable = proc.PrivateMemorySize64;
            }

            //Fraction of memory to use * number of bytes of each vertex
            float[] memAllocations = {
                0.5 * 21,
                0.25 * 12,
                0.125 * 130
            };

            for (int i = 0; i < memAllocations.Length; i++)
            {
                memAllocations[i] *= memoryAvalable / 6;
                memAllocations[i] = Math.Sqrt(memAllocations[i]);
            }

            AttributeGlobe[0] = new AttributeGlobe<HeightMesh, HeightVertex>((int)memAllocations[0]);
            AttributeGlobe[1] = new AttributeGlobe<ClimateMesh, ClimateVertex>((int)memAllocations[1]);
            AttributeGlobe[2] = new AttributeGlobe<WindMesh, WindVertex>((int)memAllocations[2]);
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