using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;

namespace DxfReader
{
    // Коллекция вершин полилинии
    class DxfVertices : ICollection<DxfVertex>, IDxfOffset
    {
        List<DxfVertex> vertices;       // внутренния коллекция вершин

        // Конструктор класса
        public DxfVertices()
        {
            vertices = new List<DxfVertex>();
        }

        // возвращает количество вершин в коллекции
        public int Count => vertices.Count;

        // коллекция только для чтения
        public bool IsReadOnly => true;

        // Итератор
        public DxfVertex this[int index]
        {
            get
            {
                if (index >= vertices.Count)
                    throw new ArgumentOutOfRangeException("index");
                return vertices[index];
            }
            set
            {
                if (index >= vertices.Count)
                    throw new ArgumentOutOfRangeException("index");
                vertices.RemoveAt(index);
                vertices.Insert(index, value);
            }
        }

        // добавляет новую вершину в коллекцию
        public void Add(DxfVertex item)
        {
            vertices.Add(item);
        }

        // добавляет коллекцию вершин
        public void AddRange(IEnumerable<DxfVertex> collection)
        {
            vertices.AddRange(collection);
        }

        // Очищает коллекцию
        public void Clear()
        {
            vertices.Clear();
        }

        // Ищет заданный объект в коллекции
        public bool Contains(DxfVertex item)
        {
            if (item == null) return false;
            foreach (DxfVertex vert in vertices)
                if (vert.Equals(item))
                    return true;
            return false;
        }

        // Копирует коллекцию в заданный массив, в указанную позицию
        public void CopyTo(DxfVertex[] array, int arrayIndex)
        {
            vertices.CopyTo(array, arrayIndex);
        }

        // Возвращает нумератор коллекции
        public IEnumerator<DxfVertex> GetEnumerator()
        {
            return new VertexEnumerator(this);
        }

        // Удаляет заданную вершину из коллекции, если она есть
        public bool Remove(DxfVertex item)
        {
            for (int i = 0; i < vertices.Count; i++)
                if (vertices[i].Equals(item))
                {
                    vertices.RemoveAt(i);
                    return true;
                }
            return false;
        }

        // Удаляет из коллекции узел с указанным индексом
        public void RemoveAt(int index)
        {
            vertices.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new VertexEnumerator(this);
        }

        // Смещает коллекцию вершин на заданный вектор
        public void OffSet(Vector vector)
        {
            foreach (DxfVertex vert in vertices)
                vert.OffSet(vector);
        }

        // Возвращает строку для записи в файл DXF версии R13
        public string ToStringOfVerR13()
        {
            string str = "";
            foreach (DxfVertex vert in vertices)
                str += vert.ToStringOfVerR13();
            return str;
        }

        // Возвращает строку для записи в файл DXF версии R11
        public override string ToString()
        {
            string str = "";
            foreach (DxfVertex vert in vertices)
                str += vert.ToString();
            return str;
        }

        // Возвращает строку для записи в файл вершин как 2D
        public string ToStringOf2d()
        {
            string str = "";
            foreach (DxfVertex vert in vertices)
                str += vert.ToStringOf2d();
            return str;
        }

        // Возвращает индекс вершины в коллекци использую функцию поиска
        public int FindIndex(Predicate<DxfVertex> func)
        {
            return vertices.FindIndex(func);
        }

        // Ищет вершину в коллекции начиная с заданного индекса используя функцию поиска
        public int FindIndex(int index, Predicate<DxfVertex> func)
        {
            return vertices.FindIndex(index, func);
        }

        // Возвращает коллекцию вершин начиная с указанного индекса и указанного размера
        public IEnumerable<DxfVertex> GetRange(int index, int count)
        {
            return vertices.GetRange(index, count);
        }
    }
}
