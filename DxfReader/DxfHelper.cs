using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace DxfReader
{
    // Обслуживающий класс
    static class DxfHelper
    {
        #region Константы

        public const double Pi = 3.1415926535897931;            // число пи
        public const double HalfPi = 1.57079632679489655;       // 1/2 пи
        public const double TwoPi = 6.28318530717959;           // 2 пи
        public const double DegToRad = 0.017453292519943295;    // перевод градусов в радианы
        public const double RadToDeg = 57.2957795130823234;     // перевод радиан в градусы

        #endregion

        // Вывод действительных чисел в строку без учета настроек культуры
        public static string DoubleToString(double value)
        {
            double result = NormalizeDouble(value);
            return result.ToString(CultureInfo.CreateSpecificCulture("en-US"));
        }

        // Перевод строки в дейсвительное число
        public static double StringToDouble(string value)
        {
            double result;
            if (!double.TryParse(value, NumberStyles.Number | NumberStyles.AllowExponent,
                CultureInfo.CreateSpecificCulture("en-US"), out result))
                throw new ArgumentException("Значение параметра не является типом Double");
            return result;
        }

        // Установка цвета линий для отрисовки на экране
        public static Brush GetBrush(DxfColors color)
        {
            Brush b = null;
            switch (color)
            {
                case DxfColors.LineOfMarking:
                    b = Brushes.Red;
                    break;
                case DxfColors.MainOutline:
                    b = SystemColors.WindowTextBrush;
                    break;
                case DxfColors.NotClosedLine:
                    b = Brushes.Orange;
                    break;
                default:
                    b = Brushes.Gray;
                    break;
            }
            return b;
        }

        // Перевод точки DxfDot в класс Windows.Point
        public static Point DotToPoint(DxfDot dot)
        {
            return new Point(dot.X, dot.Y);
        }

        // Перевод точки DxfDot2D в класс Windows.Point
        public static Point Dot2DToPoint(DxfDot2D dot)
        {
            return new Point(dot.X, dot.Y);
        }

        // Поворот вектора на заданный угол
        public static Vector RotateVector(Vector vector, double angle)
        {
            Vector vect = new Vector
            {
                X = vector.X * Math.Cos(angle * DegToRad) - vector.Y * Math.Sin(angle * DegToRad),
                Y = vector.X * Math.Sin(angle * DegToRad) + vector.Y * Math.Cos(angle * DegToRad)
            };
            return vect;
        }

        // Нормализация углов
        public static double NormalizeAngle(double angle)
        {
            double ang = angle % 360;
            if (ang < 0)
                return 360 + ang;
            return ang;
        }

        public static double NormalizeDouble(double number)
        {
            return Math.Round(number, 5);
        }

        // Проверка битового флага
        public static bool GetFlag(ushort flags, ushort mask)
        {
            return (flags & mask) != 0;
        }

        // Установка битового флага
        public static void CheckFlag(ref ushort flags, ushort mask)
        {
            flags |= mask;
        }

        // Сброс битового флага
        public static void UncheckFlag(ref ushort flags, ushort mask)
        {
            flags &= (ushort)~mask;
        }

        // Переключение битового флага
        public static void ChangeFlag(ref ushort flags, ushort mask)
        {
            flags ^= mask;
        }
    }
}
