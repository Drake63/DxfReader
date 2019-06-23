namespace DxfReader
{
    // Статический класс с константами имен графичеких объектов в файле DXF
    static class EntitiesVariableCode
    {
        public const string Arc = "ARC";                // дуга
        public const string Circle = "CIRCLE";          // окружность
        public const string Ellipse = "ELLIPSE";        // эллипс
        public const string Line = "LINE";              // линия
        public const string Point = "POINT";            // точка
        public const string Polyline = "POLYLINE";      // полилиния
        public const string LWPolyline = "LWPOLYLINE";  // полилиния
        public const string Spline = "SPLINE";          // сплайн
        public const string Vertex = "VERTEX";          // вершины полилинии
        public const string Insert = "INSERT";          // вставка объекта
        public const string Seqend = "SEQEND";          // конец полилинии POLYLINE
    }
}
