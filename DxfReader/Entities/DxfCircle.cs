using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace DxfReader
{
    // Секция CIRCLE
    class DxfCircle : IDxfEntity
    {
        // название секции всегда должно быть постоянным
        const string name = EntitiesVariableCode.Circle;

        #region Открытые только для чтения свойства

        public DxfColors Color { get; private set; }    // обозначение цвета линии окружности
        public string Name { get { return name; } }     // имя секции
        public DxfDot Center { get; private set; }      // центр откужности
        public double Radius { get; private set; }      // радиус окружности
        public Rect Bound { get; private set; }

        #endregion
        #region Конструктор, нельзя создавать класс непосредственно с помощью конструктора

        public DxfCircle() { }

        #endregion
        #region Открытые методы

        // Создание класса из пар кодов
        public static DxfCircle Create(List<DxfCodePair> pairs, bool ignoreLineType)
        {
            DxfCircle circle = new DxfCircle();

            for (int i = 0; i < pairs.Count; i++)
            {
                switch (pairs[i].Code)
                {
                    case 62:
                        circle.Color = DxfColor.Create(pairs[i], ignoreLineType);
                        break;
                    case 10:
                        if (pairs[i + 2].Code == 30)
                        {
                            circle.Center = DxfDot.Create(pairs.GetRange(i, 3));
                            i += 2;
                        }
                        else
                        {
                            circle.Center = DxfDot.Create(pairs.GetRange(i, 2));
                            i++;
                        }
                        break;
                    case 40:
                        circle.Radius = pairs[i].AsDouble;
                        break;
                }
            }

            if (circle.Color == 0 && ignoreLineType)
                circle.Color = DxfColors.MainOutline;
            else if (circle.Color == 0 || circle.Color == DxfColors.NoColor)
                return null;

            Rect rt = new Rect(circle.Center.X - circle.Radius, circle.Center.Y - circle.Radius,
                circle.Radius * 2, circle.Radius * 2);
            circle.Bound = rt;

            return circle;
        }

        // Формирование строки для вывода в файл DXF
        public override string ToString()
        {
            return string.Format("0\n{0}\n8\n0\n{1}40\n{2}\n62\n{3}\n", Name, Center.AsPoint.ToString(),
                DxfHelper.DoubleToString(Radius), (int)Color);
        }

        // Создание визуального представления окружности
        public DrawingVisual CreateVisual(double scale)
        {
            DrawingVisual visual = new DrawingVisual();
            double radius = Radius * scale;
            Point center = new Point(Center.X * scale, Center.Y * scale);
            using (DrawingContext dc = visual.RenderOpen())
            {
                dc.DrawEllipse(null, new Pen(DxfHelper.GetBrush(Color), 2.0), center, radius, radius);
            }
            return visual;
        }

        // Смещение координат на заданный вектор
        public void OffSet(Vector vector)
        {
            Center.OffSet(vector);
        }

        public string ToStringOfVerR13()
        {
            string str = ToString();
            str = str.Insert(str.IndexOf(Center.AsPoint.ToString()), "100\nAcDbCircle\n");
            return str;
        }

        #endregion
    }
}
