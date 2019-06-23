using System.Collections.Generic;

namespace DxfReader
{
    class DxfEndTangent : DxfDot
    {
        private DxfEndTangent(double x, double y, double z = 0.0)
            : base(x, y, z) { }

        public new static DxfEndTangent Create(List<DxfCodePair> pairs)
        {
            double x = 0, y = 0, z = 0;
            foreach(DxfCodePair pair in pairs)
                switch(pair.Code)
                {
                    case 13:
                        x = pair.AsDouble;
                        break;
                    case 23:
                        y = pair.AsDouble;
                        break;
                    case 33:
                        z = pair.AsDouble;
                        break;
                }

            return new DxfEndTangent(x, y, z);
        }

        public override string ToString()
        {
            return string.Format("13\n{0}\n23\n{1}\n33\n{2}\n", DxfHelper.DoubleToString(X),
                DxfHelper.DoubleToString(Y), DxfHelper.DoubleToString(Z));
        }

        public override string ToStringOf2d()
        {
            return string.Format("13\n{0}\n23\n{1}\n", DxfHelper.DoubleToString(X), DxfHelper.DoubleToString(Y));
        }
    }
}
