using System;


using System.Collections.Generic;
using Pinpoint.Globes.Vertexes;
using UnityEngine;
using static Pinpoint.Globes.Vertexes.ClimateVertex;

namespace Pinpoint.Globes
{
  public class ClimateGlobe : AttributeGlobe<ClimateVertex>
  {
    public ClimateGlobe(int resolution) : base(resolution)
    { }

    public override void Simulate()
    {
      //TODO implement

      foreach (var face in Faces)
        foreach (var item in face.Vertexes)
          Simulate(item);


    }

    private void Simulate(ClimateVertex cv)
    {
      bool isSummer;

      //DO for summer and winter
      for (int i = 0; i < 2; i++)
      {
        isSummer = i == 0;

        //Get wind pattern to nearest 100% humidity zone
        var path = GetPath(GetPoint(cv), isSummer);

        //Backtrack along path keeping track of humidity and tempreture as it does so
        var season = cv.GetSeason(isSummer);


      }

      throw new System.NotImplementedException();

    }

    private const float OCEAN_HUMIDITY_DELTA = 0.1f;
    private const float LAND_HUMIDITY_DROP_FACTOR = 0.9f;

    private ClimateVertex[] GetPath(Point<ClimateVertex> point, bool isSummer)
    {

      List<ClimateVertex> vertices = new List<ClimateVertex>();

      AtmosphereCell cell = AtmosphereCell.GetAtmosphereCell(point.Latitude);
      float humidity = 0;

      while (cell.DistanceToEnd(point.Latitude) > 0.5f && humidity < 100)
      {
        var cv = point.GetPoint();
        vertices.Add(cv);

        var heights = cv.HeightVertexes.GetElements();
        foreach (var hv in heights)
        {
          if (hv.IsOcean)
          {
            humidity += OCEAN_HUMIDITY_DELTA / heights.Length;
          }
          else
          {
            humidity *= LAND_HUMIDITY_DROP_FACTOR / heights.Length;
          }
        }

        var (x, y) = cv.WindVertex.GetWindDirection(isSummer);
        point.StepXY((int)x, (int)y);
      }

      vertices.Reverse();

      return vertices.ToArray();
    }

    private void BackTrack(ref SeasonVertex season, ClimateVertex[] path)
    {
      float humidity = 0, tempreture = 0;
      foreach (var item in path)
      {
        humidity = GetHumidity(item, humidity);
        tempreture = GetTemprature(item, tempreture);
      }
    }

    private static float GetHumidity(ClimateVertex cv, float currentHumidity)
    {
      var heights = cv.HeightVertexes.GetElements();
      foreach (var hv in heights)
      {
        if (hv.IsOcean)
        {
          currentHumidity += OCEAN_HUMIDITY_DELTA / heights.Length;
        }
        else
        {
          currentHumidity *= LAND_HUMIDITY_DROP_FACTOR / heights.Length;
        }
      }

      return currentHumidity;
    }

    private const float OCEAN_OFFSET = 18;
    private const float LAND_TEMP_OFFSET = OCEAN_OFFSET * 2;

    private const float LATITUDE_TEMP_GRADIENT = -0.5f;
    private const float LATITUDE_TEMP_OFFSET = 35;

    private const float HEIGHT_TEMP_GRADIENT = -1 / 150f;


    private float GetTemprature(ClimateVertex climateVertex, float currentTempreture)
    {

      var heights = climateVertex.HeightVertexes.GetElements();

      float equilibriumPoint = LATITUDE_TEMP_GRADIENT * GetPoint(climateVertex).Latitude + LATITUDE_TEMP_OFFSET * heights.Length;

      foreach (var hv in heights)
      {
        //Get the average tempreture from altitude
        equilibriumPoint += HEIGHT_TEMP_GRADIENT * hv.CurrentHeight + (hv.IsOcean ? OCEAN_OFFSET : LAND_TEMP_OFFSET);
      }
      equilibriumPoint /= heights.Length;

      return currentTempreture + GetEquilibriumFactor(currentTempreture, equilibriumPoint, 0.1f);
    }

    private static float GetEquilibriumFactor(float x, float equilibriumPoint, float equlibriumStrength)
    {
      //Simple linier formla
      return (x - equilibriumPoint) * equlibriumStrength;

      //Alternative cubic formula
      //return (float)(-Math.Pow(x - equilibriumPoint, 3) / Math.Abs(x - equilibriumPoint) * equlibriumStrength);
    }
  }
}