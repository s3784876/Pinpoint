
using System;
using Pinpoint.Globes;
using Pinpoint.Globes.Vertexes;

namespace Pinpoint.Testing
{
  public static class UnitTest
  {
    public static bool TestAll()
    {
      bool isWorking = true;

      isWorking &= TestWindModel();
      isWorking &= TestPointModel();
      isWorking &= TestClimateModel();

      if (!isWorking)
        return isWorking;


      isWorking &= TestHeightMapping();
      isWorking &= TestWindMapping();
      isWorking &= TestClimateMapping();

      if (!isWorking)
        return isWorking;


      isWorking &= DryRun();

      return isWorking;
    }

    private static void PrintError(string cause, Exception ex)
    {

      Console.WriteLine(cause + " failed with error:");
      Console.WriteLine('\t' + ex.ToString());
      Console.WriteLine('\t' + '\t' + ex.StackTrace);
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
        PrintError("Height Mapping", ex);
        return false;
      }
      return true;
    }

    public static bool TestWindMapping()
    {

      WindGlobe w = new WindGlobe(100, new HeightGlobe(100 * 10));
      try
      {
        w.Simulate();
      }
      catch (Exception ex)
      {
        PrintError("Wind Mapping", ex);
        return false;
      }

      return true;
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
        PrintError("Climate Mapping", ex);
        return false;
      }

      return true;
    }

    public static bool TestPointModel()
    {
      Console.WriteLine("Testing Point Model");
      const int resolution = 128;

      Point<HeightVertex> p = new Point<HeightVertex>(0f, 0f, new HeightGlobe(resolution));

      try
      {
        for (int i = 0; i <= resolution * 5; i++)
        {
          //If the loop failed to get back to where we started
          if (i == resolution * 5)
          {
            throw new Exception();
          }

          p.StepX();
          if (p.Latitude == 0f && p.Latitude == 0f)
            break;
        }
      }
      catch (System.Exception)
      {
        return false;
        throw;
      }

      //Test over one of the poles
      p.StepY(resolution + resolution / 2);

      try
      {
        for (int i = 0; i <= resolution * 3; i++)
        {
          //If the loop failed to get back to where we started
          if (i == resolution * 3)
          {
            throw new Exception();
          }

          p.StepX();

          //If a sucessful circut has been performed
          if (p.Latitude == 0f && p.Latitude == 0f)
            break;
        }
      }
      catch (System.Exception)
      {
        return false;
        throw;
      }

      return true;
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

      sbyte[] summerTemps = {
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

      sbyte[] winterTemps = {
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

      return true;
    }

    public static bool TestWindModel()
    {
      byte[] expectedResults = {
                5,
                5,
                13,
                13
            };

      byte[] xVelocities = {
                3,
                4,
                5,
                12
            };

      byte[] yVelocities = {
                4,
                3,
                12,
                5
            };

      byte x, y, r;

      for (int i = 0; i < expectedResults.Length; i++)
      {
        (x, y, r) = (xVelocities[i], yVelocities[i], expectedResults[i]);
        WindLayer l = new WindLayer(x, y);

        Console.WriteLine($"Inputs: ({x},{x})\tReult: {l.Velocity}\tExpected: {r}");

        if (l.Velocity != r)
          throw new System.Exception();
      }

      return true;
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
        PrintError("Dry Run", ex);
        return false;
      }

      return true;
    }
  }
}