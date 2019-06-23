using System.Windows;

namespace DxfReader
{
    // Секция EXTMIN
    internal class ExtMin : DxfDot, IDxfHeaderValue
    {
        // имя секции в файле DXF должно быть постоянным
        const string name = HeaderVariableCode.ExtMin;

        #region Открытые только для чтения свойства

        public string Name { get { return name; } }     // имя в файле DXF

        #endregion
        #region Конструктор

        public ExtMin(DxfDot dot) :
            base(DxfHelper.NormalizeDouble(dot.X), DxfHelper.NormalizeDouble(dot.Y), DxfHelper.NormalizeDouble(dot.Z))
        { }

        #endregion
        #region Открытые методы

        // Создание класса из пар кодов
        public new static ExtMin Create(System.Collections.Generic.List<DxfCodePair> pairs) => new ExtMin(DxfDot.Create(pairs));

        // Формирование строки для вывода в файл DXF
        public override string ToString() => string.Format("9\n{0}\n{1}", Name, base.ToString());

        public override string ToStringOfVerR13() => string.Empty;

        public override int GetHashCode() => base.GetHashCode() ^ Name.GetHashCode();

        public new ExtMin Copy() => new ExtMin(base.Copy());
        #endregion
    }
}
