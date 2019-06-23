using System;
using System.Collections;
using System.Collections.Generic;

namespace DxfReader
{
    // Колекция узлов сплайна
    class DxfKnots : ICollection<DxfKnot>
    {
        List<DxfKnot> knots;    // внутренняя колекция
        // возвращает количество узлов в колекции
        public int Count => knots.Count;
        // колекция является доступной только для чтения
        public bool IsReadOnly => true;

        // Конструктор класса
        public DxfKnots()
        {
            knots = new List<DxfKnot>();
        }

        // Индексатор
        public DxfKnot this[int index]
        {
            get
            {
                if (index >= knots.Count)
                    throw new ArgumentOutOfRangeException("index");
                return knots[index];
            }
            set
            {
                if (index >= knots.Count)
                    throw new ArgumentOutOfRangeException("index");
                knots.RemoveAt(index);
                if (index == knots.Count)
                    knots.Add(value);
                else
                    knots.Insert(index, value);
            }
        }

        // Добавление узла в колекцию
        public void Add(DxfKnot item)
        {
            knots.Add(item);
        }

        // Добавление колекции узлов
        public void AddRange(IEnumerable<DxfKnot> knots)
        {
            this.knots.AddRange(knots);
        }

        // Очистить колекции
        public void Clear()
        {
            knots.Clear();
        }

        // Определить содержит ли колекция заданный элемент
        public bool Contains(DxfKnot item)
        {
            foreach (DxfKnot knot in knots)
                if (knot.Equals(item))
                    return true;
            return false;
        }

        // Копировать колекцию с в заданный массив с указанного индекса в нем
        public void CopyTo(DxfKnot[] array, int arrayIndex)
        {
            knots.CopyTo(array, arrayIndex);
        }

        // возвращает нумератор колекции
        public IEnumerator<DxfKnot> GetEnumerator()
        {
            return new KnotEnumerator(this);
        }

        // Удалить заданный узел, если он есть
        public bool Remove(DxfKnot item)
        {
            for(int i=0;i<knots.Count;i++)
                if(knots[i].Equals(item))
                {
                    knots.RemoveAt(i);
                    return true;
                }
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new KnotEnumerator(this);
        }

        // Возвращает сторку для записи в файл DXF
        public override string ToString()
        {
            string str = "";
            foreach (DxfKnot knot in knots)
                str += knot.ToString();
            return str;
        }
    }
}
