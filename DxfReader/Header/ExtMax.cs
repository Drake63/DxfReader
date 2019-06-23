using System.Windows;
using System.Collections.Generic;

namespace DxfReader
{
    // Секция EXTMAX
    internal class ExtMax : DxfDot, IDxfHeaderValue
    {
        // имя объекта в файле DXF всегда должно быть постоянным
        const string name = HeaderVariableCode.ExtMax;

        #region Открытые только для чтения свойства

        public string Name { get { return name; } }     // имя объекта в файле DXF

        #endregion
        #region Конструктор

        public ExtMax(DxfDot dot) :
            base(dot.X, dot.Y, dot.Z)
        { }

        #endregion
        #region Открытые методы

        // Создание класса из пар кодов
        public new static ExtMax Create(List<DxfCodePair> pairs) => new ExtMax(DxfDot.Create(pairs));

        // Формирование форматированной строки для вывода в файл DXF
        public override string ToString() => string.Format("9\n{0}\n{1}", Name, base.ToString());

        public override string ToStringOfVerR13() => string.Empty;

        public override int GetHashCode() => base.GetHashCode() ^ Name.GetHashCode();

        public new ExtMax Copy() => new ExtMax(base.Copy());
        #endregion
    }
}
