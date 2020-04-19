using Pinpoint.Globe.Vertexes;
using Pinpoint.Globe.Faces;
using UnityEngine;

namespace Pinpoint.Globe
{
    public abstract class AttributeGlobe<T> where T : IInterpolatable<T>
    {
        public readonly int Resolution;

        protected Faces.Mesh<T>[] Faces = new Faces.Mesh<T>[6];

        private enum FIndex { up, down, right, left, back, front }

        public AttributeGlobe(int resolution)
        {
            Vector3[] ups = {
              Vector3.up,
              Vector3.down,
              Vector3.right,
              Vector3.left,
              Vector3.back,
              Vector3.forward
            };

            for (int i = 0; i < Faces.Length; i++)
            {
                Faces[i] = new Faces.Mesh<T>(ups[i], resolution);
            }
        }

        //Returns the point that a latitude and longitude are pointing to regardless of face
        protected T GetGlobalCoordinant(float latitude, float longitude)
        {
            Faces.Mesh<T> mesh = FindFace(latitude, longitude);

            return Faces.Mesh<T>.GetPoint(latitude, longitude);
        }

        //Known problems occur on the last row and last column of each face
        //TODO solve
        protected T GetGlobalInterpolatedCoordinant(float latitude, float longitude)
        {
            Faces.Mesh<T> mesh = FindFace(latitude, longitude);

            return Faces.Mesh<T>.GetInterpolatedPoint(latitude, longitude);
        }

        //Locates and returns the face responsible for the lat and long provided 
        public Faces.Mesh<T> FindFace(float latitude, float longitude)
        {
            return GloballyMapCoordinants(ref latitude, ref longitude);
        }

        //Returns the Faces.Mesh<T> responsible for the point, and returns the x,y co-ords of the point on that mesg
        public Faces.Mesh<T> GloballyMapCoordinants(ref float latitude, ref float longitude)
        {
            var f;

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

                f = Faces[(int)FIndex.up + (y < 0 ? 1 : 0)];
            }
            else if (fx >= fy && fx >= fz)
            {
                //Right or left face
                longitude = z;
                latitude = y;

                f = Faces[(int)FIndex.left + (x < 0 ? 1 : 0)];
            }
            else
            {
                //Front or back face
                longitude = x;
                latitude = y;

                f = Faces[(int)FIndex.front + (z < 0 ? 1 : 0)];
            }

            longitude = Clamp(longitude);
            latitude = Clamp(latitude);

            return f;
        }

        //maps value x [-1,1] to [0,resolution]
        private Clamp(float x)
        {
            return (x / 2 + 0.5) * Resolution;
        }

        public T GetPoint(int x, int y, Vector3 localUp)
        {
            foreach (var item in Faces)
            {
                if (item.LocalUp == localUp)
                    return item.GetPoint(x, y);
            }
        }

        protected T GetPoint(int x, int y, int xOffset, int yOffset)
        {

        }

        //Called to perform all the calculations to get the mesh to a completed state
        public abstract void Simulate();
    }
}