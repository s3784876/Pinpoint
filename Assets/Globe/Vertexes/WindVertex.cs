
using System;
using System.Collections.Generic;

namespace Pinpoint.Globes.Vertexes
{
  public class WindVertex : IInterpolatable<WindVertex>
  {
    #region IInterpolatable
    public WindVertex Interpolate(WindVertex opponent, float opponentWeight)
    {
      WindVertex wv1 = CloneScale((1 - opponentWeight) / 2),
      wv2 = opponent.CloneScale(opponentWeight / 2);

      for (int i = 0; i < Seasons.Length; i++)
        wv1.Seasons[i].Add(wv2.Seasons[i]);

      return wv1;
    }
    public WindVertex CloneScale(float weight)
    {
      WindVertex wl = new WindVertex(this);

      for (int i = 0; i < Seasons.Length; i++)
        wl.Seasons[i].CloneScale(weight);

      return wl;
    }
    #endregion



    private HeightVertex AverageHeight;
    public SeasonalWindVertex[] Seasons { get; protected set; }

    public SeasonalWindVertex Summer
    {
      get
      {
        return Seasons[0];
      }
      private set
      {
        Seasons[0] = value;
      }
    }

    public SeasonalWindVertex Winter
    {
      get
      {
        return Seasons[1];
      }
      private set
      {
        Seasons[1] = value;
      }
    }



    public SeasonalWindVertex GetSeasonal(bool isSummer)
    {
      return isSummer ? Summer : Winter;
    }

    public (float, float) GetWindDirection(bool isSummer)
    {
      //Get correct season
      var sv = GetSeasonal(isSummer);

      //Return sum of vectors
      return sv.GetXYSpeed(AverageElevation());
    }

    public WindVertex(WindVertex wv)
    {
      this.Seasons = wv.Seasons;
      this.AverageHeight = wv.AverageHeight;
    }

    public WindVertex(HeightVertex hv) : this()
    {
      AverageHeight = hv;
    }

    private WindVertex()
    {
      Seasons = new SeasonalWindVertex[2];

      for (int i = 0; i < Seasons.Length; i++)
      {
        Seasons[i] = new SeasonalWindVertex();
      }
    }

    public float AverageElevation()
    {
      return AverageHeight.CurrentHeight;
    }

    public override bool Equals(object obj)
    {
      return obj is WindVertex vertex &&
             EqualityComparer<HeightVertex>.Default.Equals(AverageHeight, vertex.AverageHeight) &&
             EqualityComparer<SeasonalWindVertex[]>.Default.Equals(Seasons, vertex.Seasons) &&
             EqualityComparer<SeasonalWindVertex>.Default.Equals(Summer, vertex.Summer) &&
             EqualityComparer<SeasonalWindVertex>.Default.Equals(Winter, vertex.Winter);
    }

    public override int GetHashCode()
    {
      int hashCode = -394990692;
      hashCode = hashCode * -1521134295 + EqualityComparer<HeightVertex>.Default.GetHashCode(AverageHeight);
      hashCode = hashCode * -1521134295 + EqualityComparer<SeasonalWindVertex[]>.Default.GetHashCode(Seasons);
      return hashCode;
    }
  }

  public class SeasonalWindVertex : IInterpolatable<SeasonalWindVertex>
  {

    #region IInterpolatable
    public SeasonalWindVertex Interpolate(SeasonalWindVertex opponent, float opponentWeight)
    {
      //Add half of each
      SeasonalWindVertex wv1 = CloneScale((1 - opponentWeight) / 2),
      wv2 = opponent.CloneScale(opponentWeight / 2);

      //Sum all units to get the average for each layer
      for (int i = 0; i < LAYER_COUNT; i++)
        wv1.Layers[i].Add(wv2.Layers[i]);

      return wv1;
    }
    public SeasonalWindVertex CloneScale(float weight)
    {
      SeasonalWindVertex wv = new SeasonalWindVertex(this);

      for (int i = 0; i < LAYER_COUNT; i++)
        wv.Layers[i].CloneScale(weight);

      return wv;
    }
    #endregion



    //Number of layers in the layer array
    private const int LAYER_COUNT = 10;

    //Height of layer in meters
    public const int LAYER_SIZE = 900;

    WindLayer[] _Layers;

    public WindLayer[] Layers
    {
      get
      {
        return _Layers;
      }
      private set
      {
        _Layers = value;
      }
    }


    public SeasonalWindVertex(WindLayer[] layers)
    {
      Layers = layers;
    }

    public SeasonalWindVertex(WindLayer layer)
    {
      Layers = new WindLayer[LAYER_COUNT];

      for (int i = 0; i < LAYER_COUNT; i++)
      {
        Layers[i] = new WindLayer(layer);
      }
    }

