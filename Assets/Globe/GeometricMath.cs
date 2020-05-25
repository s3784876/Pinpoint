using System;

namespace Pinpoint.Globes
{
    public static class GeometricMath
    {
        public static double EuclidianDistance(int x, int y)
        {
            return Math.Sqrt(x * x + y * y);
        }

        public static double EuclidianDistance(float x, float y)
        {
            return Math.Sqrt(x * x + y * y);
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

        public static double GetTheta(double radius, float arcLength)
        {
            // s = r * theta
            // theta = s / r
            return arcLength / radius;
        }

        public static (int, int, int) GetXYZ(float latitude, float longitude)
        {
            latitude = (float)(Math.PI * latitude / 180);
            longitude = (float)(Math.PI * longitude / 180);

            float y = (float)Math.Sin(latitude),
            x = (float)Math.Cos(longitude) * y,
            z = (float)Math.Sin(longitude) * y;

            double fx, fy, fz;
            fx = Math.Abs(x);
            fy = Math.Abs(y);
            fz = Math.Abs(z);

            if (fy >= fx && fy >= fz)
            {
                // top or bottom face
                y = (y > 0) ? 1 : -1;

            }
            else if (fx >= fy && fx >= fz)
            {
                //Right or left face
                x = (x > 0) ? 1 : -1;
            }
            else
            {
                //Front or back face
                z = (z > 0) ? 1 : -1;
            }
            MidpointRounding m = MidpointRounding.AwayFromZero;

            int ix = (int)Math.Round(x, m), iy = (int)Math.Round(y, m), iz = (int)Math.Round(z, m);


            return (ix, iy, iz);
        }

        public static (float, float) GetLatLong(float x, float y, float z)
        {
            float lat = (float)Math.Atan(z / x),
            lon = (float)Math.Atan(y / EuclidianDistance(x, z));

            return (lat, lon);


        }
    }
}