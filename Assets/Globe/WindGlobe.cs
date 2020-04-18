using Pinpoint.Globe.Vertexes;

namespace Pinpoint.Globe
{
    public class WindGlobe : AAttributeGlobe<WindVertex>
    {
        private AtmosphereCell[] Cells = new AtmosphereCell[4];

        public WindGlobe()
        {

        }

        public override void Simulate()
        {
            //throw new System.NotImplementedException();
            float lat = 0, lon = 0;

            for (byte season = 0; season < 2; season++)
            {
                //TYODO FIX INPUTS
                //Simulate Ferrel Cell in the northern Hemisphere
                SimulateCell(Cell.FERREL_FROM_CAPRICORN, season == 0);


                //Simulate Hadley Cell in the northern Hemisphere
                SimulateCell(Cell.HADLEY_IN_CAPRICORN, season == 0);


                //Simulate Hadley Cell in the Southern Hemisphere
                SimulateCell(Cell.HADLEY_IN_CANCER, season == 0);


                //Simulate Ferrel Cell in the Southern Hemisphere
                SimulateCell(Cell.FERREL_FROM_CANCER, season == 0);
            }
        }

        private enum Cell
        {
            POLAR_FROM_ARCTIC,
            FERREL_FROM_CANCER,
            HADLEY_IN_CANCER,

            HADLEY_IN_CAPRICORN,
            FERREL_FROM_CAPRICORN,
            POLAR_FROM_ANTARCTIC,
        }

        private void SimulateCell(Cell currentLocal, bool isSummer)
        {
            lat = GetLat(currentLocal, isSummer);

            for (int i = 0; i < Faces[0].Resolution; i++)
            {
                GetGlobalPoint(lat, 0);
            }
        }

        //TODO manage polar cells and work out what the hell is going on with them during summer and winter
        private float GetLat(Cell local, bool isSummer)
        {
            /* 
            
                    SUMMER  |   WINTER
            FerrelN 90      |   60
            HadleyN 60      |   0
            HadleyS 0       |   -30
            FerrelS -30     |   -90
            
            */

            int index = (int)local * -1 + 2;
            index *= 30;
            index -= (index < 0) ? 30 : 0;

            return index + (isSummer ? 60 : 0);
        }

        private float heading(float lat, Cell cell)
        {

        }
    }
}