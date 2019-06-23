using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Text;

namespace DxfReader
{
    /// <summary>
    /// Dxf документ, создается только из существующего файла и не может быть изменен после.
    /// Оптимизирует сущестующий файл, оставляет только секцию с графическими примитивами,
    /// приспособлен для работы с оборудованием AJAN
    /// </summary>
    public class DxfDocument
    {
        private int stringCount;
        private Rect bound;
        private ushort flags;
        //private bool isChanged;
        #region Свойства класса

        internal DxfHeader Header { get; private set; }         // заголовочный блок
        internal DxfBlocks Blocks { get; private set; }         // блоки
        internal DxfEntities Entities { get; private set; }     // блок примитивов 

        /// <summary>
        /// Флаги, требуемые для создания документа
        /// </summary>
        public ushort Flags
        {
            get => flags;
            set
            {
                flags = value;
                if (Entities.ContainsSplines)
                    DxfHelper.CheckFlag(ref flags, (ushort)DxfFlags.ContainsSplines);
            }
        }
        /// <summary>
        /// Создавать объекты LWPOLYLINE, если нет будут созданы объекты POLYLLINE
        /// </summary>
        public bool CreateLwPolyline { get => DxfHelper.GetFlag(Flags, (ushort)DxfFlags.CreateLwPolyline); }
        /// <summary>
        /// Размер графического содержимого в файле
        /// </summary>
        public Size Size { get { return new Size(bound.Width, bound.Height); } }
        /// <summary>
        /// Показывает количество содежащихся графических объектов в секции ENTITIES
        /// </summary>
        public int EntitiesCount { get { return Entities.Entities.Count; } }
        /// <summary>
        /// Показывает создан ли документ из файла
        /// </summary>
        public bool IsCreated { get; private set; }
        /// <summary>
        /// Возвращает и устанавливает флаг для создания дуг из объектов VERTEX, если в них устанволен параметр искривленности
        /// отличный от 0, если параметр имеет значение true - будут созданы дуги и другие необходимые объекты, которые заменят
        /// существующие, если false - ни чего изменяться не будет, значение по умолчанию false.
        /// </summary>
        public bool CreateArc { get => DxfHelper.GetFlag(Flags, (ushort)DxfFlags.CreateArc); }
        /// <summary>
        /// Параметр указывающий будет ли содежимое файла DXF сдвинуто к началу координат, так чтобы не было отрицательных значений
        /// и наимешая координата совпадала с координатой 0,0. По умолчанию параметр имеет значение true.
        /// </summary>
        public bool MoveToZero { get => DxfHelper.GetFlag(Flags, (ushort)DxfFlags.MoveToZero); }
        /// <summary>
        /// Имя файла из которого, создан документ, или под которым затем сохранен
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// Преобразовывать дугу эллипса в полилинию, программа AJAN не распознает не полный эллипс, 
        /// поэтому может понадобится преобразовать его в полилинию. По умолчанию параметр имеет значение false
        /// </summary>
        public bool PolylineFromEllipse { get => DxfHelper.GetFlag(Flags, (ushort)DxfFlags.PolylineFromEllipse); }
        /// <summary>
        /// Удалять все объекты, которые имеют тип линий отличный от установленных - основного контура, разметки и незамкнутого контура.
        /// По умолчанию все эти объекты удаляются.
        /// </summary>
        public bool IgnoreOtherLineType { get => DxfHelper.GetFlag(Flags, (ushort)DxfFlags.IgnoreOtherLineType); }
        /// <summary>
        /// Показывает, находится ли левая нижняя точка документа в начале координат
        /// </summary>
        public bool IsZero { get { return Header.ExtMin.X == 0 && Header.ExtMin.Y == 0; } }
        /// <summary>
        /// Показывает, имеются в документе точки не помеченные как незамкнутый контур или разметка
        /// </summary>
        public bool IsPointsAsMainLine { get { return Entities.Entities.Exists(e => e is DxfPoint && e.Color == DxfColors.MainOutline); } }
        /// <summary>
        /// Показывает имеются ли в документе точки
        /// </summary>
        public bool IsPoints { get { return Entities.Entities.Exists(e => e is DxfPoint); } }
        /// <summary>
        /// Возвращает количество точек в документе, если такие имеются
        /// </summary>
        public int PointsCount { get { return Entities.PointsCount(); } }
        /// <summary>
        /// Число строк в файле
        /// </summary>
        public int StringCount { get { return stringCount; } }
        /// <summary>
        /// Прямоугольная область в которой размещаются графические объекты
        /// </summary>
        public Rect Bound { get { return bound; } }
        /// <summary>
        /// Создавать документ версии R13
        /// </summary>
        public bool IsR13 { get => DxfHelper.GetFlag(Flags, (ushort)DxfFlags.CreateR13); }
        /// <summary>
        /// Записывать в строку только секцию Entities
        /// </summary>
        public bool OnlyEntities { get => DxfHelper.GetFlag(Flags, (ushort)DxfFlags.OnlyEntities); }
        /// <summary>
        /// Показывает, есть ли в документе сплайны
        /// </summary>
        public bool ContainsSplines => Entities.ContainsSplines;
        /// <summary>
        /// Точность с которой следует строить полилинию из элиптической дуги
        /// </summary>
        public int EllipsePrecision { get; set; }

