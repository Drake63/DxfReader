using System.Collections.Generic;

namespace DxfReader
{
    // Контрольная точка сплайна
    class DxfControlPoint : DxfDot
    {
        // Вес точки по умолчанию равен 1.0
        public double Weight { get; private set; }          

        // Конвертация контрольной точки сплайна в вершину полилинии
        public DxfVertex AsVertex => new DxfVertex(X, Y, Z);

        // Конструктор класса
        public DxfControlPoint(double x, double y, double z = 0.0, double w=1.0)
            :base(x,y,z)
        {
            Weight = w;
        }

        // Статический метод создания контрольной точки из пар кодов файла DXF
        public new static DxfControlPoint Create(List<DxfCodePair> pairs)
        {
            double x = 0, y = 0, z = 0, w = 1;

            foreach(DxfCodePair pair in pairs)
                switch (pair.Code)
                {
                    case 10:
                        x = pair.AsDouble;
                        break;
                    case 20:
                        y = pair.AsDouble;
                        break;
                    case 30:
                        z = pair.AsDouble;
                        break;
                    case 41:
                        w = pair.AsDouble;
                        break;
                }

            return new DxfControlPoint(x, y, z, w);
        }

        // Форматированная строка для вывод в файл DXF
        public string ToString(bool isWeight)
        {
            if (!isWeight) return base.ToString();
            return "41\n" + DxfHelper.DoubleToString(Weight) + "\n" + base.ToString();
        }

        // Форматированная строка 2D для вывода в файл DXF
        public string ToStringOf2d(bool isWeight)
        {
            if (!isWeight) return base.ToStringOf2d();
            return "41\n" + DxfHelper.DoubleToString(Weight) + "\n" + base.ToStringOf2d();
        }

        // Строка для вывода в файл версии R13
        public override string ToStringOfVerR13()
        {
            return ToString();
        }

        // Сравнение двух контрольных точек
        public override bool Equals(object obj)
        {
            if (base.Equals(obj as DxfDot) && Weight == (obj as DxfControlPoint).Weight)
                return true;
            return false;
        }

        // Расчет хеш-кода
        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Weight.GetHashCode();
        }

        // Перегрузка оператора умножения действительного числа на контрольную точку
        public static DxfControlPoint operator *(double num, DxfControlPoint point) => new DxfControlPoint(point.X * num, point.Y * num, point.Z * num);
    }
}
