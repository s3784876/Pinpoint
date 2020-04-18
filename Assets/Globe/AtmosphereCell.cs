using System;

namespace Pinpoint.Globe
{
    public class AtmosphereCell
    {
        public int StartLat;
        public int EndLat;
        private float StartHeading;
        private float EndHeading;

        private float Gradient;

        float Strength = 0.2f;

        public AtmosphereCell(int startLat, int endLat, float startHeading, float endHeading, float strength)
            : this(startLat, endLat, startHeading, startHeading, endHeading)
        {
            Strength = strength;
        }
        public AtmosphereCell(int startLat, int endLat, float startHeading, float endHeading)
        {
            StartLat = startLat;
            EndLat = endLat;
            StartHeading = startHeading;
            EndHeading = endHeading;

            Gradient = (endHeading - startHeading) / (endLat - startLat);
        }

        public float GetHeading(float lat)
        {
            //Ensure that the point is in the range.
            if (lat > EndLat || lat < StartLat)
                throw new ArgumentOutOfRangeException();

            //Plot heading as a liniar function of latitude, resulting in a parabolic path along a flat surface
            return ((lat - StartLat) * Gradient + StartHeading) % 360;
        }

        public float GetHeading(float lat, bool isSummer)
        {
            if (isSummer)
                return GetHeading(lat - 30);
            else
                return GetHeading(lat + 30);
        }
    }
}