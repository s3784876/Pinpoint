using Pinpoint.Globe.Vertexes;
using System;

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

            Console.WriteLine("All cells completed");
        }

        private void SimulateCell(AtmosphereCell currentLocal, bool isSummer)
        {
            Point<WindVertex> p;

            const int startLong = 0;
            bool goingUp = currentLocal.StartLat < currentLocal.EndLat;


            p = new Point<WindVertex>(currentLocal.StartLat, startLong, this);

            WindVertex wv;
            SeasonalWindVertex sv;
            WindLayer wl;

            float heading;
            const byte magnitude = 1;

            //Step around the great circle and simulate at every avalable point
            do
            {
                heading = currentLocal.GetHeading(p.Latitude, isSummer);
                wl = new WindLayer(magnitude, heading);

                do
                {
                    wv = GetPoint(p);

                    if (isSummer)
                        sv = wv.Summer;
                    else
                        sv = wv.Winter;

                    sv = new SeasonalWindVertex(wl);

                    p.StepX();
                } while (p.Longitude != startLong);

                //Step towards the pole
                if (goingUp)
                    p.StepY();
                else
                    p.StepY(-1);
            } while (Math.Abs(p.Latitude) <= Math.Abs(currentLocal.EndLat));

            Console.WriteLine($"Finished {(isSummer ? "Summer" : "Winter")} \t in the {currentLocal.StartLat},{currentLocal.EndLat} cell");
        }

        private void SimulatePath(Point<WindVertex> p, AtmosphereCell currentLocal)
        {
            throw new System.NotImplementedException();
        }
    }
}