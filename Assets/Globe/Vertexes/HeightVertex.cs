using System.Net;
using Pinpoint.Globe;

namespace Pinpoint.Globe.Vertexes
{
    public class HeightVertex : IInterpolatable<HeightVertex>
    {
        float InitialHeight;
        float CurrentHeight;

        Grouping<ClimateVertex> Climate;
        Grouping<WindVertex> Wind;

        //Stores 4 bits of information relevent to plotting errosion
        /*  Bit 1   Has Vegitation
            Bit 2   Is River
            Bit 3   Is Ocean/lake
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

        private HeightVertex(float initialHeight, float currentHeight, bool isRiver, bool hasVegitation, bool IsOcean)
        {
            this.InitialHeight = initialHeight;
            this.CurrentHeight = currentHeight;
            this.IsRiver = isRiver;
            this.HasVegitation = hasVegitation;
            this.IsOcean = IsOcean;
        }

        public HeightVertex(float initialHeight, float currentHeight, bool isRiver, bool hasVegitation, bool IsOcean, ClimateVertex climate, WindVertex wind) : this(initialHeight, currentHeight, isRiver, hasVegitation, IsOcean)
        {
            //Use the points as the only member in the grouping
            this.Climate = new Grouping<ClimateVertex>(climate);
            this.Wind = new Grouping<WindVertex>(wind);
        }

        public HeightVertex(float initialHeight, float currentHeight, bool isRiver, bool hasVegitation, bool IsOcean, Grouping<ClimateVertex> climate, Grouping<WindVertex> wind) : this(initialHeight, currentHeight, isRiver, hasVegitation, IsOcean)
        {
            this.Climate = climate;
            this.Wind = wind;
        }

        public HeightVertex(HeightVertex hv) : this(hv.InitialHeight, hv.CurrentHeight, hv.IsRiver, hv.HasVegitation, hv.IsOcean, hv.Climate, hv.Wind)
        { }

        public HeightVertex Interpolate(HeightVertex opponent, float opponentWeight)
        {
            //Divide each weight by 2 so that when added they sum to make the average
            HeightVertex hv1 = CloneScale((1 - opponentWeight) / 2),
            hv2 = opponent.CloneScale(opponentWeight / 2);

            hv1.InitialHeight += hv2.InitialHeight;
            hv1.CurrentHeight += hv2.CurrentHeight;

            if (opponentWeight > 0.5)
            {
                hv1.HasVegitation = hv2.HasVegitation;
                hv1.IsRiver = hv2.IsRiver;
            }

            return hv1;
        }
        public HeightVertex CloneScale(float weight)
        {
            HeightVertex hv = new HeightVertex(this);

            hv.CurrentHeight *= weight;
            hv.InitialHeight *= weight;

            return hv;
        }

        public bool IsChild(WindVertex wv)
        {
            return Wind.Equals(wv);
        }

        public bool IsChild(ClimateVertex cv)
        {
            return Climate.Equals(cv);
        }
    }
}