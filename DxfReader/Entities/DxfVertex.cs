using System.Collections.Generic;
using System.Windows;

namespace DxfReader
{
    // Класс Vertex - вершины полилинии
    class DxfVertex : DxfDot
    {
        // имя секции должно быть всегда постоянным
        const string name = EntitiesVariableCode.Vertex;

        #region Открытые только для чтения свойства класса

        public string Name { get { return name; } }                         // имя объекта в файле
        public double Bulge { get; set; }                                   // искривление - тангенс 1/4 угла дуги
        public DxfDot AsDot => new DxfDot(X, Y, Z);                         // координаты точки вершины
        public new static DxfVertex Zero => new DxfVertex(DxfDot.Zero);     // возвращает вершину с нулевыми координатами

        #endregion
        #region Конструктор класса

        internal DxfVertex(DxfDot point, double bulge = 0)
            : this(point.X, point.Y, point.Z, bulge) { }

        internal DxfVertex(double x, double y, double z = 0.0, double bulge = 0.0)
            : base(x, y, z)
        {
            Bulge = bulge;
        }

        internal DxfVertex(Vector vector, double bulge = 0)
            : base(vector)
        {
            Bulge = bulge;
        }

        internal DxfVertex() { }

        #endregion
        #region Открытые методы класса
        
        // Создание вершины из пар кодов
        public new static DxfVertex Create(List<DxfCodePair> pairs)
        {
            double bulge = 0;
            DxfDot dot = null;
            for(int i=0;i<pairs.Count; i++)
                switch(pairs[i].Code)
                {
                    case 10:
                        if((i+2)< pairs.Count && pairs[i+2].Code==30)
                        {
                            dot = DxfDot.Create(pairs.GetRange(i, 3));
                            i += 2;
                        }
                        else
                        {
                            dot = DxfDot.Create(pairs.GetRange(i, 2));
                            i++;
                        }
                        break;
                    case 42:
                        bulge = pairs[i].AsDouble;
                        break;
                }

            return new DxfVertex(dot, bulge);
        }

        // Форматированная строка для вывода в файл DXF
        public override string ToString()
        {
            string str = string.Format("0\n{0}\n8\n0\n{1}", Name, base.ToString());
            if (Bulge != 0)
                return str += "42\n" + DxfHelper.DoubleToString(Bulge) + "\n";
            return str;
        }

        // Форматированная строка для вывода в файл 2D
        public override string ToStringOf2d()
        {
            string str = string.Format("0\n{0}\n8\n0\n{1}", Name, base.ToStringOf2d());
            if (Bulge != 0)
                return str += "42\n" + DxfHelper.DoubleToString(Bulge) + "\n";
            return str;
        }

        // Фоматированная строка для вывода в файл версии R13
        public override string ToStringOfVerR13()
        {
            string str = ToString();
            str = str.Insert(str.IndexOf("10\n" + DxfHelper.DoubleToString(X)), "100\nAcDb2dVertex\n");
            return str;
        }

        // Сравнивание двух врешин
        public override bool Equals(object obj)
        {
            if (obj is DxfVertex && obj != null)
                if (base.Equals(obj as DxfDot) && Bulge == (obj as DxfVertex).Bulge)
                    return true;
            return false;
        }

        // Возвращает хеш-код вершины
        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Bulge.GetHashCode();
        }

        #endregion

        #region Перегрузка унарных операций с вершинами

        // складывание двух вершин
        public static DxfVertex operator +(DxfVertex vert1, DxfVertex vert2) => new DxfVertex(vert1.X + vert2.X, vert1.Y + vert2.Y, vert1.Z + vert2.Z);
        // умножение числа на вершину
        public static DxfVertex operator *(double num, DxfVertex vert) => new DxfVertex(vert.X * num, vert.Y * num, vert.Z * num);
        // умножение вершины на число
        public static DxfVertex operator *(DxfVertex vert, double num) => new DxfVertex(vert.X * num, vert.Y * num, vert.Z * num);

        #endregion
    }
}
