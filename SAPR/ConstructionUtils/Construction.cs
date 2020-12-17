using MathNet.Numerics.LinearAlgebra;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SAPR.ConstructionUtils
{
    class Construction
    {
        public List<Rod> Rods;
        public List<Strain> Strains;
        public bool HasRightSupport;
        public bool HasLeftSupport;
        public bool IsProcessed;

        [JsonIgnore]
        public Matrix<double> Ux;

        public Construction()
        {
            Rods = new List<Rod>();
            Strains = new List<Strain>();
            HasLeftSupport = true;
        }

        public bool IsValid()
        {
            if (Rods.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < Rods.Count; i++)
            {
                if (Rods[i].Length <= 0)
                {
                    return false;
                }

                if (Rods[i].Area <= 0)
                {
                    return false;
                }

                if (Rods[i].Elasticity <= 0)
                {
                    return false;
                }

                if (Rods[i].AllowedStress <= 0)
                {
                    return false;
                }
            }

            for (int i = 0; i < Strains.Count; i++)
            {
                if (Math.Abs(Strains[i].Magnitude) < Double.Epsilon)
                {
                    return false;
                }
            }

            var concentratedStrains = Strains.Where(o => o.StrainType == StrainType.Concentrated);
            var lengthwiseStrains = Strains.Where(o => o.StrainType == StrainType.Lengthwise);

            foreach (var strain in concentratedStrains)
            {
                if ((strain.NodeIndex <= 0) || (strain.NodeIndex > Rods.Count + 1))
                {
                    return false;
                }

                var concentratedStrainsInSameRod = concentratedStrains.Where(o => o.NodeIndex == strain.NodeIndex);

                if (concentratedStrainsInSameRod.Count() > 2)
                {
                    return false;
                }
                else if (concentratedStrainsInSameRod.Count() == 2)
                {
                    int xorRes = 0;
                    foreach (var item in concentratedStrainsInSameRod)
                    {
                        var sign = Math.Sign(item.Magnitude);
                        xorRes ^= sign;
                    }

                    if (xorRes >= 0)
                    {
                        return false;
                    }
                }
            }

            foreach (var strain in lengthwiseStrains)
            {
                if ((strain.NodeIndex <= 0) || (strain.NodeIndex > Rods.Count))
                {
                    return false;
                }

                var lengthwiseStrainsInSameRod = lengthwiseStrains.Where(o => o.NodeIndex == strain.NodeIndex);

                if (lengthwiseStrainsInSameRod.Count() > 1)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
