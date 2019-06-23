using System.Collections.Generic;
using System.Windows;

namespace DxfReader
{
    // Секция INSBASE
    class InsBase : DxfDot, IDxfHeaderValue
    {
        // название секции всегда должно быть постоянным
        const string name = HeaderVariableCode.InsBase;

        #region Открытые только для чтения свойства 

        public string Name { get { return name; } }     // имя секции

        #endregion
        #region Конструкторы

        public InsBase(DxfDot value)
            : base(value.X, value.Y, value.Z) { }
        public InsBase() : this(DxfDot.Zero) { }

        #endregion
        #region Открытые методы

        // Создание класса из пар кодов
        public new static InsBase Create(List<DxfCodePair> pairs) => new InsBase(DxfDot.Create(pairs));

        // Создание форматированной строки для вывода в файл DXF
        public override string ToString() => string.Format("9\n{0}\n{1}", Name, base.ToString());

        public override void OffSet(Vector vector)
        {
            
        }

        #endregion
    }
}
