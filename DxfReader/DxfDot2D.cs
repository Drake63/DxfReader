using System.Windows;
using System.Collections.Generic;

namespace DxfReader
{
    // 2D точка
    class DxfDot2D : IDxfOffset
    {
        #region Открытые только для чтения свойства

        public double X { get; private set; }
        public double Y { get; private set; }

        #endregion
        #region Конструкторы

        public DxfDot2D(double x, double y)
        {
            X = x;
            Y = y;
        }
        public DxfDot2D() : this(0.0, 0.0) { }

        #endregion
        #region Открытые методы

        // Создание 2D координат из пар кодов
        public static DxfDot2D Create(List<DxfCodePair> pairs)
        {
            DxfDot2D point = new DxfDot2D();
            foreach (DxfCodePair pair in pairs)
                switch (pair.Code)
                {
                    case 10:
                        point.X = pair.AsDouble;
                        break;
                    case 20:
                        point.Y = pair.AsDouble;
                        break;
                }
            return point;
        }

        // Смещение координат 2D точки на заданный вектор
        public virtual void OffSet(Vector vector)
        {
            Vector offset = new Vector(X, Y) - vector;
            X = offset.X;
            Y = offset.Y;
        }

        // Создание форматированной строки для вывода в файл DXF
        public override string ToString()
        {
            string str = string.Format("10\n{0}\n20\n{1}\n", DxfHelper.DoubleToString(X), DxfHelper.DoubleToString(Y));
            return str;
        }

        public string ToStringOfVerR13()
        {
            return ToString();
        }

        #endregion
    }
}