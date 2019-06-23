using System.Collections;
using System.Collections.Generic;

namespace DxfReader
{
    // Нумератор колекции узлов сплайна
    class KnotEnumerator : IEnumerator<DxfKnot>
    {
        DxfKnots kCollettion;   // колекция узлов
        int curIndex;           // текущий индекс
        DxfKnot curKnot;        // текущий узел

        // Конструктор в качестве параметра принимает класс колекции узлов
        public KnotEnumerator(DxfKnots collection)
        {
            kCollettion = collection;
            curIndex = -1;
            curKnot = default(DxfKnot);
        }

        // Возвращает текущий узел
        public DxfKnot Current => curKnot;

        object IEnumerator.Current => Current;

        public void Dispose() { }

        // Перемещает нумератор на следующий элемент
        public bool MoveNext()
        {
            if (++curIndex >= kCollettion.Count)
                return false;
            curKnot = kCollettion[curIndex];
            return true;
        }

        // Устанавливает нумератор в начало колекции
        public void Reset()
        {
            curIndex = -1;
        }
    }
}
