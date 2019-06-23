using System.Windows.Media;
using System.Windows;

namespace DxfReader
{
    // Интерфейс, который должны реализовать все графические объекты
    interface IDxfEntity : IDxfOffset
    {
        string Name { get; }                        // название объекта
        DxfColors Color { get; }                    // цвет линий
        Rect Bound { get; }
        DrawingVisual CreateVisual(double scale);   // создание визуального представления
    }
}
