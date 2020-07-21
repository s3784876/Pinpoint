using System;
using System.Collections.Generic;

namespace Pinpoint.Globes.Vertexes
{

  public class ClimateVertex : IInterpolatable<ClimateVertex>
  {
    #region IInterpolatable
    public ClimateVertex Interpolate(ClimateVertex opponent, float opponentWeight)
    {
      ClimateVertex cv1 = CloneScale((1 - opponentWeight) / 2),
        cv2 = opponent.CloneScale(opponentWeight / 2);

      for (int i = 0; i < seasons.Length; i++)
        cv1.seasons[i] = (SeasonVertex)cv1.seasons[i].Interpolate(cv2.seasons[i], opponentWeight);

      return cv1;
    }
    public ClimateVertex CloneScale(float weight)
    {
      ClimateVertex cv = new ClimateVertex(this);

      for (int i = 0; i < seasons.Length; i++)
        seasons[i] = (SeasonVertex)seasons[i].CloneScale(weight);

      return cv;
    }

    #endregion

    public readonly WindVertex WindVertex;
    public readonly Area<HeightVertex> HeightVertexes;

    SeasonVertex[] seasons;

    private SeasonVertex summer
    {
      get
      {
        return seasons[0];
      }
      set
      {
        seasons[0] = value;
      }
    }
    private SeasonVertex winter
    {
      get
      {
        return seasons[1];
      }
      set
      {
        seasons[1] = value;
      }
    }

    public SeasonVertex GetSeason(bool isSummer)
    {
      return isSummer ? summer : winter;
    }

    #region climate classification
    //Simplified Koppen Climate Classification
    public enum Climate
    {
      Mountain,
      Ocean,
      Tropical_Rainforrest,
      Tropical_Monsoon,
      Tropical_Savannah,
      Hot_Desert,
      Hot_Steppe,
      Continental_Humid,
      Continental_Subarctic,
      Subtropical_Mediterranean,
      Subtropical_Humid,
      Subtropical_Oceanic,
      Cold_Desert,
      Cold_Steppe,
      Polar_Tundra,
      Polar_Ice_Caps,
      UNKNOWN

    };

    public readonly static string[] ClimateNames = {
            "Mountain",
            "Ocean",
            "Tropical Rainforrest",
            "Tropical Monsoon",
            "Tropical Savannah",
            "Hot Desert",
            "Hot Steppe",
            "Continental Humid",
            "Continental Subarctic",
            "Subtropical Mediterranean",
            "Subtropical Humid",
            "Subtropical Oceanic",
            "Cold Desert",
            "Cold Steppe",
            "Polar Tundra",
            "Polar Ice Caps",
            "UNKNOWN"
        };

    public Climate Classification(float averageElevation, int longitude)
    {
      longitude = Math.Abs(longitude);


      //Ignore high altitude areas.
      if (averageElevation < 0)
        return Climate.Ocean;

      else if (averageElevation > SeasonalWindVertex.LAYER_SIZE)
        return Climate.Mountain;

      else if (longitude <= 30)
        return TropicalCell();

      //Continental and subtropical climates
      else if (longitude <= 75)
        return MidLatitudeCell(longitude);

      else
        return PolarCell();
    }

    private Climate TropicalCell()
    {
      float averageRain = (summer.AnnualRain + winter.AnnualRain) / 2;
      float threshold = 100 - (averageRain / 25.0f);

      //Tropical Climates
      if (averageRain > 720)
      {
        return Climate.Tropical_Rainforrest;
      }
      //Desert has been moved up here to catch some edge cases that savannah would eclipse
      else if (IsDesert())
        return Climate.Hot_Desert;
      else if (winter.AnnualRain / 12 < 60)
      {
        if (winter.AnnualRain / 12 >= threshold)
          return Climate.Tropical_Monsoon;
        else
          return Climate.Tropical_Savannah;
      }
      //Use Steppe for blending
      else
        return Climate.Hot_Steppe;
    }



