namespace Pinpoint.Assets.Globe
{
    public class AttributeGlobe<M, T> where M : AMesh<T>
    {
        protected int Resolution;

        protected M Faces = new M[6];

        private enum FIndex { up, down, right, left, back, front }

        public AttributeGlobe(int resolution)
        {
            Vector3[] ups = {
              Vector3.Up,
              Vector3.down,
              Vector3.Right,
              Vector3.left,
              Vector3.back,
              Vector3.Front
            };

            for (int i = 0; i < Faces.Length; i++)
            {
                Faces[i] = new M(ups[i], resolution);
            }
        }

        //Returns the point that a latitude and longitude are pointing to regardless of face
        protected T GetGlobalPoint(float latitude, float longitude)
        {
            M mesh = FindFace(latitude, longitude);

            return mesh.GetPoint(latitude, longitude);
        }

        //Known problems occur on the last row and last column of each face
        //TODO solve
        protected T GetGlobalInterpolatedPoint(float latitude, float longitude)
        {
            M mesh = FindFace(latitude, longitude);

            return mesh.GetInterpolatedPoint(latitude, longitude);
        }

        //Locates and returns the face responsible for the lat and long provided 
        public M FindFace(float latitude, float longitude)
        {
            return GloballyMapPoints(ref latitude, ref longitude);
        }

        //Returns the mesh responsible for the point, and returns the x,y co-ords of the point on that mesh
        public M GloballyMapPoints(ref float latitude, ref float longitude)
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
                longitude = x;
                latitude = z;

                return Faces[(int)FIndex.up + (y < 0 ? 1 : 0)];
            }
            else if (fx >= fy && fx >= fz)
            {
                //Right or left face
                longitude = z;
                latitude = y;

                return Faces[(int)FIndex.left + (x < 0 ? 1 : 0)];
            }
            else
            {
                //Front or back face
                longitude = x;
                latitude = y;

                return Faces[(int)FIndex.front + (z < 0 ? 1 : 0)];
            }
        }
    }
}