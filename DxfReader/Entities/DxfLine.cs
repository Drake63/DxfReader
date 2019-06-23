using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System;

namespace DxfReader
{
    // Линия DXF
    class DxfLine : IDxfEntity
    {
        // имя объекта всегда должно быть постоянным
        const string name = EntitiesVariableCode.Line;

        #region Открытые только для чтения свойства класса

        public string Name { get { return name; } }         // Название в файле DXF
        public DxfColors Color { get; private set; }        // Цвет
        public DxfDot StartPoint { get; private set; }      // Начало линии
        public DxfEndPoint EndPoint { get; private set; }   // Конец линии
        public Rect Bound { get; set; }

        #endregion
        #region Конструктор, нельзя создать класс непосредственно

        public DxfLine() { }

        #endregion
        #region Открытые методы

        // Создание линии из пар кодов
        public static DxfLine Create(List<DxfCodePair> pairs, bool ignoreLineType)
        {
            DxfLine line = new DxfLine();

            for(int i=0;i<pairs.Count;i++)
            {
                switch (pairs[i].Code)
                {
                    case 62:
                        line.Color = DxfColor.Create(pairs[i], ignoreLineType);
                        break;
                    case 10:
                        if (pairs[i + 2].Code == 30)
                        {
                            line.StartPoint = DxfDot.Create(pairs.GetRange(i, 3));
                            i += 2;
                        }
                        else
                        {
                            line.StartPoint = DxfDot.Create(pairs.GetRange(i, 2));
                            i++;
                        }
                        break;
                    case 11:
                        if ((i+2) < pairs.Count && pairs[i + 2].Code == 31)
                        {
                            line.EndPoint = DxfEndPoint.Create(pairs.GetRange(i, 3));
                            i += 2;
                        }
                        else
                        {
                            line.EndPoint = DxfEndPoint.Create(pairs.GetRange(i, 2));
                            i++;
                        }
                        break;
                }
            }

            if (line.Color == 0 && ignoreLineType)
                line.Color = DxfColors.MainOutline;
            else if (line.Color == 0 || line.Color == DxfColors.NoColor)
                return null;

            CreateRect(line);

            return line;
        }

        private static void CreateRect(DxfLine line)
        {
            Rect r = new Rect();
            r.X = Math.Min(line.StartPoint.X, line.EndPoint.X);
            r.Y = Math.Min(line.StartPoint.Y, line.EndPoint.Y);
            r.Width = Math.Abs(line.StartPoint.X - line.EndPoint.X);
            r.Height = Math.Abs(line.StartPoint.Y - line.EndPoint.Y);
            line.Bound = r;
        }

        // Создание линии из вершин полилинии
        public static DxfLine Create(DxfVertex vertex1, DxfVertex vertex2, DxfColors color = DxfColors.MainOutline)
        {
            DxfLine line = new DxfLine
            {
                Color = color,
                StartPoint = vertex1.AsDot,
                EndPoint = DxfEndPoint.EndPointFromDot(vertex2.AsDot)
            };
            CreateRect(line);
            return line;
        }


        // Создание строки форматированной для вывода в файл DXF
        public override string ToString()
        {
            return string.Format("0\n{0}\n8\n0\n62\n{1}\n{2}{3}",
                Name, (int)Color, StartPoint.ToStringOf2d(), EndPoint.ToStringOf2d());
        }

        // Создание визуального представления линии
        public DrawingVisual CreateVisual(double scale)
        {
            DrawingVisual visual = new DrawingVisual();
            Point startPoint = new Point(StartPoint.X * scale, StartPoint.Y * scale);
            Point endPoint = new Point(EndPoint.X * scale, EndPoint.Y * scale); 
            using (DrawingContext dc = visual.RenderOpen())
            {
                Pen pen = new Pen(DxfHelper.GetBrush(Color), 2);
                dc.DrawLine(pen, startPoint, endPoint);
            }
            return visual;
        }

        // Смещение координат на заданный вектор
        public void OffSet(Vector vector)
        {
            StartPoint.OffSet(vector);
            EndPoint.OffSet(vector);
        }

        public string ToStringOfVerR13()
        {
            string str = ToString();
            str = str.Insert(str.IndexOf(StartPoint.ToStringOf2d()), "100\nAcDbLine\n");
            return str;
        }

        #endregion
    }
}
