using System;

namespace Pinpoint.Assets.Globe.Vertexes
{
    public class Point
    {
        float Latitude;
        float Longitude;

        //Between 0 and resolution
        private int _x;
        private int _y;
        private int _z;

        public int x
        {
            get
            {
                return _x;
            }
            set
            {
                _x = value;
                if (value < 0 || value > Globe.Resolution)
                    Recalculate();
            }
        }
        public int y
        {
            get
            {
                return _y;
            }
            set
            {
                _y = value;
                if (value < 0 || value > Globe.Resolution)
                    Recalculate();
            }
        }
        public int z
        {
            get
            {
                return _z;
            }
            set
            {
                _z = value;
                if (value < 0 || value > Globe.Resolution)
                    Recalculate();
            }
        }

        private float xFloat
        {
            get
            {
                return GeometricMath.CompressPoint(x);
            }
            set
            {
                x = GeometricMath.ExpandPoint(value);
            }
        }
        private float yFloat
        {
            get
            {
                return GeometricMath.CompressPoint(y);
            }
            set
            {
                y = GeometricMath.ExpandPoint(value);
            }
        }
        private float zFloat
        {
            get
            {
                return GeometricMath.CompressPoint(z);
            }
            set
            {
                z = GeometricMath.ExpandPoint(value);
            }
        }

        AtributeGlobe Globe;

        private void Recalculate()
        {
            int resolution = Globe.Resolution;

            int overflow = 0;

            int[] coords = {
                x,
                y,
                z
            };

            int edgeCount = 0;

            foreach (var item in coords)
            {
                overflow += Math.Max(0, item - resolution);
                overflow -= Math.Min(0, item);
                if (IsEdge(item))
                    edgeCount++;
            }

            for (int i = 0; i < coords.Length; i++)
            {
                //Decrease for edges at the upperbound
                if (coords[i] == resolution)
                    coords[i] -= overflow / edgeCount;

                //Increase for edges at the lower bound
                else if (coords[i] == 0)
                    coords[i] += overflow / edgeCount;

                //Clamp between 0 and Resolution
                else
                    coords[i] = Math.Max(0,
                    Math.Min(resolution, coords[0])
                    );
            }

            if (IsEdge(coords[0]))
                x -= overflow / edgeCount;
            if (IsEdge(coords[1]))
                y -= overflow / edgeCount;
            if (IsEdge(coords[1]))
                z -= overflow / edgeCount;
        }

        private bool IsEdge(int i)
        {
            return i == 0 || i == Globe.Resolution;
        }

        private Vector3 GetLocalUp()
        {
            if (IsEdge(x))
            {
                if (x == 0)
                    return Vector3.Left;
                else
                    return Vector3.Right;

            }
            else if (IsEdge(y))
            {
                if (y == 0)
                    return Vector3.Down;
                else
                    return Vector3.Up;
            }
            else if (IsEdge(z))
            {
                if (z == 0)
                    return Vector3.Back;
                else
                    return Vector3.Forward;
            }
            else
                throw new System.Exception();
        }

        public void StepX(int xOffset)
        {
            if (!IsEdge(y))
            {
                x += xOffset;
            }
            else
            {
                //Is top or bottom face
                double r = GeometricMath.EuclidianDistance(floatX, floatZ);

                //Get the base angle from the pole to the point
                double theta = Math.Atan(zFloat / xFloat);

                //Rotate by an arc length of xOffset
                theta += GeometricMath.GetTheta(r, xOffset);

                z = (int)(r * Math.Sin(theta));
                x = (int)(r * Math.Cos(theta));
            }
        }

        public void StepY(int yOffset)
        {
            if (!IsEdge(y))
            {
                //Is the front/right/left/back face
                y += yOffset;
            }
            else
            {

                //Is top or bottom face
                double scale = yOffset / GeometricMath.EuclidianDistance(xFloat, zFloat);

                //If oversteps the pole
                if (scale > 1)
                {
                    xFloat = 0;
                    zFloat = 0;
                }
                else
                {
                    x -= (int)(x * scale);
                    z -= (int)(z * scale);
                }
            }
        }

        public void StepX()
        {
            //Logic for incrimenting x by 1 were more bloated than this method
            StepX(1);
        }

        public void StepY()
        {
            if (!IsEdge(y))
            {
                //Is the front/right/left/back face
                y++;
            }
            else
            {

                //Is top or bottom face
                double scale = yOffset / Math.Sqrt(xFloat * xFloat + zFloat * zFloat);

                if (scale > 1)
                {
                    xFloat = 0;
                    zFloat = 0;
                }
                else
                {
                    x -= (int)(x * scale);
                    z -= (int)(z * scale);
                }
            }

        }
    }



    class GeometricMath
    {
        public static double EuclidianDistance(int x, int y)
        {
            return Math.Sqrt(x * x + y * y);
        }

        public static CompressPointPoint(int x)
        {
            return x * 2f / Resolution - 1;
        }

        public static ExpandPoint(float x)
        {
            return x / 2 + 0.5 * Globe.Resolution;
        }

        static float getLatitude(float x, float y, float z)
        {
            throw new System.NotImplementedException();
        }

        static float getLongitude(float x, float y, float z)
        {
            throw new System.NotImplementedException();
        }

        //Returns the arc length from a given radius and angle in radians
        public static double ArcLength(float theta, float radius)
        {
            // 2pi*r * theta/2pi
            // = r*theta
            return radius * theta;
        }

        public static double GetTheta(float radius, float arcLength)
        {
            // s = r * theta
            // theta = s / r
            return arcLength / (double)radius;
        }
    }
}