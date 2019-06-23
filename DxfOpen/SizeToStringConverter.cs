using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace DxfOpener
{
    [ValueConversion(typeof(Size), typeof(string))]
    class SizeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Format(Properties.Resources.sizeToString,
                ((Size)value).Width.ToString("N2"), ((Size)value).Height.ToString("N2"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
