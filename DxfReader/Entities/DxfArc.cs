using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System;

namespace DxfReader
{
    // Дуга DXF
    class DxfArc : IDxfEntity
    {
        // имя в файле DXF всегда должно быть постоянным
        const string name = EntitiesVariableCode.Arc;

        #region Открытые только для чтения свойства 

        public DxfColors Color { get; private set; }        // цвет линии
        public string Name { get { return name; } }         // название в файле
        public DxfDot Center { get; private set; }          // координаты центра
        public double Radius { get; private set; }          // радиус
        public double StartAngle { get; private set; }      // начальный угол
        public double EndAngel { get; private set; }        // конечный угол
        public Rect Bound { get; private set; }

        #endregion
        #region Конструктор, нельзя создавать класс непосредственно через него

        private DxfArc() { }

        #endregion
        #region Открытые методы класса

        // Создание класса из пар кодов
        public static DxfArc Create(List<DxfCodePair> pairs, bool ignoreLineType)
        {
            DxfArc arc = new DxfArc();

            for(int i=0;i<pairs.Count;i++)
            {
                switch (pairs[i].Code)
                {
                    case 62:
                        arc.Color = DxfColor.Create(pairs[i], ignoreLineType);
                        break;
                    case 10:
                        if (pairs[i + 2].Code == 30)
                        {
                            arc.Center = DxfDot.Create(pairs.GetRange(i, 3));
                            i += 2;
                        }
                        else
                        {
                            arc.Center = DxfDot.Create(pairs.GetRange(i, 2));
                            i++;
                        }
                        break;
                    case 40:
                        arc.Radius = pairs[i].AsDouble;
                        break;
                    case 50:
                        arc.StartAngle = pairs[i].AsDouble;
                        break;
                    case 51:
                        arc.EndAngel = pairs[i].AsDouble;
                        break;
                }
            }

            if (arc.Color == 0 && ignoreLineType)
                arc.Color = DxfColors.MainOutline;
            else if (arc.Color == 0 || arc.Color == DxfColors.NoColor)
                return null;

            CreateBound(arc);

            return arc;
        }

        // Формирование строки для вывода в файл DXF
        public override string ToString()
        {
            return string.Format("0\n{0}\n8\n0\n62\n{1}\n{2}40\n{3}\n50\n{4}\n51\n{5}\n",
                Name, (int)Color, Center.AsPoint.ToString(), DxfHelper.DoubleToString(Radius),
                DxfHelper.DoubleToString(StartAngle), DxfHelper.DoubleToString(EndAngel));
        }

        public string ToStringOfVerR13()
        {
            string str = ToString();
            str = str.Insert(str.IndexOf(Center.AsPoint.ToString()), "100\nAcDbCircle\n");
            str = str.Insert(str.IndexOf("50\n" + DxfHelper.DoubleToString(StartAngle)), "100\nAcDbArc\n");
            return str;
        }

        // Создать объект Arc из двух вершин, в первой из который устанолен коэффициет искривления
        public static DxfArc Create(DxfVertex vertex1, DxfVertex vertex2, DxfColors color = DxfColors.MainOutline)
        {
            if (vertex1.Bulge == 0) throw new ArgumentNullException(Properties.Resource.BulgeNotFound);

            DxfArc arc = new DxfArc();

            arc.Color = color;      // цвет линии

            // Наити полный угол дуги
            double angleOfArc = 0;
            if (Math.Abs(vertex1.Bulge) == 1)
                angleOfArc = 180;
            else
                angleOfArc = Math.Abs(Math.Atan(vertex1.Bulge) * DxfHelper.RadToDeg) * 4;

            // Найти хорду и ее длинну, определить начало и конец дуги,
            // т.к. в файлах DXF направление дуги всегда по часовой стрелке,
            // если коэффициент искривления вершины имеет отрицательное значение,  
            // то поменять точки местами
            Vector startPoint = vertex1.Bulge > 0 ? new Vector(vertex1.X, vertex1.Y) : new Vector(vertex2.X, vertex2.Y);    // точка начала дуги
            Vector endPoint = vertex1.Bulge > 0 ? new Vector(vertex2.X, vertex2.Y) : new Vector(vertex1.X, vertex1.Y);      // точка конца дуги
            Vector chord = endPoint - startPoint;                                                                                                   // хорда

            // Радиус дуги
            // если угол дуги равен 180 градусов, то радиус равен половине длинны хорды
            if (angleOfArc == 180)
                arc.Radius = chord.Length / 2;
            else
                arc.Radius = Math.Abs((chord.Length / Math.Sin(angleOfArc / 2 * DxfHelper.DegToRad)) / 2);

            // найти центр хорды
            Vector centerChord = startPoint + chord / 2;
            if (angleOfArc == 180)
                arc.Center = new DxfDot(centerChord.X, centerChord.Y);
            else
            {
                // найти расстояние от центра дуги до середины хорды
                double length = Math.Sqrt(arc.Radius * arc.Radius - Math.Pow(chord.Length * .5, 2));
                // единичный вектор, перпендикулярный хорде
                Vector perp = angleOfArc > 180 ? DxfHelper.RotateVector(chord, 270) : DxfHelper.RotateVector(chord, 90);
                perp.Normalize();
                // умножить перпендикуляр на расстояние до центра дуги
                Vector size = perp * length;
                // найти центр дуги
                Vector centerArc = centerChord + size;
                arc.Center = new DxfDot(centerArc.X, centerArc.Y);
            }

            // найти начальный и конечный углы дуги
            arc.StartAngle = GetAngle(arc.Center, startPoint, arc.Radius);
            arc.EndAngel = arc.StartAngle + angleOfArc;
            arc.EndAngel = arc.EndAngel >= 360 ? arc.EndAngel - 360 : arc.EndAngel;

            CreateBound(arc);

            return arc;
        }

