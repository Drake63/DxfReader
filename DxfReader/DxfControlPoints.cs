using System.Collections;
using System.Collections.Generic;
using System.Windows;

namespace DxfReader
{
    // Коллекция контрольных точек
    class DxfControlPoints : ICollection<DxfControlPoint>, IDxfOffset
    {
        private List<DxfControlPoint> controlPoints;    // внутренняя коллекция
        private bool isWeight;                          // отличается вес контрольной точки от значения по умолчанию

        // Конструктор класса
        public DxfControlPoints()
        {
            controlPoints = new List<DxfControlPoint>();
            isWeight = false;
        }

        // Флаг, отображающий отличается вес контрольной точки от значения по умолчанию
        public bool IsWeight => isWeight;
        // Количество контрольных точек в коллекции
        public int Count => controlPoints.Count;

        // Доступность коллеции для редактирования
        public bool IsReadOnly => true;

        // Индексатор коллекции
        public DxfControlPoint this[int index]
        {
            get => controlPoints[index];
            set
            {
                controlPoints[index] = value;
                if (value.Weight != 1.0)
                    isWeight = true;
            }
        }

        // Добавляет новый элемент к коллеции
        public void Add(DxfControlPoint item)
        {
            controlPoints.Add(item);
            if (item.Weight != 1.0)
                isWeight = true;
        }

        // Очищает коллекцию
        public void Clear()
        {
            controlPoints.Clear();
        }

        // Ищет, содержит ли коллекция указанный элемент
        public bool Contains(DxfControlPoint item)
        {
            foreach(DxfControlPoint point in controlPoints)
            {
                if (point.Equals(item))
                    return true;
            }
            return false;
        }

        // Копирует коллекцию в заданный массив начиная с указанной позиции
        public void CopyTo(DxfControlPoint[] array, int arrayIndex)
        {
            controlPoints.CopyTo(array, arrayIndex);
        }

        // Возвращает нумератор коллекции
        public IEnumerator<DxfControlPoint> GetEnumerator()
        {
            return new ControlPointEnumerator(this);
        }

        // Удаляет заданный элемент из коллекции если такой найден
        public bool Remove(DxfControlPoint item)
        {
            bool result = false;
            for (int i = 0; i < controlPoints.Count; i++)
            {
                if (controlPoints[i].Equals(item))
                {
                    controlPoints.RemoveAt(i);
                    result = true;
                }
            }
            if (result && isWeight)
                isWeight = controlPoints.Exists(c => c.Weight != 1.0);
            return result;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ControlPointEnumerator(this);
        }

        // Смещает точки в коллекции на заданный вектор
        public void OffSet(Vector vector)
        {
            foreach (DxfControlPoint point in controlPoints)
                point.OffSet(vector);
        }

        // Возращает строку для файла R13
        public string ToStringOfVerR13()
        {
            return ToString();
        }

        // Возвращает строку для файла DXF
        public override string ToString()
        {
            string str = "";
            foreach (DxfControlPoint point in controlPoints)
                str += point.ToString(isWeight);
            return base.ToString();
        }

        // Возвращает строку 2D для файла DXF
        public string ToStringOf2d()
        {
            string str = "";
            foreach(DxfControlPoint point in controlPoints)
                str += point.ToStringOf2d(isWeight);
            return str;
        }
    }

    // Нумератор для коллекции DxfControlPoints
    class ControlPointEnumerator : IEnumerator<DxfControlPoint>
    {
        DxfControlPoints collection;    // коллекция DxfControlPoints
        int curIndex;                   // текущий индекс
        DxfControlPoint curPoint;       // текущая точка

        // Конструктор класса, в качестве параметра принимает коллекцию DxfControlPoints
        public ControlPointEnumerator(DxfControlPoints collection)
        {
            this.collection = collection;
            curIndex = -1;
            curPoint = default(DxfControlPoint);
        }

        // Возвращает текущее значение
        public DxfControlPoint Current => curPoint;

        object IEnumerator.Current => Current;

        public void Dispose() { }

        // Перемещает нумератор на следующую позицию
        public bool MoveNext()
        {
            if (++curIndex >= collection.Count)
                return false;
            else
                curPoint = collection[curIndex];
            return true;
        }

        // Возвращает нумератор в начало
        public void Reset()
        {
            curIndex = -1;
        }
    }
}
