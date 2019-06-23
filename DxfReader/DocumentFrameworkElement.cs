using System;
using System.Windows;
using System.Windows.Media;

namespace DxfReader
{
    /// <summary>
    /// Элемент который пердставляет визуальное представление содержимого документа DXF
    /// </summary>
    public class DocumentFrameworkElement : FrameworkElement
    {
        VisualCollection theVisuals;

        /// <summary>
        /// Коллекция визуальных объектов
        /// </summary>
        public DrawingVisual Visual
        {
            get { return theVisuals[0] as DrawingVisual; }
            set
            {
                theVisuals.Clear();
                theVisuals.Add(value);
            }
        }

        /// <summary>
        /// Количество объектов Visual в коллекции
        /// </summary>
        protected override int VisualChildrenCount
        {
            get
            {
                return theVisuals.Count;
            }
        }

        /// <summary>
        /// Ширина объекта Visual
        /// </summary>
        public double WidthVisual { get { return (theVisuals[0] as DrawingVisual).DescendantBounds.Width; } }
        /// <summary>
        /// Высота объекта Visual
        /// </summary>
        public double HeightVisual { get { return (theVisuals[0] as DrawingVisual).DescendantBounds.Height; } }
        /// <summary>
        /// Количество визуальных объектов в коллекции
        /// </summary>
        public int VisualsCount { get { return theVisuals.Count; } }
        /// <summary>
        /// Размер элемента
        /// </summary>
        public Size Size
        {
            get
            {
                if (Width == double.NaN || Height == double.NaN)
                    throw new ArgumentException(Properties.Resource.NoNumber);

                return new Size(Width, Height);
            }
        }

        /// <summary>
        /// Возвращает объект Visual из коллекции по указаному индексу
        /// </summary>
        /// <param name="index">Индекс объекта в коллекции</param>
        /// <returns></returns>
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= theVisuals.Count)
                throw new ArgumentOutOfRangeException();
            return theVisuals[index];
        }
        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public DocumentFrameworkElement()
        {
            theVisuals = new VisualCollection(this);
        }
        
        /// <summary>
        /// Добавление объекта Visual в коллекцию
        /// </summary>
        /// <param name="visual">Обект Visual</param>
        public void AddVisual(DrawingVisual visual)
        {
            theVisuals.Add(visual);
        }

        /// <summary>
        /// Добавление коллекции объектов Visual
        /// </summary>
        /// <param name="visuals">Коллекция объектов Visual</param>
        public void AddRangeVisuals(DrawingVisual[] visuals)
        {
            foreach (DrawingVisual visual in visuals)
                theVisuals.Add(visual);
        }

        /// <summary>
        /// Удаление всех объектов Visual
        /// </summary>
        public void Clear()
        {
            theVisuals.Clear();
        }
    }
}
