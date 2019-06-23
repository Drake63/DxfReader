namespace DxfReader
{
    // Секция MEASUREMENT
    class Measurement : IDxfHeaderValue
    {
        // название секции вседа должно быть одинаковым
        const string name = HeaderVariableCode.Measurement;
        #region Открытые только для чтения свойства

        public string Name { get { return name; } }     // имя секции
        public byte Flag { get; private set; }          // значение

        #endregion
        #region Конструктор

        // Флаг по умолчанию равен 1, т.е. метрическая система измерения
        public Measurement(byte flag = 1)
        {
            Flag = flag;
        }

        #endregion
        #region Открытые методы

        // Создание класса из пар кодов
        public static Measurement Create(System.Collections.Generic.List<DxfCodePair> pairs)
        {
            byte fl = pairs.Exists(p => p.Code == 70) ? pairs.Find(p => p.Code == 70).AsByte : (byte)1;
            return new Measurement(fl);
        }

        // Создание строки из пар кодов
        public override string ToString()
        {
            return string.Format("9\n{0}\n70\n{1}\n", Name, Flag);
        } 

        #endregion
    }
}
