using System;
using System.Globalization;
using System.Windows.Data;

namespace DxfOpener
{
    [ValueConversion(typeof(int), typeof(string))]
    class LineCountToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((int)value <= 0)
            { return Properties.Resources.docNotCreated; }
            else
                return string.Format(Properties.Resources.docCreated, ((int)value).ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
