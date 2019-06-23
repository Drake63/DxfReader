using System.Windows;

namespace DxfReader
{
    // Секция LIMMAX
    internal class LimMax : DxfDot2D, IDxfHeaderValue
    {
        // Назавние секции всегда должно быть постоянным
        const string name = HeaderVariableCode.LimMax;
        #region Открытые свойства

        public string Name { get { return name; } }     // название секции

        #endregion
        #region Конструктор

        public LimMax(DxfDot2D value)
            : base(value.X, value.Y) { }

        #endregion
        #region Открытые методы

        // Создание класса из пар кодов
        public new static LimMax Create(System.Collections.Generic.List<DxfCodePair> pairs) => new LimMax(DxfDot.Create(pairs).AsPoint);

        // Формирование строки для вывода в файл DXF
        public override string ToString() => string.Format("9\n{0}\n{1}", Name, base.ToString());

        public override void OffSet(Vector vector)
        {
            
        }

        #endregion
    }
}
