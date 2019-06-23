namespace DxfReader
{
    // Узел сплайна
    class DxfKnot
    {
        const string CODE = "40";   // Код узла в файле DXF
        double value;               // значение узла

        public double Knot => value;    // открытое только для чтения значение узла

        // Конструктор класса, в качестве параметра принимает значение узла
        public DxfKnot(double value)
        {
            this.value = value;
        }

        // строка для вывода в файл DXF
        public override string ToString()
        {
            return string.Format("{0}\n{1}\n", CODE, DxfHelper.DoubleToString(value));
        }

        // сравнеине двух узлов
        public override bool Equals(object obj)
        {
            if (obj is DxfKnot && obj != null)
                if (value == (obj as DxfKnot).Knot)
                    return true;
            return false;
        }

        // хеш-код узла
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public static double operator +(DxfKnot knot1, DxfKnot knot2) => knot1.Knot + knot2.Knot;
        public static double operator +(DxfKnot knot, double num) => knot.Knot + num;
        public static double operator +(double num, DxfKnot knot) => num + knot.Knot;

        public static double operator -(DxfKnot knot1, DxfKnot knot2) => knot1.Knot - knot2.Knot;
        public static double operator -(DxfKnot knot, double num) => knot.Knot - num;
        public static double operator -(double num, DxfKnot knot) => num - knot.Knot;

        public static double operator *(DxfKnot knot1, DxfKnot knot2) => knot1.Knot * knot2.Knot;
        public static double operator *(DxfKnot knot, double num) => knot.Knot * num;
        public static double operator *(double num, DxfKnot knot) => num * knot.Knot;

        public static double operator /(DxfKnot knot1, DxfKnot knot2) => knot1.Knot / knot2.Knot;
        public static double operator /(DxfKnot knot, double num) => knot.Knot / num;
        public static double operator /(double num, DxfKnot knot) => num / knot.Knot;

        public static bool operator <(DxfKnot knot1, DxfKnot knot2) => knot1.Knot < knot2.Knot;
        public static bool operator >(DxfKnot knot1, DxfKnot knot2) => knot1.Knot > knot2.Knot;

        public static bool operator <(DxfKnot knot, double num) => knot.Knot < num;
        public static bool operator >(DxfKnot knot, double num) => knot.Knot > num;

        public static bool operator <(double num, DxfKnot knot) => num < knot.Knot;
        public static bool operator >(double num, DxfKnot knot) => num > knot.Knot;

        public static bool operator <=(DxfKnot knot1, DxfKnot knot2) => knot1.Knot <= knot2.Knot;
        public static bool operator >=(DxfKnot knot1, DxfKnot knot2) => knot1.Knot >= knot2.Knot;

        public static bool operator <=(DxfKnot knot, double num) => knot.Knot <= num;
        public static bool operator >=(DxfKnot knot, double num) => knot.Knot >= num;

        public static bool operator <=(double num, DxfKnot knot) => num <= knot.Knot;
        public static bool operator >=(double num, DxfKnot knot) => num >= knot.Knot;

        public static bool operator ==(DxfKnot knot1, DxfKnot knot2) => knot1.Knot == knot2.Knot;
        public static bool operator !=(DxfKnot knot1, DxfKnot knot2) => knot1.Knot != knot2.Knot;

        public static bool operator ==(DxfKnot knot, double num) => knot.Knot == num;
        public static bool operator !=(DxfKnot knot, double num) => knot.Knot != num;

        public static bool operator ==(double num, DxfKnot knot) => num == knot.Knot;
        public static bool operator !=(double num, DxfKnot knot) => num != knot.Knot;
    }
}
