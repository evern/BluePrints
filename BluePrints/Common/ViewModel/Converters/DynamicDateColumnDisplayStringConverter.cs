using BluePrints.Common.Projections;
using System;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace BluePrints.Common.ViewModel.Converters
{
    public class DynamicDateColumnDisplayStringConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                bool isWeeks = false;

                if (values[0] == null)
                    return string.Empty;

                if (values[0] == DependencyProperty.UnsetValue)
                    return string.Empty;

                if (values[1] != null && values[1] != DependencyProperty.UnsetValue && (bool)values[1])
                    isWeeks = true;

                DateTime dateValue;
                if (DateTime.TryParse(values[0].ToString(), out dateValue))
                {
                    if (isWeeks)
                        return dateValue.ToString("dd-MMM-yy");
                    else
                        return dateValue.ToString("MMM-yy");
                }
                else
                    return values[0].ToString();
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
            }

            return values[0].ToString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}