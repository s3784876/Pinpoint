using System.Net;
using Pinpoint.Globe;

namespace Pinpoint.Globe.Vertexes
{
    public class HeightVertex : IInterpolatable<HeightVertex>
    {
        float initialHeight;
        float currentHeight;

        Interpolation<ClimateVertex> climate;
        Interpolation<WindVertex> wind;

        //Stores 4 bits of information relevent to plotting errosion
        /*  Bit 1   Has Vegitation
            Bit 2   Is River
            Bit 3   Is Ocean
         */
        private byte _Covering;


        //Tile has vegitation when the gradient to nearby tiles is less than a threshold amount, and rainfall in the area is above a threshold
        public bool HasVegitation
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

        public bool IsRiver
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

        //Tile will be configered as ocean during construction
        public bool IsOcean
        {
            get
            {
                return (_Covering & 0b_0100) != 0;
            }
            set
            {
                byte mask;
                if (value)
                {
                    mask = 0b_0100;
                    //set bit 1 true
                    _Covering |= mask;
                }
                else
                {
                    //set flase
                    mask = 0b_1011;
                    _Covering &= mask;
                }
            }
        }

        public HeightVertex(float initialHeight, float currentHeight, ClimateVertex climate, WindVertex wind, bool isRiver, bool hasVegitation, bool IsOcean)
        {
            this.initialHeight = initialHeight;
            this.currentHeight = currentHeight;
            this.climate = climate;
            this.wind = wind;
            this.IsRiver = isRiver;
            this.HasVegitation = hasVegitation;
            this.IsOcean = IsOcean;
        }

        public HeightVertex(HeightVertex hv)
        {
            this.initialHeight = hv.initialHeight;
            this.currentHeight = hv.currentHeight;
            this.climate = hv.climate;
            this.wind = hv.wind;
            this.IsRiver = hv.IsRiver;
            this.HasVegitation = hv.HasVegitation;
            this.IsOcean = hv.IsOcean;
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
                hv1.HasVegitation = hv2.HasVegitation;
                hv1.IsRiver = hv2.IsRiver;
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

        public bool IsChild(WindVertex wv)
        {

        }

        public bool IsChild(ClimateVertex cv)
        {

        }
    }
}