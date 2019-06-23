using System.Collections.Generic;
using System.Windows.Media;
using System.Windows;

namespace DxfReader
{
    // Секция POINT
    class DxfPoint : IDxfEntity
    {
        // имя секции всегда должно быть постоянным
        const string name = EntitiesVariableCode.Point;

        #region Открытые только для чтения свойства

        public string Name { get { return name; } }         // имя секции
        public DxfColors Color { get; private set; }        // цвет точки
        public DxfDot Coordinates { get; private set; }     // координаты точки
        public Rect Bound { get; private set; }

        #endregion
        #region Конструктор, нельзя создавать класс его вызовом

        private DxfPoint(DxfDot dot, DxfColors color = DxfColors.NotClosedLine)
        {
            Coordinates = dot;
            Color = color;
        }

        private DxfPoint() { }

        #endregion
        #region Открытые методы

        // Создание точки из пар кодов
        public static DxfPoint Create(List<DxfCodePair> pairs, bool ignoreLineType)
        {
            DxfPoint point = new DxfPoint();

            for(int i=0;i<pairs.Count;i++)
                switch(pairs[i].Code)
                {
                    case 62:
                        point.Color = DxfColor.Create(pairs[i], ignoreLineType);
                        break;
                    case 10:
                        if((i+2)<pairs.Count && pairs[i+2].Code==30)
                        {
                            point.Coordinates = DxfDot.Create(pairs.GetRange(i, 3));
                            i += 2;
                        }
                        else
                        {
                            point.Coordinates = DxfDot.Create(pairs.GetRange(i, 2));
                            i++;
                        }
                        break;
                }

            if (point.Color == 0 && !ignoreLineType)
                point.Color = DxfColors.MainOutline;
            if (point.Color == 0 || point.Color == DxfColors.NoColor)
                return null;

            Rect rt = new Rect(point.Coordinates.X, point.Coordinates.Y, 1, 1);
            point.Bound = rt;
            return point;
        }

        // Формирование строки для вывода в файл
        public override string ToString()
        {
            return string.Format("0\n{0}\n8\n0\n{1}62\n{2}\n", Name, Coordinates.AsPoint.ToString(), (int)Color);
        }

        // Создание визуального представления точки
        public DrawingVisual CreateVisual(double scale)
        {
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext dc = visual.RenderOpen())
            {
                dc.DrawRectangle(DxfHelper.GetBrush(Color), null, new Rect(new Point(Coordinates.X * scale - 1, Coordinates.Y * scale - 1), new Size(2, 2)));
            }
            return visual;
        }

        // Смещение координат на заданный вектор
        public void OffSet(Vector vector)
        {
            Coordinates.OffSet(vector);
        } 

        public void SetColor(DxfColors color)
        {
            Color = color;
        }

        public string ToStringOfVerR13()
        {
            string str = ToString();
            str = str.Insert(str.IndexOf(Coordinates.AsPoint.ToString()), "100\nAcDbPoint\n");
            return str;
        }

        #endregion
    }
}
