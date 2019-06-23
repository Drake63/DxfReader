using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace DxfReader
{
    class DxfPolyline : IDxfEntity
    {
        // имя секции должно быть всегда постоянным
        const string name = EntitiesVariableCode.Polyline;
        private Rect bound;

        
        #region Открытые для чтения свойства класса

        public DxfColors Color { get; set; }                    // цвет линии
        public string Name { get { return name; } }             // Название объекта
        public ushort Flags { get; set; }                       // флаг, указывающий на тип полилинии
        public DxfVertices Vertexes { get; set; }       // Коллекция вершин
        public bool IsBulge { get; set; }                       // Показывает есть ли вершины с коэфициетном кривизны
        public Rect Bound
        {
            get => bound;
            set => bound = value;
        }
        internal bool IsClosed
        {
            get => DxfHelper.GetFlag(Flags, 1);
            set
            {
                if (value != DxfHelper.GetFlag(Flags, 1))
                    Flags ^= 1;
            }
        }

        #endregion
        #region Приватный конструктор, нельзя напрямую создавать объекты DxfPolyline

        private DxfPolyline()
        {
            Vertexes = new DxfVertices();
        }

        #endregion
        #region Методы 
        
        // Добавить вершину
        public void AddVert(DxfVertex vertex)
        {
            Vertexes.Add(vertex);
        }

        // Добавить массив вершин
        public void AddRangeVerts(DxfVertex[] vertexes)
        {
            Vertexes.AddRange(vertexes);
        }

        public void AddRangeVerts(List<DxfVertex> vertexes)
        {
            Vertexes.AddRange(vertexes);
        }

        // Создание объекта Polyline из пар кодов
        public static DxfPolyline Create(List<DxfCodePair> pairs, bool ignoreLineType)
        {
            // создать новый объект
            DxfPolyline polyline = new DxfPolyline();

            List<double> ordX = new List<double>();
            List<double> ordY = new List<double>();

            for (int i = 1; i < pairs.Count; i++)
            {
                switch (pairs[i].Code)
                {
                    case 62:
                        polyline.Color = DxfColor.Create(pairs[i], ignoreLineType);
                        if (polyline.Color == DxfColors.NoColor) return null;
                        break;
                    case 70:
                        polyline.Flags = pairs[i].AsUshort;
                        break;
                    case 10:
                        if (pairs[0].Value == EntitiesVariableCode.Polyline)
                            break;
                        else
                        {
                            double x = 0, y = 0, z = 0, b = 0;
                            do
                            {
                                switch (pairs[i].Code)
                                {
                                    case 10:
                                        x = pairs[i].AsDouble;
                                        break;
                                    case 20:
                                        y = pairs[i].AsDouble;
                                        break;
                                    case 30:
                                        z = pairs[i].AsDouble;
                                        break;
                                    case 42:
                                        b = pairs[i].AsDouble;
                                        break;
                                }
                                if (i == pairs.Count - 1 || pairs[i + 1].Code == 10)
                                    break;
                                i++;
                            } while (true);

                            polyline.Vertexes.Add(new DxfVertex(x, y, z, b));
                            if (b != 0) polyline.IsBulge = true;
                            ordX.Add(x);
                            ordY.Add(y);
                        }
                        break;
                    case 0:
                        if (pairs[0].Value == EntitiesVariableCode.LWPolyline || pairs[i].Value == EntitiesVariableCode.Seqend)
                            break;
                        else if (pairs[i].Value == EntitiesVariableCode.Vertex)
                        {
                            i++;
                            double x = 0, y = 0, z = 0, b = 0;
                            do
                            {
                                switch (pairs[i].Code)
                                {
                                    case 10:
                                        x = pairs[i].AsDouble;
                                        break;
                                    case 20:
                                        y = pairs[i].AsDouble;
                                        break;
                                    case 30:
                                        z = pairs[i].AsDouble;
                                        break;
                                    case 42:
                                        b = pairs[i].AsDouble;
                                        break;
                                }
                                if (pairs[i + 1].Code == 0)
                                    break;
                                i++;
                            } while (true);
                            polyline.Vertexes.Add(new DxfVertex(x, y, z, b));
                            if (b != 0) polyline.IsBulge = true;
                            ordX.Add(x); ordY.Add(y);
                        }
                        break;
                }
            }

            if (polyline.Color == 0 && ignoreLineType)
                polyline.Color = DxfColors.MainOutline;

            if (!polyline.IsClosed && (polyline.Vertexes[0].X == polyline.Vertexes[polyline.Vertexes.Count - 1].X 
                                         && polyline.Vertexes[0].Y == polyline.Vertexes[polyline.Vertexes.Count - 1].Y))
            {
                polyline.Vertexes.RemoveAt(polyline.Vertexes.Count - 1);
                polyline.IsClosed = true;
            }

            if (polyline.IsBulge)
                for (int i = polyline.Vertexes.FindIndex(v => v.Bulge != 0); i < polyline.Vertexes.Count; i++)
                {
                    if (polyline.Vertexes[i].Bulge != 0)
                    {
                        DxfArc arc = DxfArc.Create(polyline.Vertexes[i],
                            (i == polyline.Vertexes.Count - 1 && polyline.IsClosed) ? polyline.Vertexes[0] : polyline.Vertexes[i + 1]);
                        ordX.Add(arc.Bound.Left);
                        ordX.Add(arc.Bound.Right);
                        ordY.Add(arc.Bound.Bottom);
                        ordY.Add(arc.Bound.Top);
                    }
                }
            ordX.Sort();
            ordY.Sort();
            Rect rt = new Rect(ordX[0], ordY[0], ordX[ordX.Count - 1] - ordX[0], ordY[ordY.Count - 1] - ordY[0]);
            polyline.bound = rt;

            // вернуть созданную полилинию
            return polyline;
        }

        public static DxfPolyline Create()
        {
            return new DxfPolyline();
        }
        
        // Форматировать выходную строку для вывода в файл DXF
        public override string ToString()
        {
            string str = string.Format("0\n{0}\n8\n0\n62\n{1}\n10\n0\n20\n0\n70\n{2}\n", Name, (int)Color, Flags);
            str += Vertexes.ToStringOf2d();
            str += "0\n" + EntitiesVariableCode.Seqend + "\n";
            return str;
        }

        public string ToStringOfVerR13()
        {
            string str = string.Format("0\n{0}\n8\n0\n62\n{1}\n10\n0\n20\n0\n100\nAcDb2dPolyline\n70\n{2}\n", Name, (int)Color, Flags);
            str += Vertexes.ToStringOfVerR13();
            str += "0\n" + EntitiesVariableCode.Seqend + "\n";
            return str;
        }

        // Создать визуальный объект для вывода его на экран или на печать
        public DrawingVisual CreateVisual(double scale)
        {
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext dc = visual.RenderOpen())
            {
                // установить начало пути
                string str = string.Format("M{0} {1}", DxfHelper.DoubleToString(Vertexes[0].X * scale),
                    DxfHelper.DoubleToString(Vertexes[0].Y * scale));

                // цикл по всем вершинам полилинии
                for (int i = 0; i < Vertexes.Count; i++)
                {
                    // если найдена вершина с установленным коэфициентом кривизны
                    if (Vertexes[i].Bulge != 0)
                    {
                        // определить полный угол дуги если коэфициент равен 1 или -1, то это дуга равная 180 градусам
                        double angleOfArc = 0;
                        if (Math.Abs(Vertexes[i].Bulge) == 1)
                            angleOfArc = 180;
                        else
                            angleOfArc = Math.Atan(Math.Abs(Vertexes[i].Bulge)) * DxfHelper.RadToDeg * 4;

                        // найти конечную точку дуги
                        Point endPoint = new Point();
                        // если вершина последняя и полилиния является замкнутой,
                        // установить в качестве конечной точки первую вершину
                        if(IsClosed && i+1 == Vertexes.Count)
                        {
                            endPoint.X = Vertexes[0].X * scale;
                            endPoint.Y = Vertexes[0].Y * scale;
                        }
                        else
                        {
                            endPoint.X = Vertexes[i + 1].X * scale;
                            endPoint.Y = Vertexes[i + 1].Y * scale;
                        }

                        // найти хорду
                        Vector chord = new Vector(endPoint.X - Vertexes[i].X * scale, endPoint.Y - Vertexes[i].Y * scale);

                        // определить радиус дуги
                        double radius = angleOfArc == 180 ? chord.Length * .5 : (chord.Length / Math.Sin(.5 * angleOfArc * DxfHelper.DegToRad)) / 2;
                        
                        // установить флаг большая дуга или маленькая
                        byte isLarge = angleOfArc > 180 ? (byte)1 : (byte)0;
                        // установить флаг направления вращения дуги относительно часовой стрелки
                        byte isClockwise = Vertexes[i].Bulge > 0 ? (byte)1 : (byte)0;

                        // создать путь
                        str += string.Format("A{0} {0} {1} {2} {3} {4} {5}", DxfHelper.DoubleToString(radius), 0,
                            isLarge, isClockwise, DxfHelper.DoubleToString(endPoint.X), DxfHelper.DoubleToString(endPoint.Y));
                    }
                    else
                    {
                        // создать путь для прямой линии
                        if (i + 1 == Vertexes.Count) break;
                        str += "L" + DxfHelper.DoubleToString(Vertexes[i + 1].X * scale) + " "
                            + DxfHelper.DoubleToString(Vertexes[i + 1].Y * scale);
                    }
                }

                // записать в путь является ли полилиния замкнутой
                if (IsClosed) str += "Z";

                // создать геометрию из полученного пути
                dc.DrawGeometry(null, new Pen(DxfHelper.GetBrush(Color), 2.0), Geometry.Parse(str));
            }
            return visual;
        }

        // Вернуть несколько объектов IDxfEntity, разбив полилинию на объекты POLYLINE, LINE и ARC
        public List<IDxfEntity> GetEntitiesFromPolyline()
        {
            if (!IsBulge) throw new ArgumentException(Properties.Resource.ArcsNotFound);

            // создать новый список объектов
            List<IDxfEntity> entities = new List<IDxfEntity>();
            DxfPolyline pol = null;
            DxfArc arc = null;

            // найти вершину с коэфициентом искривленности
            int index = Vertexes.FindIndex(v => v.Bulge != 0);
            for (int i = 0; i < Vertexes.Count; i++)
            {
                // если больше ничего не найдено, но еще остались вершины, 
                // создать новую полилинию
                if (index == -1)
                {
                    if (i <= Vertexes.Count - 1)
                    {
                        if (Vertexes[i].Bulge != 0) i++;
                        if ((Vertexes.Count - 1) - i == 1 && !IsClosed)
                        {
                            DxfLine line = DxfLine.Create(Vertexes[i], Vertexes[i + 1], Color);
                            CreateBound(line);
                            entities.Add(line);
                            break;
                        }
                        else if ((Vertexes.Count - 1) - i == 0 && IsClosed)
                        {
                            DxfLine line = DxfLine.Create(Vertexes[i], new DxfVertex(new DxfDot(Vertexes[0].X, Vertexes[0].Y)), Color);
                            CreateBound(line);
                            entities.Add(line);
                            break;
                        }
                        else
                        {
                            pol = new DxfPolyline
                            {
                                Color = Color,
                                IsClosed = false,
                                IsBulge = false
                            };
                            pol.Vertexes.AddRange(Vertexes.GetRange(i, Vertexes.Count - i));
                            if (IsClosed)
                                pol.Vertexes.Add(new DxfVertex(new DxfDot(Vertexes[0].X, Vertexes[0].Y)));
                            CreateBound(pol);
                            entities.Add(pol);
                        }
                    }
                    break;
                }

                // если между найденой вершиной и текущей есть еще вершины, 
                // создать новую полилинию
                if (index > i)
                {
                    if (index - i == 1)
                    {
                        DxfLine line = DxfLine.Create(Vertexes[i], Vertexes[i + 1], Color);
                        CreateBound(line);
                        entities.Add(line);
                    }
                    else
                    {
                        pol = new DxfPolyline
                        {
                            Color = Color,
                            IsClosed = false,
                            IsBulge = false
                        };
                        pol.Vertexes.AddRange(Vertexes.GetRange(i, index - i + 1));
                        if (pol.Vertexes[pol.Vertexes.Count - 1].Bulge != 0)
                        {
                            DxfVertex vert = new DxfVertex(pol.Vertexes[pol.Vertexes.Count - 1].AsDot);
                            pol.Vertexes.RemoveAt(pol.Vertexes.Count - 1);
                            pol.Vertexes.Add(vert);
                        }
                        CreateBound(pol);
                        entities.Add(pol);
                    }
                }

                // создать новую дугу

                if (index + 1 == Vertexes.Count && IsClosed)
                    arc = DxfArc.Create(Vertexes[index], Vertexes[0], Color);
                else
                    arc = DxfArc.Create(Vertexes[index], Vertexes[index + 1], Color);

                entities.Add(arc);

                i = index;

                // продолжить поиск 
                index = Vertexes.FindIndex(index + 1, v => v.Bulge != 0);
            }
            return entities;
        }
        
        // Смещение координат на заданный вектор
        public void OffSet(Vector vector)
        {
            Vertexes.OffSet(vector);
        }

        // Вычисление описывающего прямоугольника для объекта
        internal void CreateBound(IDxfEntity entity)
        {
            Rect rt = new Rect();

            if(entity is DxfLine)
            {
                rt.X = Math.Min((entity as DxfLine).StartPoint.X, (entity as DxfLine).EndPoint.X);
                rt.Y = Math.Min((entity as DxfLine).StartPoint.Y, (entity as DxfLine).EndPoint.Y);
                rt.Width = Math.Abs((entity as DxfLine).StartPoint.X - (entity as DxfLine).EndPoint.X);
                rt.Height = Math.Abs((entity as DxfLine).StartPoint.Y - (entity as DxfLine).EndPoint.Y);
                (entity as DxfLine).Bound = rt;
            }
            else if(entity is DxfPolyline)
            {
                double[] ordX = new double[(entity as DxfPolyline).Vertexes.Count];
                double[] ordY = new double[(entity as DxfPolyline).Vertexes.Count];

                for(int i=0;i<(entity as DxfPolyline).Vertexes.Count;i++)
                {
                    ordX[i] = (entity as DxfPolyline).Vertexes[i].X;
                    ordY[i] = (entity as DxfPolyline).Vertexes[i].Y;
                }

                Array.Sort(ordX);
                Array.Sort(ordY);
                rt.X = ordX[0];
                rt.Y = ordY[0];
                rt.Width = ordX[ordX.Length - 1] - ordX[0];
                rt.Height = ordY[ordY.Length - 1] - ordY[0];
                (entity as DxfPolyline).Bound = rt;
            }
        }

        #endregion
    }
}
