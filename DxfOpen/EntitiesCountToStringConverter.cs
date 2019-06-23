using System;
using System.Globalization;
using System.Windows.Data;

namespace DxfOpener
{
    [ValueConversion(typeof(int), typeof(string))]
    class EntitiesCountToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((int)value == 0)
                return Properties.Resources.NoObjects;
            else
                return Properties.Resources.objectsCount + " " + ((int)value).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