    private Climate MidLatitudeCell(int latitude)
    {
      float seasonalRainVariation = winter.AnnualRain == 0 ?
                                      float.PositiveInfinity :
                                      (summer.AnnualRain / winter.AnnualRain - 1);
      seasonalRainVariation *= seasonalRainVariation;

      if (latitude > 60 || (latitude >= 45 && winter.Temperature < 0))
        return Climate.Continental_Subarctic;

      else if (latitude < 45 && winter.Temperature > 0 && summer.AnnualRain / 12 >= 30)
        return Climate.Subtropical_Mediterranean;

      else if (winter.Temperature > 0 && summer.Temperature > 22)
        return Climate.Subtropical_Humid;

      else if (winter.Temperature > 0 && summer.Temperature > 10 && seasonalRainVariation < 0.5)
        return Climate.Subtropical_Oceanic;

      else if (IsDesert())
        return Climate.Cold_Desert;

      else if (latitude < 50 && IsSteppe())
        return Climate.Cold_Steppe;

      else
        return Climate.Continental_Humid;
    }

    private Climate PolarCell()
    {
      if (summer.Temperature > 0)
        return Climate.Polar_Tundra;
      else
        return Climate.Polar_Ice_Caps;
    }

    private int PrecipitationScore(float totalRain)
    {
      int precipitationScore = (summer.Temperature + winter.Temperature) * 10;

      if (summer.AnnualRain >= 0.7 * totalRain)
        precipitationScore += 280;
      else if (summer.AnnualRain >= 0.3 * totalRain)
        precipitationScore += 140;

      return precipitationScore;
    }

    //Returns true if climate matches BW classification
    private bool IsDesert()
    {
      float totalRain = (summer.AnnualRain + winter.AnnualRain) / 2;

      return totalRain < 0.5 * PrecipitationScore(totalRain);
    }

    private bool IsSteppe()
    {
      float totalRain = (summer.AnnualRain + winter.AnnualRain) / 2;
      int precipitationScore = PrecipitationScore(totalRain);

      return totalRain > 0.5 * precipitationScore
      && totalRain < precipitationScore;
    }

    public override bool Equals(object obj)
    {
      return obj is ClimateVertex vertex &&
             EqualityComparer<WindVertex>.Default.Equals(WindVertex, vertex.WindVertex) &&
             EqualityComparer<Area<HeightVertex>>.Default.Equals(HeightVertexes, vertex.HeightVertexes) &&
             EqualityComparer<SeasonVertex[]>.Default.Equals(seasons, vertex.seasons);
    }

    public override int GetHashCode()
    {
      int hashCode = -1583679682;
      hashCode = hashCode * -1521134295 + EqualityComparer<WindVertex>.Default.GetHashCode(WindVertex);
      hashCode = hashCode * -1521134295 + EqualityComparer<Area<HeightVertex>>.Default.GetHashCode(HeightVertexes);
      hashCode = hashCode * -1521134295 + EqualityComparer<SeasonVertex[]>.Default.GetHashCode(seasons);
      return hashCode;
    }


    #endregion

    #region Constructors

    public ClimateVertex(SeasonVertex[] sv)
    {
      if (sv.Length != 2)
      {
        throw new ArgumentOutOfRangeException();
      }
      seasons = sv;
    }

    public ClimateVertex(Grouping<WindVertex> g) : this(0, 0, 0, 0, g)
    { }

    private ClimateVertex(ClimateVertex cv)
    {
      this.seasons = cv.seasons;
    }

    public ClimateVertex(float summerRain, sbyte summerTemperature, float winterRain, sbyte winterTemperature)
    {
      summer = new SeasonVertex(1, summerRain, 1, summerTemperature);
      winter = new SeasonVertex(1, winterRain, 1, winterTemperature);

    }

