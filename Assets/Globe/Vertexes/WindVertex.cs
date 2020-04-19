using System;
using Pinpoint.Globe;

namespace Pinpoint.Globe.Vertexes
{
    public class WindVertex : IInterpolatable<WindVertex>
    {
        #region IInterpolatable
        public WindVertex Interpolate(WindVertex opponent, float opponentWeight)
        {
            WindVertex wv1 = CloneScale((1 - opponentWeight) / 2),
            wv2 = opponent.CloneScale(opponentWeight / 2);

            for (int i = 0; i < seasons.Length; i++)
                wv1.seasons[i].Add(wv2.seasons[i]);

            return wv1;
        }
        public WindVertex CloneScale(float weight)
        {
            WindVertex wl = new WindVertex(this);

            for (int i = 0; i < seasons.Length; i++)
                wl.seasons[i].Scale(weight);

            return wl;
        }
        #endregion

        SeasonalWindVertex[] seasons;

        SeasonalWindVertex Summer
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

        SeasonalWindVertex Winter
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

        public WindVertex(WindVertex swv)
        {
            this.seasons = swv.seasons;
        }

        public WindVertex()
        {
            seasons = new SeasonalWindVertex[2];

            for (int i = 0; i < seasons.Length; i++)
            {
                seasons[i] = new SeasonalWindVertex();
            }
        }
    }

    public class SeasonalWindVertex : IInterpolatable<SeasonalWindVertex>
    {

        #region IInterpolatable
        public SeasonalWindVertex Interpolate(SeasonalWindVertex opponent, float opponentWeight)
        {
            //Add half of each
            SeasonalWindVertex wv1 = Scale((1 - opponentWeight) / 2),
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

        private SeasonalWindVertex(SeasonalWindVertex swv)
        {
            this.Layers = swv.Layers;
        }

        public SeasonalWindVertex()
        {
            Layers = new WindLayer[LAYER_COUNT];

            for (int i = 0; i < LAYER_COUNT; i++)
            {
                Layers[i] = new WindLayer();
            }
        }

        public int getGroundSpeed(float averageElevation)
        {
            //Sum layers that are below the ground level
            int max = Math.Min(LAYER_COUNT, (int)(averageElevation / LAYER_SIZE));

            float yTotal = 0, xTotal = 0;
            for (int i = 0; i < max; i++)
            {
                yTotal += Layers[i].GetYVelocity();
                xTotal += Layers[i].GetXVelocity();
            }

            return (int)Math.Sqrt(yTotal * yTotal + xTotal * xTotal);
        }

        public void Add(WindLayer[] layers)
        {
            for (int i = 0; i < LAYER_COUNT; i++)
            {
                Layers[i].Add(layers[i]);
            }
        }
        public void Add(WindVertex vertex)
        {
            Add(vertex.Layers);
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

            byte Velocity;
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


        }
    }
}