        #endregion

        /// <summary>
        /// Конструктор класса создает пустой документ со свойствами по умолчанию
        /// </summary>
        public DxfDocument()
        {
            Header = new DxfHeader();
            Blocks = new DxfBlocks();
            Entities = new DxfEntities();

        }

        /// <summary>
        /// Создание документа из текста
        /// </summary>
        /// <param name="obj">Контент полученный как текст из элемента управления</param>
        public void Create(object obj)
        {
            if (!(obj is string)) return;

            List<DxfCodePair> pairs = new List<DxfCodePair>();


            using (StringReader reader = new StringReader(obj as string))
            {
                while (reader.Peek() != -1)
                {
                    DxfCodePair pair = new DxfCodePair(reader);
                    pairs.Add(pair);
                }
            }

            IsCreated = PairsParse(pairs);
        }

        /// <summary>
        /// Создать документ из существующего файла
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        public void Create(string fileName)
        {            
            if (fileName == "" && fileName == null) return;

            // создать новый документ
            List<DxfCodePair> pairs = new List<DxfCodePair>();  // создать массив пар код - значение
            string[] text = File.ReadAllLines(fileName);
            for(int i=0; i<text.Length;i++)
            {
                DxfCodePair pair = new DxfCodePair(text[i], text[++i]);
                pairs.Add(pair);
            }
            // открыть заданный файл для чтения
            //using (StreamReader reader = new StreamReader(fileName))
            //{
            //    while (!reader.EndOfStream)
            //    {
            //        // заполнить массив полученными данными
            //        DxfCodePair pair = new DxfCodePair(reader);
            //        pairs.Add(pair);
            //    }
            //}
            
            IsCreated = PairsParse(pairs);
            FileName = fileName;
        }

