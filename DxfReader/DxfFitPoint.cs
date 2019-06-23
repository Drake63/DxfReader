using System.Collections.Generic;

namespace DxfReader
{
    // Управляющая точка класса Spline
    class DxfFitPoint : DxfEndPoint
    {
        // внутренний конструктор
        internal DxfFitPoint(DxfEndPoint point)
            : base(point.X, point.Y, point.Z) { }

        // создание точки из пар файла DXF
        public new static DxfFitPoint Create(List<DxfCodePair> pairs)
        {
            return new DxfFitPoint(DxfEndPoint.Create(pairs));
        }
    }
}