        // Создание визуального представления дуги
        public DrawingVisual CreateVisual(double scale)
        {
            Vector center = new Vector(Center.X * scale, Center.Y * scale);
            double radius = Radius * scale;
            // найти начальную точку дуги
            Point startPoint = new Point(radius * Math.Cos(StartAngle * DxfHelper.DegToRad) + center.X,
                radius * Math.Sin(StartAngle * DxfHelper.DegToRad) + center.Y);
            // найти конечную точку дуги
            Point endPoint = new Point(radius * Math.Cos(EndAngel * DxfHelper.DegToRad) + center.X,
                radius * Math.Sin(EndAngel * DxfHelper.DegToRad) + center.Y);

            // определить угол дуги
            double angle = StartAngle < EndAngel ? EndAngel - StartAngle : 360 - (StartAngle - EndAngel);
            // установить флаг, если угол больше 180 градусов - 1, в обратном случае - 0
            byte isLarge = angle > 180 ? (byte)1 : (byte)0;

            // сформировать строку пути
            string str = string.Format("M{0} {1}A{2} {2} {3} {4} {5} {6} {7}", DxfHelper.DoubleToString(startPoint.X),
                DxfHelper.DoubleToString(startPoint.Y), DxfHelper.DoubleToString(radius), 0,
                isLarge, 1, DxfHelper.DoubleToString(endPoint.X), DxfHelper.DoubleToString(endPoint.Y));

            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext dc = visual.RenderOpen())
            {
                // создать геометрию
                dc.DrawGeometry(null, new Pen(DxfHelper.GetBrush(Color), 2), Geometry.Parse(str));
            }
            // вернуть объект DrawingVisual с созданной геометрией
            return visual;
        }

        // Смещение координат на заданный вектор
        public void OffSet(Vector vector)
        {
            Center.OffSet(vector);
        }

        #endregion
        #region Вспомогательные методы

        // Определение угла поворота точки на окружности относительно центра
        private static double GetAngle(DxfDot center, Vector dot, double radius)
        {
            double angle = Math.Asin(Math.Abs(center.Y - dot.Y) / radius) * DxfHelper.RadToDeg;
            if(double.IsNaN(angle))
                angle = Math.Acos(Math.Abs(center.X - dot.X) / radius) * DxfHelper.RadToDeg;

            if (center.X <= dot.X && center.Y <= dot.Y)
                return angle;
            else if (center.X >= dot.X && center.Y <= dot.Y)
                return 180 - angle;
            else if (center.X >= dot.X && center.Y >= dot.Y)
                return 180 + angle;
            else if (center.X <= dot.X && center.Y >= dot.Y)
            {
                angle = 360 - angle;
                angle = angle >= 360 ? angle - 360 : angle;
            }
            return angle;
        } 

        private static void CreateBound(DxfArc arc)
        {
            double angle = (arc.StartAngle < arc.EndAngel ? arc.EndAngel - arc.StartAngle : 360 - (arc.StartAngle - arc.EndAngel));
            int precision = (int)angle;
            if (precision < 3)
                precision = 3;

            double[] ordX = new double[precision];
            double[] ordY = new double[precision];
                double step = Math.Abs(angle / (precision - 1));
                for (int i = 0; i < precision; i++)
                {
                    ordX[i] = arc.Radius * Math.Cos((arc.StartAngle + step * i) * DxfHelper.DegToRad) + arc.Center.X;
                    ordY[i] = arc.Radius * Math.Sin((arc.StartAngle + step * i) * DxfHelper.DegToRad) + arc.Center.Y;
                }
            Array.Sort(ordX);
            Array.Sort(ordY);
            
            Rect rct = new Rect(ordX[0], ordY[0], ordX[ordX.Length - 1] - ordX[0], ordY[ordY.Length - 1] - ordY[0]);
            arc.Bound = rct;
        }

        #endregion
    }
}