        private bool PairsParse(List<DxfCodePair> pairs)
        {
            stringCount = pairs.Count * 2;

            Blocks.Blocks.Clear();
            Entities.Entities.Clear();
            // Создать необходимые элементы документа
            int index = pairs.FindIndex(p => p.Code == 2 && p.Value == "HEADER" || p.Value == "BLOCKS" || p.Code == 2 && p.Value == "ENTITIES");
            for (int i = index; i < pairs.Count; i++)
            {
                if (pairs[i].Code == 2 && pairs[i].Value == "OBJECTS") break;
                switch (pairs[i].Value)
                {
                    case "HEADER":      // заголовок
                        index = pairs.FindIndex(i, j => j.Code == 0 && j.Value == "SECTION");
                        Header = DxfHeader.Create(pairs.GetRange(i, index - i));
                        break;
                    case "BLOCKS":      // блоки
                        index = pairs.FindIndex(i, j => j.Code == 0 && j.Value == "SECTION");
                        Blocks = DxfBlocks.Create(pairs.GetRange(i, index - i));
                        break;
                    case "ENTITIES":    // графические элементы
                        index = pairs.FindIndex(i, j => j.Code == 0 && j.Value == "SECTION");
                        if (index == -1) index = pairs.FindIndex(i, j => j.Code == 0 && j.Value == "ENDSEC") + 1;
                        Entities.Create(pairs.GetRange(i, index - i), Flags);
                        break;
                    default:    // все остальные секции пропускать
                        index = pairs.FindIndex(i, j => j.Code == 0 && j.Value == "SECTION");
                        break;
                }
                i = index != -1 ? pairs.FindIndex(index, p => p.Code == 2) - 1 : pairs.Count - 1;
                if (i < 0) break;
            }

            //if (Entities.Entities.Count == 0) return;

            ExtMin baseMin = Header.ExtMin.Copy();

            // если в блоке примитивов есть объекты Insert
            if (Entities.Entities.Exists(e => e.Name == EntitiesVariableCode.Insert))
            {
                // найти индекс первого блока Insert
                index = Entities.Entities.FindIndex(e => e.Name == EntitiesVariableCode.Insert);
                for (int i = index; i < Entities.Entities.Count; i++)
                {
                    DxfInsert ins = Entities.Entities[i] as DxfInsert;

                    if (!(Blocks.Blocks.Exists(b => b.Name == ins.BlockName)))
                        throw new ArgumentException(string.Format("{0} {1} {2}", Properties.Resource.BlockName, ins.BlockName, Properties.Resource.NotFound));
                    List<IDxfEntity> entities = DxfEntities.CreateEntities(Blocks.Blocks.Find(b => b.Name == ins.BlockName).Pairs,
                        CreateLwPolyline, IgnoreOtherLineType, out bool containsSplines);

                    if (containsSplines && !Entities.ContainsSplines)
                        Entities.ContainsSplines = containsSplines;
                    // удалить текущий блок Insert и на его место вставить коллекцию извлеченных примитивов
                    Entities.Entities.RemoveAt(i);
                    if (entities.Count > 0)
                        Entities.Entities.InsertRange(i, entities);

                    // продолжить поиск блоков Insert
                    index = Entities.Entities.FindIndex(i, e => e.Name == EntitiesVariableCode.Insert);
                    if (index == -1) break;
                    i = index - 1;
                }
            }

            // очистить коллекцию блоков
            Blocks.Blocks.Clear();

            if (PolylineFromEllipse)
                CreatePolylinesFromEllises(50);

            List<double> ordX = new List<double>();
            List<double> ordY = new List<double>();

            foreach (IDxfEntity ent in Entities.Entities)
            {
                ordX.AddRange(new double[] { ent.Bound.Left, ent.Bound.Right });
                ordY.AddRange(new double[] { ent.Bound.Bottom, ent.Bound.Top });
            }
            ordX.Sort();
            ordY.Sort();
            Rect rt = new Rect(0, 0, 0, 0);
            if (Entities.Entities.Count != 0)
                rt = new Rect(ordX[0], ordY[0], ordX[ordX.Count - 1] - ordX[0], ordY[ordY.Count - 1] - ordY[0]);
            bound = rt;

            Header.ExtMin = new ExtMin(new DxfDot(bound.Left, bound.Top));
            Header.ExtMax = new ExtMax(new DxfDot(bound.Right, bound.Bottom));


            // если установлен флаг moveToZero, пересчитать все координаты, и установить левый нижний угол на нулевую точку
            if (MoveToZero)
            {
                MoveEntitiesToZero();
            }

            if (Header.ExtMin.Equals(baseMin))
                DxfHelper.UncheckFlag(ref flags, (ushort)DxfFlags.Changed);
            else
                DxfHelper.CheckFlag(ref flags, (ushort)DxfFlags.Changed);

            if (Entities.ContainsSplines)
                DxfHelper.CheckFlag(ref flags, (ushort)DxfFlags.ContainsSplines);

            return true;
        }

        /// <summary>
        /// Смещение всех объектов так, чтоб левый нижний угол совпал с началом координат
        /// </summary>
        /// <returns>Возвращаемое значение показывает, произошло ли преобразование</returns>
        public bool MoveEntitiesToZero()
        {
            if (Header.ExtMin.X == 0.0 && Header.ExtMin.Y == 0) return false;
            Vector vector = new Vector(Header.ExtMin.X, Header.ExtMin.Y);
            foreach (IDxfHeaderValue header in Header.Values)
                if (header is IDxfOffset)
                    (header as IDxfOffset).OffSet(vector);
            foreach (IDxfEntity entity in Entities.Entities)
                entity.OffSet(vector);
            Header.ExtMin = new ExtMin(DxfDot.Zero);
            return true;
        }

        /// <summary>
        /// Создание полилинии из не полных эллипсов в документе, если они существуют
        /// </summary>
        /// <param name="precision">Точность с которой будут создаваться полилинии, указывается желательное количество вершин</param>
        /// <returns>Возвращаемое значение показывает, произошло ли преобразование</returns>
        public bool CreatePolylinesFromEllises(int precision)
        {
            return Entities.CreatePolylineFromEllipse(CreateLwPolyline, precision);
        }

        /// <summary>
        /// Разбиение существующих полилиний на более простые объекты, если в них указаны параметры искривления
        /// </summary>
        /// <returns>Возвращаемое значение показывает, произошло ли преобразование</returns>
        public bool CreateArcFromPolyline()
        {
            return Entities.CreateArc();
        }