    public ClimateVertex(float summerRain, sbyte summerTemperature, float winterRain, sbyte winterTemperature, Grouping<WindVertex> g) : this(summerRain, summerTemperature, winterRain, winterTemperature)
    {

      WindVertex = g.Get();
    }

    #endregion



    public class SeasonVertex : IInterpolatable<SeasonVertex>
    {
      #region IInterpolatable
      public SeasonVertex Interpolate(SeasonVertex opponent, float opponentWeight)
      {
        SeasonVertex sv1 = CloneScale((1 - opponentWeight) / 2),
          sv2 = opponent.CloneScale(opponentWeight / 2);

        sv1._AnnalRain += sv2._AnnalRain;
        sv1._Humidity += sv2._Humidity;
        sv1._Temperature += sv2._Temperature;
        sv1._SoilHardness += sv2._SoilHardness;

        return sv1;
      }
      public SeasonVertex CloneScale(float weight)
      {
        SeasonVertex sv = new SeasonVertex(this);

        sv.AnnualRain *= weight;
        sv.Humidity *= weight;
        sv.SoilHardness *= weight;
        sv.Temperature = (sbyte)(sv.Temperature * weight);

        return sv;
      }

      public override bool Equals(object obj)
      {
        return obj is SeasonVertex vertex &&
               _SoilHardness == vertex._SoilHardness &&
               _AnnalRain == vertex._AnnalRain &&
               _Temperature == vertex._Temperature &&
               _Humidity == vertex._Humidity;
      }

      public override int GetHashCode()
      {
        int hashCode = 465157026;
        hashCode = hashCode * -1521134295 + _SoilHardness.GetHashCode();
        hashCode = hashCode * -1521134295 + _AnnalRain.GetHashCode();
        hashCode = hashCode * -1521134295 + _Temperature.GetHashCode();
        hashCode = hashCode * -1521134295 + _Humidity.GetHashCode();
        return hashCode;
      }


      #endregion

      #region variables
      private byte _SoilHardness;
      private byte _AnnalRain;
      private sbyte _Temperature;
      private byte _Humidity;
      #endregion

      #region Properties
      public float SoilHardness
      {
        get
        {
          return _SoilHardness / (float)byte.MaxValue;
        }
        set
        {
          _SoilHardness = (byte)(value * byte.MaxValue);
        }
      }

      //Returns a value between 0 and 721, the max value useful for the climate classification (Rainforest)
      public float AnnualRain
      {
        get
        {
          return (256 / (float)(_AnnalRain + 1) - 1) * 721 / 255;
        }
        set
        {
          _AnnalRain = (byte)Math.Round((255 * value - 183855) / (255 * value + 721));
        }
      }


      //TODO set rainfall biased on temperature on set
      public float Humidity
      {
        get
        {
          return _Humidity / (float)byte.MaxValue;
        }
        set
        {
          _Humidity = (byte)(value * byte.MaxValue);

          //Use Magnus formula for the maximum water pressure 
          var vaporPressure = Humidity * Math.Exp((17.625 * Temperature) / (Temperature + 243.04));

          //Use Linear regression of vapor pressure to rainfall amount [1]


        }
      }

      public sbyte Temperature
      {
        get
        {
          return _Temperature;
        }
        set
        {
          _Temperature = value;
        }
      }
      #endregion


      #region Constructors
      public SeasonVertex(SeasonVertex sv)
      {
        SoilHardness = sv.SoilHardness;
        AnnualRain = sv.AnnualRain;
        Humidity = sv.Humidity;
        Temperature = sv.Temperature;
      }

      public SeasonVertex(float soilHardness, float rain, float humidity, sbyte temperature)
      {
        SoilHardness = soilHardness;
        AnnualRain = rain;
        Humidity = humidity;
        Temperature = temperature;
      }


      #endregion
    }
  }
}

/*

 [1] Fick, S.E. and R.J. Hijmans, 2017. WorldClim 2: new 1km spatial resolution climate surfaces for global land areas. International Journal of Climatology 37 (12): 4302-4315.

*/
