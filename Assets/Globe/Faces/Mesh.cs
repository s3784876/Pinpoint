using System;
using UnityEngine;

namespace Pinpoint.Globe.Faces
{
    public class Mesh<T> where T : Pinpoint.Globe.ISupersampleable<T>
    {
        T[] Vertexes;
        Vector3 LocalUp;

        int Resolution;

        //Constructor
        protected Mesh(Vector3 localUp, int resolution)
        {
            this.Vertexes = new T[resolution * resolution];
            LocalUp = localUp;
            Resolution = resolution;

            for (int i = 0; i < Vertexes.Length; i++)
            {
                Vertexes[i] = new T();
            }
        }

        //Returns the pont located at the given co-ordinates
        protected T GetPoint(float latitude, float longitude)
        {
            MapPoint(ref latitude, ref longitude);
            int index = (int)latitude + (int)(longitude * Resolution);

            return GetPoint(index);
        }

        //Returns vertexes[i]
        public T GetPoint(int index)
        {
            return Vertexes[index];
        }

        //Returns the aproximation of the conditions at the given lat and long by returning a weighted average of the points in the region
        protected T GetInterpolatedPoint(float latitude, float longitude)
        {
            MapPoint(ref latitude, ref longitude);

            T[] items = new T[4];
            Vector2Int[] shifts = {
              new Vector2Int(0,0),
              new Vector2Int(1,0),
              new Vector2Int(0,1),
              new Vector2Int(1,0),
            };

            int baseIndex = (int)latitude + (int)(longitude * Resolution);

            for (int i = 0; i < items.Length; i++)
                items[i] = GetPoint(baseIndex + shifts[i].x + shifts[i].y * Resolution);

            //merge nodes along x direction
            for (int i = 0; i < items.Length; i += 2)
                items[i / 2] = items[i].Interpolate(items[i + 1], latitude % 1);

            //Merge nodes along y direction
            for (int i = 0; i < items.Length / 2; i += 2)
                items[i / 2] = items[i].Interpolate(items[i + 1], longitude % 1);

            return items[0];
        }

        //Formats the latitude and longitude to represent the percentage that the target is along the x and y of the face.
        private void MapPoint(ref float latitude, ref float longitude)
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

                if (LocalUp != Vector3.up && LocalUp != Vector3.down)
                    throw new IndexOutOfRangeException();
            }
            else if (fx >= fy && fx >= fz)
            {
                //Right or left face
                longitude = z;
                latitude = y;

                if (LocalUp != Vector3.right && LocalUp != Vector3.left)
                    throw new IndexOutOfRangeException();
            }
            else
            {
                //Front or back face
                longitude = x;
                latitude = y;

                if (LocalUp != Vector3.forward && LocalUp != Vector3.back)
                    throw new IndexOutOfRangeException();
            }
        }
    }
}