using System.Collections.Generic;

namespace DxfReader
{
    class DxfMajorAxis : DxfEndPoint
    {
        internal DxfMajorAxis(DxfEndPoint point)
            : base(point.X, point.Y, point.Z) { }

        public new static DxfMajorAxis Create(List<DxfCodePair> pairs)
        {
            return new DxfMajorAxis(DxfEndPoint.Create(pairs));
        }
    }
}
