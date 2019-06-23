using System.Collections;
using System.Collections.Generic;

namespace DxfReader
{
    // Нумератор коллекции DxfFitPoints
    class FitPointEnumerator : IEnumerator<DxfFitPoint>
    {
        DxfFitPoints dxfFitPoints;          // колекция точек
        int curIndex;                       // текущий индекс
        DxfFitPoint curFitPoint;            // текущая точка

        // Конструктор класса в качестве параметра принимает коллекцию DxfFitPoints
        public FitPointEnumerator(DxfFitPoints collection)
        {
            dxfFitPoints = collection;
            curIndex = -1;
            curFitPoint = default(DxfFitPoint);
        }

        // Возвращает текущую точку
        public DxfFitPoint Current => curFitPoint;

        object IEnumerator.Current => Current;

        public void Dispose() { }

        // Перемещает перечислитель к следующему элементу коллекции
        public bool MoveNext()
        {
            if (++curIndex >= dxfFitPoints.Count)
                return false;
            curFitPoint = dxfFitPoints[curIndex];
            return true;
        }

        // Устанавливает перечислитель в начало
        public void Reset()
        {
            curIndex = -1;
        }
    }
}
