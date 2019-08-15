using BluePrints.Common.Projections;
using System;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace BluePrints.Common.ViewModel.Converters
{
    public class DynamicDateColumnDisplayStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null)
                    return string.Empty;

                if (value == DependencyProperty.UnsetValue)
                    return string.Empty;

                DateTime dateValue;
                if (DateTime.TryParse(value.ToString(), out dateValue))
                {
                    return dateValue.ToString("MMM-yy");
                }
                else
                    return value.ToString();
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
            }

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}