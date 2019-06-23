namespace DxfReader
{
    /// <summary>
    /// Перечисление кодов цветов для AJANCAM
    /// </summary>
    public enum DxfColors
    {
        /// <summary>
        /// Основной контур
        /// </summary>
        MainOutline = 93,
        /// <summary>
        /// Незамкнутый контур
        /// </summary>
        NotClosedLine = 4,
        /// <summary>
        /// Разметка
        /// </summary>
        LineOfMarking = 1,
        /// <summary>
        /// Все остальные типы
        /// </summary>
        NoColor = -1
    }
}