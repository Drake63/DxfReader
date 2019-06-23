using System.Collections.Generic;
using System;
using System.Windows;

namespace DxfReader
{
    // Секция ENTITITES
    class DxfEntities : IDxfOffset
    {
        ushort flags;
        internal bool CreateLwPolylint { get => DxfHelper.GetFlag(flags, (int)DxfFlags.CreateLwPolyline); }
        internal bool CreateArcFromVertex { get => DxfHelper.GetFlag(flags, (int)DxfFlags.CreateArc); }
        internal bool IgnoreLineType { get => DxfHelper.GetFlag(flags, (int)DxfFlags.IgnoreOtherLineType); }
        internal bool CreateR13 { get => DxfHelper.GetFlag(flags, (int)DxfFlags.CreateR13); }

        #region Открытые только для чтения свойства

        public List<IDxfEntity> Entities { get; private set; }
        public bool IsInserted { get; set; }
        public bool ContainsSplines { get; set; }

        #endregion
        #region Конструктор

        public DxfEntities()
        {
            Entities = new List<IDxfEntity>();
        }

        #endregion
        #region Открытые методы

        // Добавление графического объекта во внутренний список
        public void Add(IDxfEntity entity)
        {
            Entities.Add(entity);
        }

        // Создание секции из пар кодов
        public void Create(List<DxfCodePair> pairs, ushort flags)
        {
            this.flags = flags;

            // найти первую пару с кодом 0
            Entities.AddRange(CreateEntities(pairs, CreateLwPolylint, IgnoreLineType, out bool containsSplines));

            // если требуется разбивать полилинии на несколько более простых объектов
            if (CreateArcFromVertex)
            {
                CreateArc();
            }

            // если задан параметр не игнорировать объекты с недопустимым цветом, удалить их из коллекции
            if (!IgnoreLineType)
                DeleteNoColor();

            ContainsSplines = containsSplines;
        }

        public static List<IDxfEntity> CreateEntities(List<DxfCodePair> pairs, bool createLWPolyline, bool ignoreLineType, out bool containsSplines)
        {
            List<IDxfEntity> Entities = new List<IDxfEntity>();
            containsSplines = false;

            int index = pairs.FindIndex(p => p.Code == 0);

            for (int i = index; i < pairs.Count; i++)
            {
                if (pairs[i].Code == 0 && (pairs[i].Value == "ENDSEC" || pairs[i].Value == "ENDBLK")) break;
                if (pairs[i].Code == 0)
                    switch (pairs[i].Value)
                    {
                        case EntitiesVariableCode.Arc:      // создать дугу
                            index = pairs.FindIndex(i + 1, j => j.Code == 0);
                            DxfArc arc = DxfArc.Create(pairs.GetRange(i, index - i), ignoreLineType);
                            if (arc != null)
                                Entities.Add(arc);
                            break;
                        case EntitiesVariableCode.Circle:   // создать окружность
                            index = pairs.FindIndex(i + 1, j => j.Code == 0);
                            DxfCircle circle = DxfCircle.Create(pairs.GetRange(i, index - i), ignoreLineType);
                            if (circle != null)
                                Entities.Add(circle);
                            break;
                        case EntitiesVariableCode.Ellipse:  // создать эллипс
                            index = pairs.FindIndex(i + 1, j => j.Code == 0);
                            DxfEllipse ellipse = DxfEllipse.Create(pairs.GetRange(i, index - i), ignoreLineType);
                            if (ellipse != null)
                                Entities.Add(ellipse);
                            break;
                        case EntitiesVariableCode.Insert:   // создать объект insert
                            index = pairs.FindIndex(i, j => j.Code == 2);
                            Entities.Add(DxfInsert.Create(pairs[index], ignoreLineType));
                            index = pairs.FindIndex(index, j => j.Code == 0);                            
                            break;
                        case EntitiesVariableCode.Line:     // создать отрезок
                            index = pairs.FindIndex(i + 1, j => j.Code == 0);
                            DxfLine line = DxfLine.Create(pairs.GetRange(i, index - i), ignoreLineType);
                            if (line != null)
                                Entities.Add(line);
                            break;
                        case EntitiesVariableCode.LWPolyline:
                            index = pairs.FindIndex(i + 1, j => j.Code == 0);
                            if (createLWPolyline)  // если createLWPolyline = true создать объект LWPOLYLINE
                            {
                                LwPolyline lwPol = LwPolyline.Create(pairs.GetRange(i, index - i), ignoreLineType);
                                if (lwPol != null)
                                    Entities.Add(lwPol);
                            }
                            else                   // если false, создать POLYLINE
                            {
                                DxfPolyline pol = DxfPolyline.Create(pairs.GetRange(i, index - i), ignoreLineType);
                                if (pol != null)
                                    Entities.Add(pol);
                            }
                            break;
                        case EntitiesVariableCode.Point: // создать точку
                            index = pairs.FindIndex(i + 1, j => j.Code == 0);
                            DxfPoint point = DxfPoint.Create(pairs.GetRange(i, index - i), ignoreLineType);
                            if (point != null)
                                Entities.Add(point);
                            break;
                        case EntitiesVariableCode.Polyline:
                            index = pairs.FindIndex(i + 1, j => j.Code == 0 && j.Value == EntitiesVariableCode.Seqend);
                            if (createLWPolyline)  // если createLWPolyline = true создать объект LWPOLYLINE
                            {
                                LwPolyline lwPol = LwPolyline.Create(pairs.GetRange(i, index - i + 1), ignoreLineType);
                                if (lwPol != null)
                                    Entities.Add(lwPol);
                            }
                            else                   // если false, создать POLYLINE
                            {
                                DxfPolyline pol = DxfPolyline.Create(pairs.GetRange(i, index - i + 1), ignoreLineType);
                                if (pol != null)
                                    Entities.Add(pol);
                            }
                            break;
                        case EntitiesVariableCode.Spline:   // объект сплайн не верно интерпритируется программой ajancam, поэтому его следует конвертировать в допустимые объекты
                            index = pairs.FindIndex(i + 1, j => j.Code == 0);
                            DxfSpline spline = DxfSpline.Create(pairs.GetRange(i, index - i), ignoreLineType);
                            if (spline != null)
                            {
                                Entities.Add(spline);
                                containsSplines = true;
                            }

                            break;
                        default:
                            index = pairs.FindIndex(i + 1, p => p.Code == 0);
                            break;
                    }
                i = index - 1;
            }

            return Entities;
        }

