using System.Collections;
using System.Collections.Generic;

namespace DxfReader
{
    // Нумератор класса Vertex
    class VertexEnumerator : IEnumerator<DxfVertex>
    {
        DxfVertices vertices;       // коллекция вершин
        int curIndex;               // текущий индекс
        DxfVertex curVertex;        // текущая вершина

        // Конструктор класса
        public VertexEnumerator(DxfVertices collection)
        {
            vertices = collection;
            curIndex = -1;
            curVertex = default(DxfVertex);
        }

        // Возвращает текущую вершину
        public DxfVertex Current => curVertex;

        object IEnumerator.Current => Current;

        public void Dispose() { }

        // Перемещает перечислитель к следующему элементу
        public bool MoveNext()
        {
            if (++curIndex >= vertices.Count)
                return false;
            curVertex = vertices[curIndex];
            return true;
        }

        // Устанавливает перечислитель в его начальное положение
        public void Reset()
        {
            curIndex = -1;
        }
    }
}
