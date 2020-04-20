using Pinpoint.Globe.Vertexes;

namespace Pinpoint.Globe
{
    public class WindGlobe : AttributeGlobe<WindVertex>
    {
        private AtmosphereCell[] Cells = new AtmosphereCell[4];

        public WindGlobe(int resolution) : base(resolution)
        {
            //Ferrel cell in northern hemisphere
            Cells[0] = new AtmosphereCell(30, 60, 0, 90);

            //Hadley cell in north hemisphere
            Cells[1] = new AtmosphereCell(30, 0, 180, 270);

            //Hadley Cell in southern Hemisphere
            Cells[2] = new AtmosphereCell(-30, 0, 360, 270);

            //Ferrel cell in southern hemisphere
            Cells[3] = new AtmosphereCell(-60, -30, 180, 90);
        }

        public override void Simulate()
        {
            //Simulate summer and winter
            for (byte season = 0; season < 2; season++)

                //Simulate each cell
                foreach (var cell in Cells)
                    SimulateCell(cell, season == 0);
        }

        private void SimulateCell(AtmosphereCell currentLocal, bool isSummer)
        {
            Point p;

            const int startLong = 0;

            p = new Point(currentLocal.startLat, startLong);


            //Step around the great circle and simulate at every avalable point
            do
            {
                SimulatePath(p);
                p.StepX();
            } while (p.Longitude != startLong);
        }

        private void SimulatePath(Point p, AtmosphereCell currentLocal)
        {
            WindVertex wv = FindPoint(p);

            throw new System.NotImplementedException();
        }
    }
}