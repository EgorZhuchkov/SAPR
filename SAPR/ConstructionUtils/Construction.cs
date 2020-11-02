using System;
using System.Collections.Generic;
using System.Text;

namespace SAPR.ConstructionUtils
{
    class Construction
    {
        public List<Rod> Rods;
        public List<Strain> Strains;
        public bool HasRightSupport;
        public bool HasLeftSupport;

        public void Update(List<Rod> newRods, List<Strain> newStrains)
        {
            Rods = newRods;
            Strains = newStrains;
        }
    }
}
