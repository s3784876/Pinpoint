namespace Pinpoint.Globe
{
    public abstract class AAttributeGlobe<T> where T : ISupersamplable
    {
        protected int Resolution;

        protected Mesh Faces = new Mesh[6];

        private enum FIndex { up, down, right, left, back, front }

        public AAttributeGlobe(int resolution)
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
                Faces[i] = new Mesh(ups[i], resolution);
            }
        }

        //Returns the point that a latitude and longitude are pointing to regardless of face
        protected T GetGlobalCoordinant(float latitude, float longitude)
        {
            Mesh mesh = FindFace(latitude, longitude);

            return mesh.GetPoint(latitude, longitude);
        }

        //Known problems occur on the last row and last column of each face
        //TODO solve
        protected T GetGlobalInterpolatedCoordinant(float latitude, float longitude)
        {
            Mesh mesh = FindFace(latitude, longitude);

            return mesh.GetInterpolatedPoint(latitude, longitude);
        }

        //Locates and returns the face responsible for the lat and long provided 
        public Mesh FindFace(float latitude, float longitude)
        {
            return GloballyMapCoordinants(ref latitude, ref longitude);
        }

        //Returns the mesh responsible for the point, and returns the x,y co-ords of the point on that mesh
        public Mesh GloballyMapCoordinants(ref float latitude, ref float longitude)
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

        public Mesh GloballyMapPoints(int x, int y)
        {
            ClampPoints(ref x, ref y);

            if (x / Resolution == 0 && y / Resolution == 0)
                return Faces[FIndex.front];

            else if (x / Resolution == -1 && y / Resolution == 0)
                return Faces[FIndex.left];

            else if (x / Resolution == 1 && y / Resolution == 0)
                return Faces[FIndex.right];

            else if (x / Resolution == 2 && y / Resolution == 0)
                return Faces[FIndex.back];

            else if (x / Resolution == 0 && y / Resolution == 1)
                return Faces[FIndex.up];

            else if (x / Resolution == 0 && y / Resolution == -1)
                return Faces[FIndex.down];

            else
                throw new System.ArgumentOutOfRangeException();
        }

        private void ClampPoints(ref int x, ref int y)
        {
            //Up from top
            if (y > 2 * Resolution)
            {
                y = Resolution * 3 - y;
                x = Resolution * 3 - x;
            }

            //Up from left
            if (y > Resolution && x < 0)
            {
                int i = y;
                y = x + Resolution * 2;
                x = i - Resolution;
            }

            //Up from right
            if (y > Resolution && x / Resolution == 1)
            {
                int i = y;
                y = x;
                x = i - Resolution;
            }

            //Up from back
            if (y > Resolution && x > 2 * Resolution)
            {
                y = Resolution * 3 - y;
                x = Resolution * 3 - x;
            }

            //Down from left
            //TODO

            //Down from bottom


            //Down from right


            //Down from back


            //Left from left


            //Left from top


            //Left from bottom


            //Right From top


            //Right from bottom


            //Right from back
        }

        //Called to perform all the calculations to get the mesh to a completed state
        public abstract void Simulate();
    }
}