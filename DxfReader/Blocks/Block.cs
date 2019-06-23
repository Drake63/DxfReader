using System.Collections.Generic;

namespace DxfReader
{
    // Секция BLOCK
    class Block
    {
        #region Открытые только для чтения свойства

        public string Name { get; private set; }                // имя блока, указное в файле DXF под кодом 2
        public List<DxfCodePair> Pairs { get; private set; }    // список пар кодов этого блока, для передачи его в метод создания графических объектов

        #endregion
        #region Конструктор класса, нельзя создавать блок непосредственно

        private Block()
        {
            Pairs = new List<DxfCodePair>();
        }

        #endregion
        #region Открытые методы

        // Создание блока из пар кодов
        public static Block Create(List<DxfCodePair> pairs)
        {
            Block block = new Block
            {
                Name = pairs.Find(p => p.Code == 2).Value
            };
            block.Pairs.AddRange(pairs);
            return block;
        } 

        #endregion
    }
}
