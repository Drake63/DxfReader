using System.Collections.Generic;
using System.Windows.Media;
using System.Windows;

namespace DxfReader
{
    // Секция LWPOLYLINE, класс является оберткой для класса DxfPolyline
    class LwPolyline : IDxfEntity
    {
        // название секции всегда должно быть постоянным
        const string name = EntitiesVariableCode.LWPolyline;

        private DxfPolyline polyline; // внутрениий объект POLYLINE

        #region Открытые только для чтения свойства

        public int NumberOfVertices => polyline.Vertexes.Count;     // число вершин полилинии
        public string Name => name;                                 // имя секции
        public DxfColors Color => polyline.Color;                   // цвет линии
        public ushort Flag => polyline.Flags;                       // флаг, является полилиния закрытой или нет
        public bool IsBulge => polyline.IsBulge;                    // показывает наличие вершин с установленным коэфициентом изогнутости
        public Rect Bound => polyline.Bound;                        // 

        #endregion
        #region Закрытый конструктор, нельзя создать класс непосредственно с помощью конструктора

        private LwPolyline() { }

        #endregion
        #region Методы класса

        // Создание объекта из пар кодов
        public static LwPolyline Create(List<DxfCodePair> pairs, bool ignoreLineType)
        {
            LwPolyline lwPolyline = new LwPolyline();
            lwPolyline.polyline = DxfPolyline.Create(pairs, ignoreLineType);
            if (lwPolyline.polyline == null) return null;

            return lwPolyline;
        }

        // Преобразование POLYLINE в LWPOLILINE
        public static LwPolyline Create(DxfPolyline polyline)
        {
            LwPolyline lPolyline = new LwPolyline
            {
                polyline = polyline
            };
            return lPolyline;
        }

        // Перобразование в POLYLINE
        public DxfPolyline ToPolyline()
        {
            return polyline;
        }

        // Строка форматированная для вывода в файл DXF
        public override string ToString()
        {
            string str = string.Format("0\n{0}\n8\n0\n62\n{1}\n90\n{2}\n70\n{3}\n",
                Name, (int)Color, NumberOfVertices, Flag);
            foreach (DxfVertex vert in polyline.Vertexes)
            {
                str += "10\n" + DxfHelper.DoubleToString(vert.X) +
                    "\n20\n" + DxfHelper.DoubleToString(vert.Y) + "\n";
                if (vert.Bulge != 0)
                    str += "42\n" + DxfHelper.DoubleToString(vert.Bulge) + "\n";
            }
            return str;
        }

        public string ToStringOfVerR13()
        {
            string str = ToString();
            str = str.Insert(str.IndexOf("90\n" + NumberOfVertices + "\n"), "100\nAcDbPolyline\n");
            return str;
        }

        // Создание визуального представления объекта
        public DrawingVisual CreateVisual(double scale)
        {
            return polyline.CreateVisual(scale);
        } 

        // Разибить на несколько объектов LWPOLYLINE и ARC
        public List<IDxfEntity> GetEntitiesFromLWPolyline()
        {
            List<IDxfEntity> entities = new List<IDxfEntity>();

            foreach(IDxfEntity ent in polyline.GetEntitiesFromPolyline())
            {
                if (ent is DxfArc || ent is DxfLine) entities.Add(ent);
                else
                {
                    LwPolyline pol = new LwPolyline();
                    pol.polyline = ent as DxfPolyline;
                    entities.Add(pol);
                }
            }
            return entities;
        }

        // Сместить координаты на заданный вектор
        public void OffSet(Vector vector)
        {
            polyline.OffSet(vector);
        }

        #endregion
    }
}
