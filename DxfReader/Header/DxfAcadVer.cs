namespace DxfReader
{
    // Класс представляющий версию файла DXF
    class DxfAcadVer : IDxfHeaderValue
    {
        const string name = HeaderVariableCode.AcadVer; // константа - имя секции всегда имеет одно значение

        #region Отрытые только для чтения свойства

        public string Name { get { return name; } }     // имя секции в файле DXF
        public string Value { get; private set; }       // значение

        #endregion
        #region Конструктор

        // Устанавливается изначально версия по умолчанию R11 или R12
        public DxfAcadVer(string version = "AC1009")
        {
            Value = version;
        }

        #endregion
        #region Открытые методы

        // Изменение версии, если в это необходимо, на версию R13
        public void ChangeVersion(bool isR13)
        {
            if (isR13)
                Value = "AC1012";
            else
                Value = "AC1009";
        }

        // Создание форматированной строки для вывода в файл DXF
        public override string ToString()
        {
            string str = string.Format("9\n{0}\n1\n{1}\n", Name, Value);
            return str;
        } 

        #endregion
    }
}
