using System;
using System.Windows;
using System.Windows.Data;

namespace BluePrints.Common.ViewModel.Converters
{
    public class WeeksToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == DependencyProperty.UnsetValue || value == null)
                return "";

            decimal weeks = (decimal)value;
            if (weeks > 0)
                return weeks.ToString("#.##") + " weeks";
            else
                return "0 week";
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}