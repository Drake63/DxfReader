using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace DxfReader
{
    // Эллипс
    class DxfEllipse : IDxfEntity
    {
        // название секции всегда должно быть постоянным
        const string name = EntitiesVariableCode.Ellipse;

        double majorRadius = 0;
        double minorRadius = 0;
        double rotateAngle = 0;
        Vector center;

        #region Открытые только для чтения свойства класса

        public string Name { get { return name; } }         // название в файле DXF
        public DxfColors Color { get; set; }                // цвет линии
        public DxfDot Center { get; private set; }          // координаты центра
        public DxfMajorAxis MajorAxis { get; private set; } // вектор радиуса по оси X
        public double Ratio { get; private set; }           // отношение радиусов по осям Y и X
        public double StartParam { get; private set; }      // начальный угол, всегда 0, если эллипс полный
        public double EndParam { get; private set; }        // конечный угол, всегда 2пи, если эллипс полный
        public bool IsFull
        {
            get
            {
                return StartParam == 0.0 && Math.Round(EndParam,6) == Math.Round(DxfHelper.TwoPi, 6) ? true : false;
            }
        }
        public Rect Bound { get; private set; }

        #endregion
        #region Коструктор, нельзя создать класс непосредственно, используя конструктор

        private DxfEllipse() { }

        #endregion
        #region Открытые методы

        // Метод создания эллипса из пар кодов
        public static DxfEllipse Create(List<DxfCodePair> pairs, bool ignoreLineType)
        {
            DxfEllipse ellipse = new DxfEllipse();
             
            for(int i=0;i<pairs.Count;i++)
                switch (pairs[i].Code)
                {
                    case 62:
                        ellipse.Color = DxfColor.Create(pairs[i], ignoreLineType);
                        break;
                    case 10:
                        if (pairs[i + 2].Code==30)
                        {
                            ellipse.Center = DxfDot.Create(pairs.GetRange(i, 3));
                            i += 2;
                        }
                        else
                        {
                            ellipse.Center = DxfDot.Create(pairs.GetRange(i, 2));
                            i++;
                        }
                        break;
                    case 11:
                        if(pairs[i+2].Code==31)
                        {
                            ellipse.MajorAxis = DxfMajorAxis.Create(pairs.GetRange(i, 3));
                            i += 2;
                        }
                        else
                        {
                            ellipse.MajorAxis = DxfMajorAxis.Create(pairs.GetRange(i, 2));
                            i++;
                        }
                        break;
                    case 40:
                        ellipse.Ratio = pairs[i].AsDouble;
                        break;
                    case 41:
                        ellipse.StartParam = pairs[i].AsDouble;
                        break;
                    case 42:
                        ellipse.EndParam = pairs[i].AsDouble;
                        break;
                }

            if (ellipse.Color == 0 && ignoreLineType)
                ellipse.Color = DxfColors.MainOutline;
            else if (ellipse.Color == 0 || ellipse.Color == DxfColors.NoColor)
                return null;

            Vector majorVector = new Vector(ellipse.MajorAxis.X, ellipse.MajorAxis.Y);
            ellipse.majorRadius = majorVector.Length;
            ellipse.minorRadius = ellipse.majorRadius * ellipse.Ratio;
            ellipse.rotateAngle = Vector.AngleBetween(new Vector(1, 0), majorVector);
            ellipse.center = new Vector(ellipse.Center.X, ellipse.Center.Y);

            int precision = (int)(Math.Abs(ellipse.EndParam - ellipse.StartParam) * DxfHelper.RadToDeg);
            double step = Math.Abs(ellipse.EndParam - ellipse.StartParam) / (precision - 1);
            double[] ordX = new double[precision];
            double[] ordY = new double[precision];

            for(int i=0;i<precision;i++)
            {
                double angle = ellipse.StartParam + step * i;

                Vector vector = ellipse.GetVector(angle, 1, ellipse.rotateAngle);

                ordX[i] = vector.X;
                ordY[i] = vector.Y;
            }
            Array.Sort(ordX);
            Array.Sort(ordY);
            Rect rt = new Rect(ordX[0], ordY[0], ordX[ordX.Length - 1] - ordX[0], ordY[ordY.Length - 1] - ordY[0]);
            ellipse.Bound = rt;
            return ellipse;
        }

        // Преобразовать эллипс в POLYLINE
        public DxfPolyline ToPolyline(int precision)
        {            
            DxfPolyline polyline = DxfPolyline.Create();                // Создать новую полилинию
            polyline.Color = Color;                                     // присвоить цвет линии как у эллипса
            polyline.IsBulge = false;                                   // искревленности нет
            Vector center = new Vector(Center.X, Center.Y);      // определить угол поворота эллипса относительно оси 0X
            double[] ordX = new double[precision];
            double[] ordY = new double[precision];

            polyline.IsClosed = IsFull ? true : false;                             // определить тип полилинии
            // список вершин полилинии
            List<DxfVertex> vertexes = new List<DxfVertex>();

            double step = (EndParam - StartParam) / (precision - 1);    // шаг расчета вершин зависит от заданной точности

            for(int i = 0; i< precision; i++)
            {
                double angle = StartParam + step * i;   // угол расчета вершины

                // создать новый вектор для этого угла
                Vector vector = GetVector(angle, 1, rotateAngle);

                // создать новую вершину 
                DxfVertex vertex = new DxfVertex(vector);
                ordX[i] = vector.X;
                ordY[i] = vector.Y;
                // добавить вершину в коллекцию
                vertexes.Add(vertex);
            }            

            // добавить созданный массив вершин в коллекцию полилинии
            polyline.AddRangeVerts(vertexes);

            Array.Sort(ordX);
            Array.Sort(ordY);

            Rect rt = new Rect(ordX[0], ordY[0], ordX[ordX.Length - 1] - ordX[0], ordY[ordY.Length - 1] - ordY[0]);
            polyline.Bound = rt;

            // вернуть созданную полилинию
            return polyline;
        }

        // Преоборазовать эллипс в LWPOLYLINE
        public LwPolyline ToLwPolyline(int precision)
        {
            // создать новую LwPolyline из полилинии
            return LwPolyline.Create(ToPolyline(precision));
        }

        // Метод создания визуального представления
        public DrawingVisual CreateVisual(double scale)
        {
            DrawingVisual visual = new DrawingVisual();

            if (IsFull)    // эллипс полный
            {
                Point center = new Point(Center.X * scale, Center.Y * scale);
                using (DrawingContext dc = visual.RenderOpen())
                {
                    // Создать визуальное представление эллипса
                    dc.DrawEllipse(null, new Pen(DxfHelper.GetBrush(Color), 2.0),
                        center, majorRadius * scale, minorRadius * scale);
                }

                // задать трансформацию объекту DrawingVisual
                visual.Transform = new RotateTransform();
                // определить центр поворота эллипса
                (visual.Transform as RotateTransform).CenterX = center.X;
                (visual.Transform as RotateTransform).CenterY = center.Y;
                // задать угол поворота
                (visual.Transform as RotateTransform).Angle = rotateAngle; 
            }
            else        // часть эллипса
            {
                Vector startPoint = GetVector(StartParam, scale, rotateAngle);      // найти начальную точку эллипса
                Vector endPoint = GetVector(EndParam, scale, rotateAngle);          // найти конечную точку
                byte isLarge = EndParam - StartParam > DxfHelper.Pi ? (byte)1 : (byte)0;    // определить большая дуга или нет

                // создать строку пути для эллиптической дуги
                string str = string.Format("M{0} {1}A{2} {3} {4} {5} {6} {7} {8}", DxfHelper.DoubleToString(startPoint.X),
                    DxfHelper.DoubleToString(startPoint.Y), DxfHelper.DoubleToString(majorRadius*scale), DxfHelper.DoubleToString(minorRadius* scale),
                    DxfHelper.DoubleToString(rotateAngle), isLarge, 1, DxfHelper.DoubleToString(endPoint.X), DxfHelper.DoubleToString(endPoint.Y));
                using (DrawingContext dc = visual.RenderOpen())
                {
                    // создать визуальное представление для эллиптической дуги
                    dc.DrawGeometry(null, new Pen(DxfHelper.GetBrush(Color), 2), Geometry.Parse(str));
                }
            }

            return visual;
        }

        // Создание форматированной строки для вывода в файл DXF
        public override string ToString()
        {
            return string.Format("0\n{0}\n8\n0\n62\n{1}\n{2}{3}40\n{4}\n41\n{5}\n42\n{6}\n",
                Name, (int)Color, Center.AsPoint.ToString(), MajorAxis.ToStringOf2d(),
                DxfHelper.DoubleToString(Ratio), DxfHelper.DoubleToString(StartParam), DxfHelper.DoubleToString(EndParam));
        }

        public string ToStringOfVerR13()
        {
            string str = ToString();
            str = str.Insert(str.IndexOf(Center.AsPoint.ToString()), "100\nAcDbEllipse\n");
            return str;
        }

        // Выполнение смещения коодринат на заданный вектор
        public void OffSet(Vector vector)
        {
            Center.OffSet(vector);
            center = center - vector;
        }

        #endregion

        // Вычисление вектора радиуса эллипса для заданного угла 
        Vector PolarCoordRelToCenter(double angle)
        {
            Vector major = new Vector(MajorAxis.X, MajorAxis.Y);
            double majorLength = major.Length;
            double minorLength = majorLength * Ratio;
            double angleToRad = angle * DxfHelper.DegToRad;
            double majorSin = majorLength * Math.Sin(angleToRad);
            double minorCos = minorLength * Math.Cos(angleToRad);
            double radiusLength = (majorLength * minorLength) / Math.Sqrt(majorSin * majorSin + minorCos * minorCos);

            return new Vector(radiusLength * Math.Cos(angleToRad), radiusLength * Math.Sin(angleToRad));
        }

        // Определение координат точки, параметр угол в радианах
        private Vector GetVector(double param, double scale, double rotate)
        {
            Vector vector = DxfHelper.RotateVector(new Vector(scale * majorRadius * Math.Cos(param), scale * minorRadius * Math.Sin(param)), rotate);

            return scale * center + vector;
        }
    }
}
