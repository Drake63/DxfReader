using System;

namespace DxfReader
{
    /// <summary>
    /// Ошибка возникающая при неудачной попытке поиска в коллекции Entities
    /// </summary>
    public class EntitiesNoFoundException : Exception
    {
        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="message">Сообщение об ошибке</param>
        public EntitiesNoFoundException(string message)
            : base(message) { }
    }
}
