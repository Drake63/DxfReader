using System.Windows;

namespace DxfReader
{
    // Точка 3D
    class DxfDot : IDxfOffset
    {
        #region Открытые только для чтения свойства

        public double X { get; internal set; }                               // координата X
        public double Y { get; internal set; }                               // координата Y
        public double Z { get; internal set; }                               // координата Z
        public DxfDot2D AsPoint { get { return new DxfDot2D(X, Y); } }      // представление 3D координат как 2D

        public static DxfDot Zero { get { return new DxfDot(); } }          // статическое свойство, возвращающее новую точку с нулевыми координатами

        #endregion
        #region Конструкторы

        public DxfDot(double x, double y, double z = 0.0)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public DxfDot(DxfDot2D point) : this(point.X, point.Y) { }
        public DxfDot() : this(0.0, 0.0) { }
        public DxfDot(Vector vector) : this(vector.X, vector.Y) { }

        #endregion
        #region Открытые методы класса

        // Создание координат 3D из пар кодов
        public static DxfDot Create(System.Collections.Generic.List<DxfCodePair> pairs)
        {
            DxfDot dot = new DxfDot();
            foreach (DxfCodePair pair in pairs)
                switch (pair.Code)
                {
                    case 10:
                        dot.X = pair.AsDouble;
                        break;
                    case 20:
                        dot.Y = pair.AsDouble;
                        break;
                    case 30:
                        dot.Z = pair.AsDouble;
                        break;
                }
            return dot;
        }

        // Выполнение смещение координат на заданный вектор
        public virtual void OffSet(Vector vector)
        {
            Vector offset = new Vector(X, Y) - vector;
            X = offset.X;
            Y = offset.Y;
        }

        // Формирование форматированной строки для вывода в файл DXF
        public override string ToString()
        {
            return string.Format("10\n{0}\n20\n{1}\n30\n{2}\n", DxfHelper.DoubleToString(X),
                DxfHelper.DoubleToString(Y), DxfHelper.DoubleToString(Z));
        }
         public virtual string ToStringOf2d()
        {
            return AsPoint.ToString();
        }

        public virtual string ToStringOfVerR13()
        {
            return ToString();
        }
        #endregion

        public override bool Equals(object obj)
        {
            if (obj is DxfDot && obj != null)
                if (X == (obj as DxfDot).X && Y == (obj as DxfDot).Y && Z == (obj as DxfDot).Z)
                    return true;
            return false;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }

        public DxfDot Copy()
        {
            return new DxfDot(X, Y, Z);
        }
    }
}