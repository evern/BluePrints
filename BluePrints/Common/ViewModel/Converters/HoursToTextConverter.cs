using System;
using System.Windows;
using System.Windows.Data;

namespace BluePrints.Common.ViewModel.Converters
{
    public class HoursToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == DependencyProperty.UnsetValue || value == null)
                return "0 min";

            decimal hours = (decimal)value;
            if (hours > 24)
                return (hours / 24).ToString("#") + " day(s)";
            else if (hours > 1)
                return hours.ToString("#") + " hour(s)";
            else if (hours > 0)
                return (hours * 60).ToString("#") + " min(s)";
            else
                return "0 min";
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}