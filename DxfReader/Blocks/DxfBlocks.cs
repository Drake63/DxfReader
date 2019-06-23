using System.Collections.Generic;

namespace DxfReader
{
    // Секция  BLOCKS, она нужна только если в секции ENTITIES встречается 
    // секция вставки INSERT с именем блока содержащим графические примитивы
    class DxfBlocks
    {
        #region Дсступные только для чтения свойства

        public List<Block> Blocks { get; private set; }     // список найденых блоков

        #endregion
        #region Конструктор

        public DxfBlocks()
        {
            Blocks = new List<Block>();
        }

        #endregion
        #region Открытые методы

        // Создание секции BLOCKS из пар кодов
        public static DxfBlocks Create(List<DxfCodePair> pairs)
        {
            DxfBlocks blocks = new DxfBlocks();
            int index = pairs.FindIndex(i => i.Code == 0 && i.Value == "BLOCK");
            if (index == -1) return blocks;

            for (int i = index; i < pairs.Count; i++)
            {
                if (pairs[i].Code == 0 && pairs[i].Value == "BLOCK")
                {
                    index = pairs.FindIndex(i, j => j.Code == 0 && j.Value == "ENDBLK");
                    blocks.Blocks.Add(Block.Create(pairs.GetRange(i, (index + 1) - i)));
                }
                index = pairs.FindIndex(index, p => p.Code == 0 && p.Value == "BLOCK");
                i = index > 0 ? index - 1 : pairs.Count - 1;
            }
            return blocks;
        }

        // Формирование строки для вывода в файл DXF
        public override string ToString()
        {
            // т.к. секция блоков не требуется, то выводится пустая секция BLOCKS
            return "0\nSECTION\n2\nBLOCKS\n0\nENDSEC\n";
        } 

        #endregion
    }
}
