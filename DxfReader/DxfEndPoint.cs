using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DxfReader
{
    class DxfEndPoint : DxfDot
    {
        internal DxfEndPoint(double x, double y, double z = 0.0)
            : base(x, y, z) { }

        public new static DxfEndPoint Create(List<DxfCodePair> pairs)
        {
            double x = 0, y = 0, z = 0;
            foreach(DxfCodePair pair in pairs)
                switch(pair.Code)
                {
                    case 11:
                        x = pair.AsDouble;
                        break;
                    case 21:
                        y = pair.AsDouble;
                        break;
                    case 31:
                        z = pair.AsDouble;
                        break;
                }
            return new DxfEndPoint(x, y, z);
        }

        public override string ToString()
        {
            return string.Format("11\n{0}\n21\n{1}\n31\n{2}\n", DxfHelper.DoubleToString(X),
                DxfHelper.DoubleToString(Y), DxfHelper.DoubleToString(Z));
        }

        public override string ToStringOf2d()
        {
            return string.Format("11\n{0}\n21\n{1}\n", DxfHelper.DoubleToString(X), DxfHelper.DoubleToString(Y));
        }

        public static DxfEndPoint EndPointFromDot(DxfDot dot)
        {
            return new DxfEndPoint(dot.X, dot.Y, dot.Z);
        }
    }
}
