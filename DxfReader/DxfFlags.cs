using System;

namespace DxfReader
{
    /// <summary>
    /// Флаги для создания документа
    /// </summary>
    [Flags]
    public enum DxfFlags
    {
        /// <summary>
        /// Преобразовывать Polyline в LwPolyline
        /// </summary>
        CreateLwPolyline = 0x01,
        /// <summary>
        /// Создавать дуги из объектов Vertex, если в нем установлен флаг искревленности
        /// </summary>
        CreateArc=0x02,
        /// <summary>
        /// Перемещать ли объекты в начало координат
        /// </summary>
        MoveToZero = 0x04,
        /// <summary>
        /// Создавать объект Polyline из элиптической дуги
        /// </summary>
        PolylineFromEllipse = 0x08,
        /// <summary>
        /// Все линии, отличные от разметки, незамкнутого контура и основной линии, 
        /// игнорировать, если нет - преобразовывать в основной контур
        /// </summary>
        IgnoreOtherLineType=0x10,
        /// <summary>
        /// Создавать документ R13 вместо документа по умолчанию R11
        /// </summary>
        CreateR13 = 0x20,
        /// <summary>
        /// Записывать в строку только объекты Entities
        /// </summary>
        OnlyEntities = 0x40,
        /// <summary>
        /// Имеются ли в документе сплайны
        /// </summary>
        ContainsSplines = 0x80,
        /// <summary>
        /// Изменился ли документ
        /// </summary>
        Changed = 0x100
    }
}
