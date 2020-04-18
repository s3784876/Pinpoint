using Microsoft.VisualBasic.Devices.ComputerInfo;

namespace Pinpoint.Globe
{
    public class Globe
    {
        private AttributeGlobe[] AttributeGlobe;

        public Globe()
        {
            AttributeGlobe = new AttributeGlobe[3];

            //Fraction of memory to use * number of bytes of each vertex
            float[] memAllocations = {
                0.5 * 21,
                0.25 * 12,
                0.125 * 130
            };

            for (int i = 0; i < memAllocations.Length; i++)
            {
                memAllocations[i] *= TotalPhysicalMemory / 6;
                memAllocations[i] = Math.Sqrt(memAllocations[i]);
            }

            AttributeGlobe[0] = new AttributeGlobe<HeightMesh, HeightVertex>((int)memAllocations[0]);
            AttributeGlobe[1] = new AttributeGlobe<ClimateMesh, ClimateVertex>((int)memAllocations[0]);
            AttributeGlobe[2] = new AttributeGlobe<WindMesh, WindVertex>((int)memAllocations[0]);
        }
    }
}