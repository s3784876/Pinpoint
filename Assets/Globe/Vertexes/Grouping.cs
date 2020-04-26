using UnityEngine;

namespace Pinpoint.Globe.Vertexes
{
    public class Grouping<T> where T : IInterpolatable<T>
    {
        private T[] Parents;
        Vector2 Weights;

        public Grouping(T[] parents, Vector2 location)
        {
            this.Parents = parents;


            location.x %= 1;
            location.y %= 1;
            this.Weights = location;
        }

        public T Get()
        {
            T result = Parents[0].Interpolate(Parents[1], Weights.x);
            T opponent = Parents[2].Interpolate(Parents[3], Weights.x);

            return result.Interpolate(opponent, Weights.y);
        }
    }
}