        // Удаление объектов имеющих не установленные цвета линий
        public bool DeleteNoColor()
        {
            if (Entities.Exists(e => e.Color == DxfColors.NoColor))
            {
                Entities.RemoveAll(e => e.Color == DxfColors.NoColor);
                return true;
            }
            return false;
        }

        // Создание полилинии из не полного эллипса, первый параметр - показатель какой именно объект создавать,
        // второй "точность" - определяет количество вершин которые будет иметь полилиния после преобразования
        public bool CreatePolylineFromEllipse(bool createLWPolyline, int precision)
        {
            bool isDone = false;

            if (Entities.Exists(e => e.Name == EntitiesVariableCode.Ellipse))
            {
                int index = Entities.FindIndex(e => e.Name == EntitiesVariableCode.Ellipse);
                IDxfEntity entity = null;
                for (int i = index; i < Entities.Count; i++)
                {
                    if ((Entities[i] as DxfEllipse).StartParam != 0.0 ||
                        Math.Round((Entities[i] as DxfEllipse).EndParam, 6) != Math.Round(DxfHelper.TwoPi, 6))
                    {
                        entity = createLWPolyline ? (Entities[i] as DxfEllipse).ToLwPolyline(precision) :
                            (IDxfEntity)(Entities[i] as DxfEllipse).ToPolyline(precision);
                        Entities.RemoveAt(i);
                        Entities.Insert(i, entity);
                        isDone = true;
                    }
                    index = Entities.FindIndex(i + 1, e => e.Name == EntitiesVariableCode.Ellipse);
                    i = index == -1 ? Entities.Count - 1 : index - 1;
                }
            }
            return isDone;
        }

        // Разбиение полилинии, имеющей в своих вершинах коэфициент искривление на более простые объекты,
        // линий, полилинии без искривлений и дуги
        public bool CreateArc()
        {
            bool isDone = false;

            // определить существуют ли объекты с установленными коэфициентами искривления отличными от 0
            if (Entities.Exists(e => e.Name == EntitiesVariableCode.Polyline && ((DxfPolyline)e).IsBulge == true ||
            e.Name == EntitiesVariableCode.LWPolyline && ((LwPolyline)e).IsBulge == true))
            {
                // Если есть такие объекты, разбить из на более простые и вставить их на место обнаруженного
                List<IDxfEntity> ent = null;

                for (int i = 0; i < Entities.Count; i++)
                {
                    switch (Entities[i].Name)
                    {
                        case EntitiesVariableCode.Polyline:
                            if ((Entities[i] as DxfPolyline).IsBulge)
                            {
                                ent = (Entities[i] as DxfPolyline).GetEntitiesFromPolyline();
                                Entities.RemoveAt(i);
                                Entities.InsertRange(i, ent);
                                i = i + ent.Count - 1;
                                isDone = true;
                            }
                            break;
                        case EntitiesVariableCode.LWPolyline:
                            if ((Entities[i] as LwPolyline).IsBulge)
                            {
                                ent = (Entities[i] as LwPolyline).GetEntitiesFromLWPolyline();
                                Entities.RemoveAt(i);
                                Entities.InsertRange(i, ent);
                                i = i + ent.Count - 1;
                                isDone = true;
                            }
                            break;
                    }
                }
            }
            return isDone;
        }

