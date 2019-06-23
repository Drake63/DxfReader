using System.Collections.Generic;

namespace DxfReader
{
    class DxfStarTangent : DxfDot
    {
        private DxfStarTangent(double x, double y, double z = 0)
            : base(x, y, z) { }
        public new static DxfStarTangent Create(List<DxfCodePair> pairs)
        {
            double x = 0, y = 0, z = 0;
            foreach(DxfCodePair pair in pairs)
                switch (pair.Code)
                {
                    case 12:
                        x = pair.AsDouble;
                        break;
                    case 22:
                        y = pair.AsDouble;
                        break;
                    case 32:
                        z = pair.AsDouble;
                        break;
                }
            return new DxfStarTangent(x, y, z);            
        }

        public override string ToString()
        {
            return string.Format("12\n{0}\n22\n{1}\n32\n{2}\n", DxfHelper.DoubleToString(X),
                DxfHelper.DoubleToString(Y), DxfHelper.DoubleToString(Z));
        }

        public override string ToStringOf2d()
        {
            return string.Format("12\n{0}\n22\n{1}\n", DxfHelper.DoubleToString(X), DxfHelper.DoubleToString(Y));
        }
    }
}
