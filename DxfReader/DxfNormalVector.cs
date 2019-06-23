using System.Collections.Generic;

namespace DxfReader
{
    class DxfNormalVector : DxfDot
    {
        private DxfNormalVector(double x, double y, double z = 0.0)
            : base(x, y, z) { }

        public new static DxfNormalVector Create(List<DxfCodePair> pairs)
        {
            double x = 0, y = 0, z = 0;
            foreach(DxfCodePair pair in pairs)
                switch(pair.Code)
                {
                    case 210:
                        x = pair.AsDouble;
                        break;
                    case 220:
                        y = pair.AsDouble;
                        break;
                    case 230:
                        z = pair.AsDouble;
                        break;
                }

            return new DxfNormalVector(x, y, z);
        }

        public override string ToString()
        {
            return string.Format("210\n{0}\n220\n{1}\n230\n{0}\n", DxfHelper.DoubleToString(X),
                DxfHelper.DoubleToString(Y), DxfHelper.DoubleToString(Z));
        }

        public override string ToStringOf2d()
        {
            return string.Format("210\n{0}\n220\n{1}\n", DxfHelper.DoubleToString(X), DxfHelper.DoubleToString(Y));
        }
    }
}