        public bool ConvertPolylineToLWPolyline()
        {            
            if (!Entities.Exists(e => e.Name == EntitiesVariableCode.Polyline)) return false;

            int index = Entities.FindIndex(e => e.Name == EntitiesVariableCode.Polyline);

            for (int i = index; i < Entities.Count; i++)
            {
                LwPolyline lPolyline = LwPolyline.Create(Entities[i] as DxfPolyline);
                Entities.RemoveAt(i);
                Entities.Insert(i, lPolyline);
                index = Entities.FindIndex(i, e => e.Name == EntitiesVariableCode.Polyline);
                if (index == -1) break;
                i = index - 1;
            }
            return true;
        }

        public bool ConvertLWPolylineToPolyline()
        {
            if (!Entities.Exists(e => e.Name == EntitiesVariableCode.LWPolyline)) return false;

            int index = Entities.FindIndex(e => e.Name == EntitiesVariableCode.LWPolyline);
            
            for(int i = index;i<Entities.Count;i++)
            {
                DxfPolyline polyline = (Entities[i] as LwPolyline).ToPolyline();
                Entities.RemoveAt(i);
                Entities.Insert(i, polyline);
                index = Entities.FindIndex(i, e => e.Name == EntitiesVariableCode.LWPolyline);
                if (index == -1) break;
                i = index - 1;
            }
            return true;
        }

        public bool ChangePointsColor(DxfColors color)
        {
            if (!Entities.Exists(e => e is DxfPoint)) return false;

            int index = Entities.FindIndex(e => e is DxfPoint);

            for(int i = index; i<Entities.Count; i++)
            {
                (Entities[i] as DxfPoint).SetColor(color);
                if (i + 1 == Entities.Count) break;
                index = Entities.FindIndex(i + 1, e => e is DxfPoint);
                if (index == -1) break;
                i = index - 1;
            }

            return true;
        }

        public bool DeletePoints()
        {
            if (!Entities.Exists(e => e is DxfPoint)) return false;

            return Entities.RemoveAll(e => e is DxfPoint) == 0 ? false : true;
        }

        public int PointsCount()
        {
            if (!Entities.Exists(e => e is DxfPoint)) return 0;

            List<IDxfEntity> points = Entities.FindAll(e => e is DxfPoint);
            return points.Count;
        }

        // Формирование строки для вывода в файл DXF
        public override string ToString()
        {
            string str = "0\nSECTION\n2\nENTITIES\n";
            foreach (IDxfEntity entity in Entities)
            {
                if (CreateR13)
                    str += entity.ToStringOfVerR13();
                else
                    str += entity.ToString();
            }
            str += "0\nENDSEC\n";
            return str;
        }

        // Смещение всех объектов на заданый вектор
        public void OffSet(Vector vector)
        {
            foreach (IDxfOffset obj in Entities)
                obj.OffSet(vector);
        }

        // Форматированная строка для вывода в файл R13
        public string ToStringOfVerR13()
        {
            return ToString();
        }

        // Создание полилиний из сплайнов
        public bool SplineToPolyline(int precision)
        {
            int index = Entities.FindIndex(e => e.Name == EntitiesVariableCode.Spline);
            if (index == -1) return false;

            for (int i = index; i < Entities.Count; i++)
            {
                if (Entities[i].Name == EntitiesVariableCode.Spline)
                {
                    DxfPolyline pol = (Entities[i] as DxfSpline).SplineToPolyline(precision);
                    Entities.RemoveAt(i);
                    Entities.Insert(i, pol);
                }
            }
            ContainsSplines = false;

            if (CreateLwPolylint)
                return ConvertPolylineToLWPolyline();

            return true;
        }
        #endregion
    }
}
