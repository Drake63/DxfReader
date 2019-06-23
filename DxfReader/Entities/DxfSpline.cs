using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace DxfReader
{
    // Сплайн
    class DxfSpline : IDxfEntity
    {
        const string name = EntitiesVariableCode.Spline;
        const double KNOT_TOLERANCE = 1E-07;
        const double CONTROL_TOLERANCE = 1E-07;
        const double FIT_TOLERANCE = 1E-10;

        DxfPolyline polyline;
        int precision = 100;

        #region Открытые только для чтения свойства
        public string Name => name;                                         // имя сущности в файле
        public DxfColors Color { get; private set; }                        // тип линии
        public Rect Bound { get; private set; }                             // ограничивающий прямоугольник
        public DxfNormalVector NormalVector { get; private set; }           // нормальный вектор
        public short NumberOfKnots { get; private set; }                    // количество узлов
        public short NumberOfControlPoints { get; private set; }            // количество контрольных точек
        public short NumberOfFitPoints { get; private set; }                // количество управляющих точек, если они есть
        public short Degree { get; private set; }                           // степень сплайна
        public ushort Flags { get; private set; }                           // Флаги сплайна
        public bool IsClosed => DxfHelper.GetFlag(Flags, 1);                // замкнутый
        public bool IsPeriodic => DxfHelper.GetFlag(Flags, 2);              // периодический
        public bool IsRational => DxfHelper.GetFlag(Flags, 4);              // рациональный
        public bool IsPlanar => DxfHelper.GetFlag(Flags, 8);                // плоский
        public bool IsLinear => DxfHelper.GetFlag(Flags, 16);               // линейный
        public double KnotTolerance { get; private set; }                   // допуск для узлов
        public double ControlTolerance { get; private set; }                // допуск для котрольных точек
        public double FitTolerance { get; private set; }                    // допуск для управляющих точек
        public DxfStarTangent StartTangent { get; private set; }            // начальная точка, если есть
        public DxfEndTangent EndTangent { get; private set; }               // конечная точка, если есть
        public DxfKnots Knots { get; private set; }                         // значения узлов
        public DxfControlPoints ControlPoints { get; private set; }         // координаты контрольных точек
        public DxfFitPoints FitPoints { get; private set; }                 // координаты управляющих точек
        public int Precision                                                // точность с какой требуется строить полилинию из сплайна
        {
            get => precision;
            set => precision = value;
        }

        #endregion

        #region Приватный конструктор, создавать напрямую класс нельзя

        private DxfSpline()
        {
            Knots = new DxfKnots();
            ControlPoints = new DxfControlPoints();
            FitPoints = new DxfFitPoints();
            KnotTolerance = KNOT_TOLERANCE;
            ControlTolerance = CONTROL_TOLERANCE;
            FitTolerance = FIT_TOLERANCE;
        } 

        #endregion
        
        public DrawingVisual CreateVisual(double scale)
        {
             return polyline.CreateVisual(scale);
        }


        #region Методы класса

        // Создание объекта Spline из файла dxf
        public static DxfSpline Create(List<DxfCodePair> pairs, bool ignoreLineType)
        {
            // создать новый объект
            DxfSpline spline = new DxfSpline();

            for (int i=0;i<pairs.Count;i++)
            {
                switch (pairs[i].Code)
                {
                    case 0: break;
                    case 62:
                        spline.Color = DxfColor.Create(pairs[i], ignoreLineType);
                        if (spline.Color == DxfColors.NoColor) return null;
                        break;
                    case 70:
                        spline.Flags = pairs[i].AsUshort;
                        break;
                    case 71:
                        spline.Degree = pairs[i].AsByte;
                        break;
                    case 72:
                        spline.NumberOfKnots = pairs[i].AsShort;
                        break;
                    case 73:
                        spline.NumberOfControlPoints = pairs[i].AsShort;
                        break;
                    case 74:
                        spline.NumberOfFitPoints = pairs[i].AsShort;
                        break;
                    case 42:
                        spline.KnotTolerance = pairs[i].AsDouble;
                        break;
                    case 43:
                        spline.ControlTolerance = pairs[i].AsDouble;
                        break;
                    case 44:
                        spline.FitTolerance = pairs[i].AsDouble;
                        break;
                    case 11:
                        if ((i + 3) < pairs.Count && pairs[i + 3].Code == 31)
                        {
                            spline.FitPoints.Add(DxfFitPoint.Create(pairs.GetRange(i, 3)));
                            i += 2;
                        }
                        else
                        {
                            spline.FitPoints.Add(DxfFitPoint.Create(pairs.GetRange(i, 2)));
                            i++;
                        }
                        break;
                    case 12:
                        if ((i + 3) < pairs.Count && pairs[i + 3].Code == 32)
                        {
                            spline.StartTangent = DxfStarTangent.Create(pairs.GetRange(i, 3));
                            i += 2;
                        }
                        else
                        {
                            spline.StartTangent = DxfStarTangent.Create(pairs.GetRange(i, 2));
                            i++;
                        }
                        break;
                    case 13:
                        if ((i + 3) < pairs.Count && pairs[i + 3].Code == 33)
                        {
                            spline.EndTangent = DxfEndTangent.Create(pairs.GetRange(i, 3));
                            i += 2;
                        }
                        else
                        {
                            spline.EndTangent = DxfEndTangent.Create(pairs.GetRange(i, 2));
                            i++;
                        }
                        break;
                    case 40:
                        spline.Knots.Add(new DxfKnot(pairs[i].AsDouble));
                        if(pairs[i+1].Code==10 || pairs[i+1].Code==41)
                        {
                            i++;
                            double x = 0, y = 0, z = 0, w = 1.0;

                            while (pairs[i].Code != 0 && pairs[i].Code != 11 && i < pairs.Count -1)
                            {
                                if (pairs[i].Code == 10)
                                {
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
                                            case 41:
                                                w = pairs[i].AsDouble;
                                                break;
                                        }
                                        i++;
                                        if (i >= pairs.Count)
                                        {
                                            i = pairs.Count - 1;
                                            break;
                                        }
                                    } while (pairs[i].Code != 10);
                                    spline.ControlPoints.Add(new DxfControlPoint(x, y, z, w));
                                }
                                else if (pairs[i].Code == 41)
                                {
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
                                            case 41:
                                                w = pairs[i].AsDouble;
                                                break;
                                        }
                                        i++;
                                        if (i >= pairs.Count)
                                        {
                                            i = pairs.Count - 1;
                                            break;
                                        }
                                    } while (pairs[i].Code != 41);
                                    spline.ControlPoints.Add(new DxfControlPoint(x, y, z, w));                                    
                                }
                            }
                        }
                        break;
                    case 210:
                        if ((i + 3) < pairs.Count && pairs[i + 3].Code == 230)
                        {
                            spline.NormalVector = DxfNormalVector.Create(pairs.GetRange(i, 3));
                            i += 2;
                        }
                        else
                        {
                            spline.NormalVector = DxfNormalVector.Create(pairs.GetRange(i, 2));
                            i++;
                        }
                        break;
                }
            }

            // Если тип линии не известен и установлен флаг инорировать типы линий, 
            // установить основную линию, иначе вернуть null вместо объекта Spline
            if (spline.Color == 0 && ignoreLineType)
                spline.Color = DxfColors.MainOutline;
            else if (spline.Color == 0 && !ignoreLineType)
                return null;

            // проверить данные количестве узлов, контрольных и управляющих точек  и скоректировать их
            if (spline.ControlPoints.Count != spline.NumberOfControlPoints)
                spline.NumberOfControlPoints = (short)spline.ControlPoints.Count;
            if (spline.Knots.Count != spline.NumberOfKnots)
                spline.NumberOfKnots = (short)spline.Knots.Count;
            if (spline.FitPoints.Count != spline.NumberOfFitPoints)
                spline.NumberOfFitPoints = (short)spline.FitPoints.Count;

            spline.polyline = DxfPolyline.Create();
            spline.polyline.Color = spline.Color;
            spline.polyline.IsClosed = spline.IsClosed;
            spline.polyline.Vertexes = spline.PolylineVertices(spline.precision);

            // вычислить границы сплайна
            spline.polyline.CreateBound(spline.polyline);
            spline.Bound = spline.polyline.Bound;

            // вернуть созданый объект
            return spline;
        }

        // Сдвинуть сплайн на заданный вектор
        public void OffSet(Vector vector)
        {
            if (StartTangent != null)
                StartTangent.OffSet(vector);
            if (EndTangent != null)
                EndTangent.OffSet(vector);

            ControlPoints.OffSet(vector);

            if (FitPoints.Count > 0)
                FitPoints.OffSet(vector);

            if (polyline != null)
                polyline.OffSet(vector);
        }

        // Создание строки для записи ее в файл DXF
        public override string ToString()
        {
            string result = string.Format("0\n{0}\n8\n0\n62\n{1}\n", Name, (int)Color);
            if (IsPlanar)
                result += NormalVector.ToStringOf2d();
            result += string.Format("70\n{0}\n71\n{1}\n", Flags, Degree);
            if (KnotTolerance != KNOT_TOLERANCE)
                result += "42\n" + DxfHelper.DoubleToString(KnotTolerance) + "\n";
            if (ControlTolerance != CONTROL_TOLERANCE)
                result += "43\n" + DxfHelper.DoubleToString(ControlTolerance) + "\n";
            if (FitTolerance != FIT_TOLERANCE)
                result += "44\n" + DxfHelper.DoubleToString(FitTolerance) + "\n";

            if (StartTangent != null)
                result += StartTangent.ToStringOf2d();
            if (EndTangent != null)
                result += EndTangent.ToStringOf2d();

            result += Knots.ToString();

            result += ControlPoints.ToStringOf2d();

            if (FitPoints.Count > 0)
                FitPoints.ToString();

            return result;
        }

        // Создание строки для записи ее в файл DXF R13
        public string ToStringOfVerR13()
        {
            string str = ToString();
            string indStr = "62\n" + (int)Color + "\n";
            int index = str.IndexOf(indStr) + indStr.Length;
            str = str.Insert(index, "100\nAcDbSpline\n");
            return str;
        }

        #endregion

        #region Расчет вершин полилинии из сплайна

        private DxfVertices PolylineVertices(int precision)
        {
            if (ControlPoints.Count == 0)
                throw new NotSupportedException(Properties.Resource.NeedCtrlPoints);

            double firsKnot, lastKnot;
            DxfVertices vertices = new DxfVertices();

            if (IsClosed)
            {
                precision--;
                firsKnot = Knots[0].Knot;
                lastKnot = Knots[Knots.Count - 1].Knot;
            }
            else if (IsPeriodic)
            {
                firsKnot = Knots[Degree].Knot;
                lastKnot = Knots[(Knots.Count - Degree) - 1].Knot;
            }
            else
            {
                firsKnot = Knots[0].Knot;
                lastKnot = Knots[Knots.Count - 1].Knot;
            }

            double step = (lastKnot - firsKnot) / (double)precision;
            for (int i = 0; i < precision; i++)
            {
                double knot = firsKnot + (step * i);
                vertices.Add(GetVertex(knot));
            }

            if (!IsClosed)
                vertices.Add(ControlPoints[ControlPoints.Count - 1].AsVertex);

            return vertices;
        }

        private DxfVertex GetVertex(double knot)
        {
            DxfVertex zero = DxfVertex.Zero;
            double step = 0.0;

            for (int i = 0; i < ControlPoints.Count; i++)
            {
                double offset = GetOffset(i, Degree, knot);
                step += offset * ControlPoints[i].Weight;
                zero += ((ControlPoints[i].Weight * offset) * ControlPoints[i]).AsVertex;
            }
            if (Math.Abs(step) < double.Epsilon)
                return DxfVertex.Zero;
            return (1.0 / step) * zero;
        }

        private double GetOffset(int i, int degree, double knot)
        {
            if (degree <= 0)
            {
                if (Knots[i] <= knot && knot < Knots[i + 1])
                    return 1.0;
                return 0.0;
            }

            double num = 0.0;
            if (Math.Abs(Knots[i + degree] - Knots[i]) >= double.Epsilon)
                num = (knot - Knots[i]) / (Knots[i + degree] - Knots[i]);

            double num2 = 0.0;
            if (Math.Abs(Knots[(i + degree) + 1] - Knots[i + 1]) >= double.Epsilon)
                num2 = (Knots[(i + degree) + 1] - knot) / (Knots[(i + degree) + 1] - Knots[i + 1]);

            return ((num * GetOffset(i, degree - 1, knot)) + (num2 * GetOffset(i + 1, degree - 1, knot)));
        } 

        #endregion

        public DxfPolyline SplineToPolyline(int precision)
        {
            if (precision == 100)
                return this.polyline;

            DxfPolyline polyline = DxfPolyline.Create();
            polyline.Color = Color;
            polyline.IsClosed = IsClosed;
            polyline.Vertexes = PolylineVertices(precision);
            polyline.CreateBound(polyline);

            return polyline;
        }
    }
}
