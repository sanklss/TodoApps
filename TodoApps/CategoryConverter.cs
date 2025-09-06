using System;
using System.Globalization;
using System.Windows.Data;

namespace TodoApps
{
    public class CategoryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;
            if (string.IsNullOrEmpty(str)) return "Без категории";
            var parts = str.Split('|');
            return parts.Length > 1 ? parts[1] : parts[0];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}