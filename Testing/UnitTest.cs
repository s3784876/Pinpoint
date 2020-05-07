using Pinpoint.Globe;
using Pinpoint.Globe.Vertexes;

namespace Pinpoint.Testing
{
    public static class UnitTest
    {
        public static bool TestAll()
        {
            TestWindModel();
            TestClimateModel();


            TestHeightMapping();
            TestWindMapping();
            TestClimateMapping();

            DryRun();
        }

        public static bool TestHeightMapping()
        {
            HeightGlobe h = new HeightGlobe(100);
            try
            {
                h.Simulate();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Height Mapping failed with error:");
                Console.WriteLine(ex.GetType());
                Console.WriteLine(ex.message);
                Console.WriteLine(ex.StackTrace);
                return false;
            }
        }

        public static bool TestWindMapping()
        {

            WindGlobe w = new WindGlobe(100);
            try
            {
                w.Simulate();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Wind Mapping failed with error:");
                Console.WriteLine(ex.GetType());
                Console.WriteLine(ex.message);
                Console.WriteLine(ex.StackTrace);
                return false;
            }
        }

        public static bool TestClimateMapping()
        {
            ClimateGlobe c = new ClimateGlobe(100);
            try
            {
                c.Simulate();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cliamte Mapping failed with error:");
                Console.WriteLine('\t' + ex.GetType());
                Console.WriteLine('\t' + '\t' + ex.message);
                Console.WriteLine('\t' + '\t' + ex.StackTrace);
                return false;
            }
        }

        public static bool TestClimateModel()
        {
            string[] expectedResults = {
                "Tropical Rainforrest",
                "Tropical Monsoon",     //Miami
                "Tropical Savannah",    //Darwin
                "Hot Desert",           //Sabha
                "Hot Steppe",           //Patos
                "Continental Subarctic",
                "Subtropical Mediterranean",
                "Subtropical Humid",
                "Subtropical Oceanic",
                "Cold Desert",          //Leh
                "Cold Steppe",          //Penticton
                "Continental Humid",
                "Polar Tundra",
                "Polar Ice Caps"
            };

            int[] summerRainfalls = {
                721,
                50 * 12,
                375 * 12,

                0,
                140 * 12,


                1,
                30 * 12,
                1,
                1,

                10,
                30,
                30,

                0,
                0
            };

            int[] winterRainfalls = {
                721,
                190 * 12,
                10*12,

                0,
                0,


                1,
                0,
                1,
                1,

                7,
                25,
                25,


                0,
                0
            };

            int[] summerTemps = {
                20,
                20,
                20,

                32,
                27,


                1,
                1,
                23,
                11,

                17,
                22,
                22,

                1,
                -1
            };

            int[] winterTemps = {
                20,
                20,
                20,

                15,
                24,


                -1,
                1,
                1,
                1,

                -5,
                -1,
                -1,


                -10,
                -10
            };


            int[] latitude = {
                15,
                15,
                15,
                15,
                15,

                50,
                40,
                50,
                50,
                50,
                40,
                50,

                80,
                80
            };


            for (int i = 0; i < expectedResults.Length; i++)
            {
                ClimateVertex v = new ClimateVertex(summerRainfalls[i], summerTemps[i], winterRainfalls[i], winterTemps[i]);
                ClimateVertex.Climate c = v.Classification(1, latitude[i]);

                Console.WriteLine($"Result: {ClimateVertex.ClimateNames[(int)c]}\t\tExpected: {expectedResults[i]}");

                if (ClimateVertex.ClimateNames[(int)c] != expectedResults[i])
                {
                    Console.WriteLine("Unexpected result in climate clasification");
                    throw new System.Exception();
                }
            }
        }

        public static bool TestWindModel()
        {
            int[] expectedResults = {
                5,
                5,
                13,
                13
            };

            int[] xVelocities = {
                3,
                4,
                5,
                12
            };

            int[] yVelocities = {
                4,
                3,
                12,
                5
            };

            int x, y, r;

            for (int i = 0; i < expectedReults.Length; i++)
            {
                (x, y, r) = (xVelocities[i], yVelocities[i], expectedResults[i]);
                WindLayer l = new WindLayer(x, y);

                Console.WriteLine($"Inputs: ({x},{x})\tReult: {l.Velocity}\tExpected: {r}");

                if (l.Velocity != r)
                    throw new System.Exception();
            }
        }

        public static bool DryRun()
        {
            try
            {
                Globe g = new Globe();
                g.SimulateGlobes();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Dry run failed with error:");
                Console.WriteLine('\t' + ex.GetType());
                Console.WriteLine('\t' + '\t' + ex.message);
                Console.WriteLine('\t' + '\t' + ex.StackTrace);
                return false;
            }
        }
    }
}