using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;


namespace WpfCtrlLib
{
    /// <summary>
    /// Элемент управления NumericUpDown
    /// </summary>
    [ContentProperty("Value")]
    [TemplatePart(Name = ("PART_TextBox"), Type = (typeof(TextBox)))]
    [TemplatePart(Name =("PART_UpButton"), Type =(typeof(RepeatButton)))]
    [TemplatePart(Name = ("PART_DownButton"), Type = (typeof(RepeatButton)))]
    public class NumericUpDown : Control
    {
        private static readonly RoutedUICommand upCommand;
        private static readonly RoutedUICommand downCommand;

        /// <summary>
        /// Свойство зависимости максимальное допустимое значени
        /// </summary>
        public static readonly DependencyProperty MaxValueProperty;
        /// <summary>
        /// Свойство зависимости минимальное допустимое значение
        /// </summary>
        public static readonly DependencyProperty MinValueProperty;
        /// <summary>
        /// Свойство зависимости - значение на которое изменяется значение
        /// </summary>
        public static readonly DependencyProperty IncrementProperty;
        /// <summary>
        /// Свойство зависимости - текущее значение элемента
        /// </summary>
        public static readonly DependencyProperty ValueProperty;
        /// <summary>
        /// Маршрутизируемое событие, возникающее при изменении текущего значения
        /// </summary>
        public static readonly RoutedEvent ValueChangedEvent;

        /// <summary>
        /// Максимум до которого можно увеличивать текущее значение
        /// </summary>
        public int MaxValue
        {
            get => (int)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        /// <summary>
        /// Минимум ниже которого нельзя уменьшить текущее значение
        /// </summary>
        public int MinValue
        {
            get => (int)GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }

        /// <summary>
        /// Шаг с которым изменяется текущее значение за один раз
        /// </summary>
        public int Increment
        {
            get => (int)GetValue(IncrementProperty);
            set => SetValue(IncrementProperty, value);
        }

        /// <summary>
        /// Текущее значение элемента NumericUpDown
        /// </summary>
        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        /// <summary>
        /// Маршрутизируемое событие, возникающее при изменение текущего значения
        /// </summary>
        public event ValueChangedEventHandler ValueChanged
        {
            add => AddHandler(ValueChangedEvent, value);
            remove => RemoveHandler(ValueChangedEvent, value);
        }

        /// <summary>
        /// Команда для увеличения текущего значения
        /// </summary>
        public static RoutedUICommand UpCommand => upCommand;
        /// <summary>
        /// Команда для уменьшения текущего значения
        /// </summary>
        public static RoutedUICommand DownCommand => downCommand;

        static NumericUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(typeof(NumericUpDown)));

            MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(int), typeof(NumericUpDown),
                new FrameworkPropertyMetadata(100, new PropertyChangedCallback(MaxValueChangedCallback), new CoerceValueCallback(CoerceMaxValueCallback)));
            MinValueProperty = DependencyProperty.Register("MinValue", typeof(int), typeof(NumericUpDown),
                new FrameworkPropertyMetadata(0, new PropertyChangedCallback(MinValueChangedCallback), new CoerceValueCallback(CoerceMinValueCallback)));
            IncrementProperty = DependencyProperty.Register("Increment", typeof(int), typeof(NumericUpDown),
                new FrameworkPropertyMetadata(1, null, new CoerceValueCallback(CoercyIncrementCallback)));
            ValueProperty = DependencyProperty.Register("Value", typeof(int), typeof(NumericUpDown),
                new FrameworkPropertyMetadata(0, new PropertyChangedCallback(ValueChangedCallback), new CoerceValueCallback(CoerceValueChangedCallback)));
            ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, 
                typeof(ValueChangedEventHandler), typeof(NumericUpDown));

            upCommand = new RoutedUICommand("Up", "UpCommand", typeof(NumericUpDown), 
                new InputGestureCollection(new List<InputGesture> { new KeyGesture(Key.Up) }));
            downCommand = new RoutedUICommand("Down", "DownCommand", typeof(NumericUpDown),
                new InputGestureCollection(new List<InputGesture> { new KeyGesture(Key.Down) }));
            CommandManager.RegisterClassCommandBinding(typeof(NumericUpDown),
                new CommandBinding(upCommand, UpCommand_Executed, UpCommand_canExecute));
            CommandManager.RegisterClassCommandBinding(typeof(NumericUpDown),
                new CommandBinding(downCommand, DownCommand_Executed, DownCommand_canExecute));
        }

        private static void DownCommand_canExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            int value = (sender as NumericUpDown).Value;
            e.CanExecute = value > (sender as NumericUpDown).MinValue;
        }

        private static void DownCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NumericUpDown num = sender as NumericUpDown;
            if (num.Value <= num.MinValue)
                return;
            num.Value -= num.Increment;
        }

        private static void UpCommand_canExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            int value = (sender as NumericUpDown).Value;
            e.CanExecute = value < (sender as NumericUpDown).MaxValue;
        }

        private static void UpCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NumericUpDown num = sender as NumericUpDown;
            num.Value += num.Increment;
        }

        private static object CoerceValueChangedCallback(DependencyObject d, object baseValue)
        {
            NumericUpDown num = d as NumericUpDown;
            if ((int)baseValue < num.MinValue)
                return num.MinValue;
            else if ((int)baseValue > num.MaxValue)
                return num.MaxValue;
            return (int)baseValue;
        }

        private static void ValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDown num = d as NumericUpDown;

            ValueChangedEventArgs args = new ValueChangedEventArgs((int)e.OldValue, (int)e.NewValue, ValueChangedEvent);
            num.RaiseEvent(args);
        }

        private static object CoercyIncrementCallback(DependencyObject d, object baseValue)
        {
            NumericUpDown num = d as NumericUpDown;
            int value = num.MaxValue - num.MinValue;
            if ((int)baseValue > value)
                return value;
            return (int)baseValue;
        }

        private static object CoerceMinValueCallback(DependencyObject d, object baseValue)
        {
            int maxValue = (d as NumericUpDown).MaxValue;
            if ((int)baseValue > maxValue)
                return maxValue;
            return (int)baseValue;
        }

        private static void MinValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDown num = d as NumericUpDown;
            num.CoerceValue(MaxValueProperty);
            num.CoerceValue(ValueProperty);
        }

        private static object CoerceMaxValueCallback(DependencyObject d, object baseValue)
        {
            int minValue = (d as NumericUpDown).MinValue;

            if ((int)baseValue < minValue)
                return minValue;
            return (int)baseValue;
        }

        private static void MaxValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDown num = d as NumericUpDown;
            num.CoerceValue(MinValueProperty);
            num.CoerceValue(ValueProperty);
        }

        /// <summary>
        /// Выполняет построение визуального дерева
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (base.GetTemplateChild("PART_TextBox") is TextBox textBox)
            {
                textBox.TextChanged += new TextChangedEventHandler(OnTextChanged);
                textBox.MouseWheel += new MouseWheelEventHandler(OnMouseWeel);
            }
        }

        private void OnMouseWeel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
                Value += Increment;
            else
                Value -= Increment;
        }

        /// <summary>
        /// Предаватительная проверка вводимого текста
        /// </summary>
        /// <param name="e">Параметр содержащий аргументы, связанные с изменения в TextComposition</param>
        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            if (e.Text != "-" && !Char.IsDigit(e.Text, e.Text.Length - 1))
                e.Handled = true;
            else
                e.Handled = false;
        }

        /// <summary>
        /// Проверка нажатой клавиши на клавиатуре
        /// </summary>
        /// <param name="e">Параметр предаставляющий данные для перенаправленных событий UIElenment.Key.Up и UIElenment.Key.Down</param>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                e.Handled = true;
            if (e.Key == Key.Up || e.Key == Key.Down)
                Focus();
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox box = sender as TextBox;
            if (box.Text == "-" && MinValue < 0) return;

            if (int.TryParse(box.Text, out int value))
            {
                Value = value >= MaxValue ? MaxValue : value;
                Value = value <= MinValue ? MinValue : value;
            }

            box.Text = Value.ToString();
        }
    }
}