        /// <summary>
        /// Удаление объектов с типом линий, отличных от требуемых
        /// </summary>
        /// <returns>Возвращаемое значение показывает, произошло ли преобразование</returns>
        public bool DeleteOtherLineType()
        {
            return Entities.DeleteNoColor();
        }
         /// <summary>
         /// Преобразование полилиний
         /// </summary>
         /// <param name="toLwPolyline">Параметр, указывающий должна ли формироваться LWPolyline</param>
         /// <returns>Возвращаемое значение показывает, произошло ли преобразование</returns>
        public bool ConvertPolyline(bool toLwPolyline)
        {
            if (toLwPolyline)
                return Entities.ConvertPolylineToLWPolyline();
            else
                return Entities.ConvertLWPolylineToPolyline();
        }

        /// <summary>
        /// Изменяет цвет точки на указанный
        /// </summary>
        /// <param name="color">Цвет из перечисления DxfColors</param>
        /// <returns>Метод возвращает true, если цвет был изменен</returns>
        public bool ChangePointsColor(DxfColors color)
        {
            return Entities.ChangePointsColor(color);
        }

        /// <summary>
        /// Удаление всех точек из документа
        /// </summary>
        /// <returns>Если удаление произошло, возвращается true</returns>
        public bool DeletePoints()
        {
            return Entities.DeletePoints();
        }

        /// <summary>
        /// Возвращает содержимое файла в виде строк, для передачи его в файл DXF
        /// </summary>
        /// <returns>Возвращает результирующую строку</returns>
        public override string ToString()
        {
            if (!IsCreated) return string.Empty;

            string str = "";

            if (OnlyEntities)
            {
                str = Entities.ToString();
                return str;
            }

            Header.ChangeVersion(IsR13);

            str = Header.ToString();
            if (IsR13)
                str += DxfTables.BlockOfTables + Blocks.ToString() + Entities.ToString() + "0\nEOF\n";
            else
                str += Blocks.ToString() + Entities.ToString() + "0\nEOF\n";

            return str;
        }

        /// <summary>
        /// Удаление всех данных из документа
        /// </summary>
        public void Clear()
        {
            FileName = "";
            Entities.Entities.Clear();
            Blocks.Blocks.Clear();
            Header = new DxfHeader();
            IsCreated = false;
        }
        
        /// <summary>
        /// Генерирует объект DrawingVisual для передачи его в элеметн управления, который будет отображать графическое содержимое файла
        /// </summary>
        /// <param name="size">Требуемый размер изображения</param>
        /// <returns>Возвращает объект DrawingVisual</returns>
        public DrawingVisual CreateVisual(Size size)
        {
            DrawingVisual visual = new DrawingVisual();
            double scaleX = size.Width / (bound.Width + 10);
            double scaleY = size.Height / (bound.Height + 10);
            double scale = scaleX > scaleY ? Math.Abs(scaleY) : Math.Abs(scaleX);
            foreach (IDxfEntity ent in Entities.Entities)
            {
                DrawingVisual vis = ent.CreateVisual(scale);
                if (vis != null)
                    visual.Children.Add(vis);
            }
            Rect rect = visual.ContentBounds;
            rect.Union(visual.DescendantBounds);

            visual.Offset = new Vector(visual.Offset.X - Header.ExtMin.X * scale - 5, visual.Offset.Y - Header.ExtMin.Y * scale - 5);
            return visual;
        }

        /// <summary>
        /// Переключение флагов документа
        /// </summary>
        /// <param name="mask">Праметр из перечня DxfFlags</param>
        public void ChangeFlags(DxfFlags mask)
        {
            Flags ^= (ushort)mask;
        }

        /// <summary>
        /// Возвращает значение заданного флага
        /// </summary>
        /// <param name="mask">Флаг, значение которого требуется получить</param>
        /// <returns></returns>
        public bool GetFlag(DxfFlags mask)
        {
            return DxfHelper.GetFlag(flags, (ushort)mask);
        }

        /// <summary>
        /// Преобразование сплайна в полилинию
        /// </summary>
        /// <param name="precision">Точность с которой нужно преобразовать сплайн (количество вершин полилинии)</param> 
        /// <returns>Возвращает true, есил преобразование прошло успещно и false в противном случае</returns>
        public bool SplineToPolyline(int precision)
        {
            if (!Entities.Entities.Exists(e => e.Name == EntitiesVariableCode.Spline))
                throw new EntitiesNoFoundException(Properties.Resource.SplinesNotFound);

            return Entities.SplineToPolyline(precision);
        }
    }
}
