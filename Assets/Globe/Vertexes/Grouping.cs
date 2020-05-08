using UnityEngine;

namespace Pinpoint.Globe.Vertexes
{
    public class Grouping<T> where T : IInterpolatable<T>
    {
        private T[] Parents;
        Vector2 Weights;

        
        public Grouping(T[] parents, Vector2 index)
        {
            this.Parents = parents;


            index.x %= 1;
            index.y %= 1;
            this.Weights = index;
        }

        public Grouping(T parent)
        {
            Parents = new T[4];

            for (int i = 0; i < Parents.Length; i++)
            {
                Parents[i] = parent;
            }

            Weights = Vector2.zero;
        }

        public T Get()
        {
            T result = Parents[0].Interpolate(Parents[1], Weights.x);
            T opponent = Parents[2].Interpolate(Parents[3], Weights.x);

            return result.Interpolate(opponent, Weights.y);
        }
    }
}