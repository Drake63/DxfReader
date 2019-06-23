using System.Collections.Generic;
using System.Windows;

namespace DxfReader
{
    // Секция Header файла DXF
    class DxfHeader : IDxfOffset
    {
        // Список требуемых значения заголовочной секции 
        Dictionary<string, IDxfHeaderValue> variables;

        #region Открытые только для чтения свойства

        // вся коллекция заголовочных секций
        public ICollection<IDxfHeaderValue> Values { get { return variables.Values; } }
        // секция версии файла
        public DxfAcadVer AcadVer
        {
            get { return (DxfAcadVer)variables[HeaderVariableCode.AcadVer]; }
            private set { variables[HeaderVariableCode.AcadVer] = value; }
        }
        // секция INSBASE
        public InsBase InsBase
        {
            get { return (InsBase)variables[HeaderVariableCode.InsBase]; }
            private set { variables[HeaderVariableCode.InsBase] = value; }
        }
        // секция EXTMIN
        public ExtMin ExtMin
        {
            get { return (ExtMin)variables[HeaderVariableCode.ExtMin]; }
            set { variables[HeaderVariableCode.ExtMin] = value; }
        }
        // секция EXTMAX
        public ExtMax ExtMax
        {
            get { return (ExtMax)variables[HeaderVariableCode.ExtMax]; }
            set { variables[HeaderVariableCode.ExtMax] = value; }
        }
        // секция LIMMIN
        public LimMin LimMin
        {
            get { return (LimMin)variables[HeaderVariableCode.LimMin]; }
            private set { variables[HeaderVariableCode.LimMin] = value; }
        }
        // секция LIMMAX
        public LimMax LimMax
        {
            get { return (LimMax)variables[HeaderVariableCode.LimMax]; }
            private set { variables[HeaderVariableCode.LimMax] = value; }
        }
        // секция MEASUREMENT
        public Measurement Measurement
        {
            get { return (Measurement)variables[HeaderVariableCode.Measurement]; }
            private set { variables[HeaderVariableCode.Measurement] = value; }
        }

        #endregion
        #region Конструктор

        public DxfHeader()
        {
            // создать секцию заголовка со значениями по умолчанию
            variables = new Dictionary<string, IDxfHeaderValue>()
            {
                {HeaderVariableCode.AcadVer, new DxfAcadVer() },
                {HeaderVariableCode.InsBase, new InsBase() },
                {HeaderVariableCode.ExtMin, new ExtMin(DxfDot.Zero) },
                {HeaderVariableCode.ExtMax, new ExtMax(DxfDot.Zero) },
                {HeaderVariableCode.LimMin, new LimMin(new DxfDot2D()) },
                {HeaderVariableCode.LimMax, new LimMax(new DxfDot2D(420,297)) },
                {HeaderVariableCode.Measurement, new Measurement() }
            };
        }

        #endregion
        #region Открытые методы

        // Создание класса из пар кодов
        public static DxfHeader Create(List<DxfCodePair> pairs)
        {
            DxfHeader header = new DxfHeader();

            int index = GetIndexAndCount(pairs, HeaderVariableCode.ExtMin, out int count);
            if (index != -1)
                header.ExtMin = ExtMin.Create(pairs.GetRange(index, count));

            index = GetIndexAndCount(pairs, HeaderVariableCode.ExtMax, out count);
            if (index != -1)
                header.ExtMax = ExtMax.Create(pairs.GetRange(index, count));

            return header;
        }

        // Формирование форматированной строки для вывода в файл DXF
        public override string ToString()
        {
            string str = "0\nSECTION\n2\nHEADER\n";
            foreach (IDxfHeaderValue value in Values)
                if (value != null)
                    str += value.ToString();
            str += "0\nENDSEC\n";

            return str;
        }

        // Сдвиг значений координат, на заданный вектор
        public void OffSet(Vector vector)
        {
            foreach (IDxfHeaderValue value in Values)
                if (value is IDxfOffset)
                    (value as IDxfOffset).OffSet(vector);
        }

        #endregion
        #region Вспомогательные методы

        // поиск индекса требуемой пары кодов и их количества
        static int GetIndexAndCount(List<DxfCodePair> pairs, string name, out int count)
        {
            // найти индекс труемой пары
            int index = pairs.FindIndex(p => p.Value == name);

            // найти количество нужных кодов, для передачи методу создания объекта
            count = index != -1 ? pairs.FindIndex(index + 1, p => p.Code == 9) : 0;
            count = count == -1 ? pairs.FindIndex(index + 1, p => p.Code == 0) - index : count - index;

            return index;
        }

        public string ToStringOfVerR13()
        {
            return string.Empty;
        }

        public void ChangeVersion(bool isR13)
        {
            AcadVer.ChangeVersion(isR13);
        }

        #endregion
    }
}