    private SeasonalWindVertex(SeasonalWindVertex swv)
    {
      this.Layers = swv.Layers;
    }

    public SeasonalWindVertex() : this(new WindLayer())
    { }

    public int GetGroundSpeed(float averageElevation)
    {
      var (yTotal, xTotal) = GetXYSpeed(averageElevation);

      return (int)GeometricMath.EuclidianDistance(xTotal, yTotal);
    }

    public (float, float) GetXYSpeed(float averageElevation)
    {
      //Sum layers that are below the ground level
      int max = Math.Min(LAYER_COUNT, (int)(averageElevation / LAYER_SIZE));

      float yTotal = 0, xTotal = 0;
      for (int i = 0; i < max; i++)
      {
        yTotal += Layers[i].GetYVelocity();
        xTotal += Layers[i].GetXVelocity();
      }

      return (yTotal, xTotal);
    }

    public void Add(WindLayer[] layers)
    {
      for (int i = 0; i < LAYER_COUNT; i++)
      {
        Layers[i].Add(layers[i]);
      }
    }

    public void Add(SeasonalWindVertex sv)
    {
      Add(sv.Layers);
    }

    public override bool Equals(object obj)
    {
      return obj is SeasonalWindVertex vertex &&
             EqualityComparer<WindLayer[]>.Default.Equals(_Layers, vertex._Layers) &&
             EqualityComparer<WindLayer[]>.Default.Equals(Layers, vertex.Layers);
    }

    public override int GetHashCode()
    {
      int hashCode = -654778673;
      hashCode = hashCode * -1521134295 + EqualityComparer<WindLayer[]>.Default.GetHashCode(_Layers);
      hashCode = hashCode * -1521134295 + EqualityComparer<WindLayer[]>.Default.GetHashCode(Layers);
      return hashCode;
    }
  }
  public class WindLayer : IInterpolatable<WindLayer>
  {
    #region IInterpolatable
    public WindLayer Interpolate(WindLayer opponent, float opponentWeight)
    {
      WindLayer wl1 = CloneScale((1 - opponentWeight) / 2),
      wl2 = opponent.CloneScale(opponentWeight / 2);

      wl1.Add(wl2);

      return wl1;
    }

    public void Scale(float weight)
    {
      Velocity = (byte)(Velocity * weight);
    }
    public WindLayer CloneScale(float weight)
    {
      WindLayer wl = new WindLayer(this);

      wl.Velocity = (byte)(wl.Velocity * weight);

      return wl;
    }
    #endregion

    public byte Velocity { get; private set; }
    byte _Direction;

    //Returns the direction the layer is heading, in radians, relitive to north
    float Direction
    {
      get
      {
        return (float)(_Direction / (float)byte.MaxValue * 2 * Math.PI);
      }
      set
      {
        _Direction = (byte)((value % 2 * Math.PI) / (2 * Math.PI)
            * byte.MaxValue);
      }
    }



    public WindLayer(byte velocity, float direction)
    {
      Velocity = velocity;
      Direction = direction;
    }

    public WindLayer(byte xVelocity, byte yVelocity)
    {
      Velocity = (byte)Math.Round(GeometricMath.EuclidianDistance(xVelocity, yVelocity), MidpointRounding.AwayFromZero);
      Direction = (float)Math.Tanh(yVelocity / xVelocity);
    }

    public WindLayer(WindLayer wl)
    {
      Velocity = wl.Velocity;
      _Direction = wl._Direction;
    }

    public WindLayer()
    {
      Velocity = 0;
      Direction = 0;
    }

    public short GetXVelocity()
    {
      return (short)(Math.Sin(Direction) * Velocity);
    }

    public short GetYVelocity()
    {
      return (short)(Math.Cos(Direction) * Velocity);
    }

    public void Add(WindLayer w)
    {
      int xVel = GetXVelocity() + w.GetXVelocity(),
          yVel = GetYVelocity() + w.GetYVelocity();

      Direction = (float)Math.Atan(xVel / yVel);
      Velocity = (byte)Math.Min(Math.Sqrt(xVel * xVel + yVel * yVel), byte.MaxValue);
    }

    public override bool Equals(object obj)
    {
      return obj is WindLayer layer &&
             Velocity == layer.Velocity &&
             _Direction == layer._Direction &&
             Direction == layer.Direction;
    }

    public override int GetHashCode()
    {
      int hashCode = -247130553;
      hashCode = hashCode * -1521134295 + Velocity.GetHashCode();
      hashCode = hashCode * -1521134295 + _Direction.GetHashCode();
      hashCode = hashCode * -1521134295 + Direction.GetHashCode();
      return hashCode;
    }
  }
}