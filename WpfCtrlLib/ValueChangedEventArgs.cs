using System.Windows;

namespace WpfCtrlLib
{
    /// <summary>
    /// Делегат функции обработки маршрутизируемого события, возникающего при изменении значения свойства Value
    /// </summary>
    /// <param name="sender">Объект в иницировавший событие</param>
    /// <param name="e">Объект содержащий информацию о состоянии и данные, связанные с событием</param>
    public delegate void ValueChangedEventHandler(object sender, ValueChangedEventArgs e);

    /// <summary>
    /// Класс содержащий информацию и данные, связанные с маршрутизируемым событием ValueChangedEventHandler
    /// </summary>
    public class ValueChangedEventArgs : RoutedEventArgs
    {
        private readonly int oldValue;
        private readonly int newValue;
         /// <summary>
         /// Предыдущее значение свойства Value
         /// </summary>
        public int OldValue => oldValue;
        /// <summary>
        /// Новое значение свойства Value
        /// </summary>
        public int NewValie => newValue;

        /// <summary>
        /// Конструктор экземпляра класса
        /// </summary>
        /// <param name="oldValue">Старое значение свойства Value</param>
        /// <param name="newValue">Новое значение свойства Value</param>
        /// <param name="id">Идентификатор маршрутизируемого события</param>
        public ValueChangedEventArgs(int oldValue, int newValue, RoutedEvent id)
        {
            this.oldValue = oldValue;
            this.newValue = newValue;
            RoutedEvent = id;
        }
    }
}
