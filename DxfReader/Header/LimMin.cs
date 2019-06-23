using System.Windows;

namespace DxfReader
{
    // Секция LIMMIN
    class LimMin : DxfDot2D, IDxfHeaderValue
    {
        // название секции всегда должно быть постоянным
        const string name = HeaderVariableCode.LimMin;

        #region Открытые только для чтения свойства

        public string Name { get { return name; } }     // имя секции

        #endregion
        #region Конструктор

        public LimMin(DxfDot2D value)
            : base(value.X, value.Y) { }

        public LimMin() : this(new DxfDot2D(0, 0)) { }

        #endregion
        #region Открытые методы

        // Создание класса из пар кодов
        public new static LimMin Create(System.Collections.Generic.List<DxfCodePair> pairs) => new LimMin(DxfDot.Create(pairs).AsPoint);

        // Формирование строки для вывода в файл DXF
        public override string ToString() => string.Format("9\n{0}\n{1}", Name, base.ToString());

        public override void OffSet(Vector vector)
        {
            
        }

        #endregion
    }
}
