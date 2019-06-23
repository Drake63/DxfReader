using System.Collections;
using System.Collections.Generic;
using System.Windows;

namespace DxfReader
{
    // Коллекция управляющий точек сплайна
    class DxfFitPoints : ICollection<DxfFitPoint>, IDxfOffset
    {
        List<DxfFitPoint> points;           // внутренния коллекция
        public int Count => points.Count;   // количество точек в коллекции

        public bool IsReadOnly => true;     // коллекция только для чтения

        // Конструктор класса
        public DxfFitPoints()
        {
            points = new List<DxfFitPoint>();
        }

        // Индексатор
        public DxfFitPoint this[int index]
        {
            get => points[index];
            set => points[index] = value;
        }
        // Добавление точки в коллекцию
        public void Add(DxfFitPoint item)
        {
            points.Add(item);
        }
        // Добавление массива точек в коллекцию
        public void AddRange(IEnumerable<DxfFitPoint> items)
        {
            points.AddRange(items);
        }
        // Очистка коллекции
        public void Clear()
        {
            points.Clear();
        }
        // Проверка есть ли заданный элемент в коллекции
        public bool Contains(DxfFitPoint item)
        {
            foreach (DxfFitPoint point in points)
                if (point.Equals(item))
                    return true;
            return false;
        }
        // Копировать коллекцию в целевой массив начиная с заданного индекса в нем
        public void CopyTo(DxfFitPoint[] array, int arrayIndex)
        {
            points.CopyTo(array, arrayIndex);
        }
        // Вернуть нумератор колекции
        public IEnumerator<DxfFitPoint> GetEnumerator()
        {
            return new FitPointEnumerator(this);
        }
        // Удалить заданный элемент
        public bool Remove(DxfFitPoint item)
        {
            for(int i=0; i<points.Count;i++)
                if(points[i].Equals(item))
                {
                    points.RemoveAt(i);
                    return true;
                }
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new FitPointEnumerator(this);
        }

        // Вывод строки для записи в файл DXF
        public override string ToString()
        {
            string str = "";
            foreach (DxfFitPoint point in points)
                str += point.ToString();
            return str;
        }
        // Вывод строки для записи в файл в формате 2D
        public string ToStringOf2d()
        {
            string str = "";
            foreach (DxfFitPoint point in points)
                str += point.ToStringOf2d();
            return str;
        }

        // Смещение управляющих точек на заданный вектор
        public void OffSet(Vector vector)
        {
            foreach (DxfFitPoint point in points)
                point.OffSet(vector);
        }

        // Вывод строки для записи в файл версии R13
        public string ToStringOfVerR13()
        {
            return ToString();
        }
    }
}
