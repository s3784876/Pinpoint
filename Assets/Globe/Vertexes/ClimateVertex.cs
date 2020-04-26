using System;
using Pinpoint.Globe;

namespace Pinpoint.Globe.Vertexes
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

        #region climate clasification
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

        public readonly string[] ClimateNames = {
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
            float averageRain = (summer.AnualRain + winter.AnualRain) / 2;
            float threshold = 100 - (averageRain / 25.0f);

            //Tropical Climates
            if (averageRain > 720)
            {
                return Climate.Tropical_Rainforrest;
            }
            //Desert has been moved up here to catch some edge cases that savannah would eclipse
            else if (IsDesert())
                return Climate.Hot_Desert;
            else if (winter.AnualRain / 12 < 60)
            {
                if (winter.AnualRain / 12 >= threshold)
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
            float seasonalRainVariation = winter.AnualRain == 0 ?
                                            float.PositiveInfinity :
                                            (summer.AnualRain / winter.AnualRain - 1);
            seasonalRainVariation *= seasonalRainVariation;

            if (latitude > 60 || (latitude >= 45 && winter.Tempreture < 0))
                return Climate.Continental_Subarctic;

            else if (latitude < 45 && winter.Tempreture > 0 && summer.AnualRain / 12 >= 30)
                return Climate.Subtropical_Mediterranean;

            else if (winter.Tempreture > 0 && summer.Tempreture > 22)
                return Climate.Subtropical_Humid;

            else if (winter.Tempreture > 0 && summer.Tempreture > 10 && seasonalRainVariation < 0.5)
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
            if (summer.Tempreture > 0)
                return Climate.Polar_Tundra;
            else
                return Climate.Polar_Ice_Caps;
        }

        private int PrecipitationScore(float totalRain)
        {
            int precipitationScore = (summer.Tempreture + winter.Tempreture) * 10;

            if (summer.AnualRain >= 0.7 * totalRain)
                precipitationScore += 280;
            else if (summer.AnualRain >= 0.3 * totalRain)
                precipitationScore += 140;

            return precipitationScore;
        }

        //Returns true if climate matches BW clasification
        private bool IsDesert()
        {
            float totalRain = (summer.AnualRain + winter.AnualRain) / 2;

            return totalRain < 0.5 * PrecipitationScore(totalRain);
        }

        private bool IsSteppe()
        {
            float totalRain = (summer.AnualRain + winter.AnualRain) / 2;
            int precipitationScore = PrecipitationScore(totalRain);

            return totalRain > 0.5 * precipitationScore
            && totalRain < precipitationScore;
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

        private ClimateVertex(ClimateVertex cv)
        {
            this.seasons = cv.seasons;
        }

        public ClimateVertex(float summerRain, sbyte summerTempreture, float winterRain, sbyte winterTempreture)
        {
            summer = new SeasonVertex(1, summerRain, 1, summerTempreture);
            winter = new SeasonVertex(1, winterRain, 1, winterTempreture);
        }

        #endregion

        public class SeasonVertex : IInterpolatable<SeasonVertex>
        {
            #region IInterpolatable
            public SeasonVertex Interpolate(SeasonVertex opponent, float opponentWeight)
            {
                SeasonVertex sv1 = CloneScale((1 - opponentWeight) / 2),
                  sv2 = opponent.CloneScale(opponentWeight / 2);

                sv1._AnualRain += sv2._AnualRain;
                sv1._Humidity += sv2._Humidity;
                sv1._Tempreture += sv2._Tempreture;
                sv1._SoilHardness += sv2._SoilHardness;

                return sv1;
            }
            public SeasonVertex CloneScale(float weight)
            {
                SeasonVertex sv = new SeasonVertex(this);

                sv.AnualRain *= weight;
                sv.Humidity *= weight;
                sv.SoilHardness *= weight;
                sv.Tempreture = (sbyte)(sv.Tempreture * weight);

                return sv;
            }

            #endregion

            #region variables
            private byte _SoilHardness;
            private byte _AnualRain;
            private sbyte _Tempreture;
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

            //Returns a value between 0 and 721, the max value useful for the climate clasification
            public float AnualRain
            {
                get
                {
                    return (256 / (float)(_AnualRain + 1) - 1) * 721 / 255;
                }
                set
                {
                    _AnualRain = (byte)Math.Round((255 * value - 183855) / (255 * value + 721));
                }
            }

            public float Humidity
            {
                get
                {
                    return _Humidity / (float)byte.MaxValue;
                }
                set
                {
                    _Humidity = (byte)(value * byte.MaxValue);
                }
            }

            public sbyte Tempreture
            {
                get
                {
                    return _Tempreture;
                }
                set
                {
                    _Tempreture = value;
                }
            }
            #endregion


            #region Constructors
            public SeasonVertex(SeasonVertex sv)
            {
                SoilHardness = sv.SoilHardness;
                AnualRain = sv.AnualRain;
                Humidity = sv.Humidity;
                Tempreture = sv.Tempreture;
            }

            public SeasonVertex(float soilHardness, float rain, float humidity, sbyte tempreture)
            {
                SoilHardness = soilHardness;
                AnualRain = rain;
                Humidity = humidity;
                Tempreture = tempreture;
            }


            #endregion
        }
    }
}