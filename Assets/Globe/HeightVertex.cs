using System.Net;
using System.Security.Cryptography.X509Certificates;
namespace Pinpoint.Globe
{
  public class HeightVertex : ISupersampleable<HeightVertex>
  {
    float initialHeight;
    float currentHeight;

    ClimateVertex climate;
    AnualWindVertex wind;

    //Stores 4 bytes of information relevent to plotting errosion
    /*  Bit 1  Has Vegitation
        Bit 2  Is River
     */
    private byte _Covering;

    public bool isRiver
    {
      get
      {
        return (_Covering & 0b_0010) != 0;
      }
      set
      {
        byte mask;
        if (value)
        {
          mask = 0b_0010;
          //set bit 2 true
          _Covering |= mask;
        }
        else
        {
          //set flase
          mask = 0b_1101;
          _Covering &= mask;
        }
      }
    }

    //Tile has vegitation when the gradient to nearby tiles is less than a threshold amount, and rainfall in the area is above a threshold
    public bool hasVegitation
    {
      get
      {
        return (_Covering & 0b_0001) != 0;
      }
      set
      {
        byte mask;
        if (value)
        {
          mask = 0b_0001;
          //set bit 1 true
          _Covering |= mask;
        }
        else
        {
          //set flase
          mask = 0b_1110;
          _Covering &= mask;
        }
      }
    }

    public HeightVertex(float initialHeight, float currentHeight, ClimateVertex climate, AnualWindVertex wind, bool isRiver, bool hasVegitation)
    {
      this.initialHeight = initialHeight;
      this.currentHeight = currentHeight;
      this.climate = climate;
      this.wind = wind;
      this.isRiver = isRiver;
      this.hasVegitation = hasVegitation;
    }

    public HeightVertex(HeightVertex hv)
    {
      this.initialHeight = hv.initialHeight;
      this.currentHeight = hv.currentHeight;
      this.climate = hv.climate;
      this.wind = hv.wind;
      this.isRiver = hv.isRiver;
      this.hasVegitation = hv.hasVegitation;
    }

    public HeightVertex Interpolate(HeightVertex opponent, float opponentWeight)
    {
      //Divide each weight by 2 so that when added they sum to make the average
      HeightVertex hv1 = Scale((1 - opponentWeight) / 2),
      hv2 = opponent.Scale(opponentWeight / 2);

      hv1.initialHeight += hv2.initialHeight;
      hv1.currentHeight += hv2.currentHeight;

      if (opponentWeight > 0.5)
      {
        hv1.hasVegitation = hv2.hasVegitation;
        hv1.isRiver = hv2.isRiver;
      }

      return hv1;
    }
    public HeightVertex Scale(float weight)
    {
      HeightVertex hv = new HeightVertex(this);

      hv.currentHeight *= weight;
      hv.initialHeight *= weight;

      return hv;
    }

    public bool IsChild(AnualWindVertex wv)
    {

    }

    public bool IsChild(ClimateVertex cv)
    {
      
    }
  }
}