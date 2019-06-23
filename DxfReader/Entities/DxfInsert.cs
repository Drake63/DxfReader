using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace DxfReader
{
    // Секция INSERT
    class DxfInsert : IDxfEntity
    {
        // имя секции должно быть всегда постоянным
        const string name = EntitiesVariableCode.Insert;

        bool ignoreLineType = false;

        #region Открытые только для чтения свойства

        public string Name { get { return name; } }             // название секции
        public DxfColors Color { get; private set; }            // цвет линии, в этом классе он не устанавливается
        public string BlockName { get; private set; }           // имя блока в секции BLOCKS на который ссылается данный объект
        public Rect Bound { get; }

        #endregion
        #region Конструктор, нельзя создать класс непосредсвенно его вызовом

        private DxfInsert() { }

        #endregion
        #region Открытые методы класса

        // Создание класса из пары кодов, требуется только пара, которая содежит имя блока
        public static DxfInsert Create(DxfCodePair pair, bool ignoreLineType)
        {
            if (pair.Code != 2)
                throw new ArgumentException(Properties.Resource.ParamCode2 + " " + pair.Code);
            DxfInsert insert = new DxfInsert();
            insert.BlockName = pair.Value;
            insert.ignoreLineType = ignoreLineType;
            return insert;
        }

        // этот класс ни чего не визиализирует
        public DrawingVisual CreateVisual(double scale)
        {
            return null;
        }

        // этот класс ни чего не смещает
        public void OffSet(Vector vector)
        {
            // здесь ни чего делать не нужно!!!!
        }

        public string ToStringOfVerR13()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
