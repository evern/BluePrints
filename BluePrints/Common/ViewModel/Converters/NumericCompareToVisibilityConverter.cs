using System;
using System.Windows;
using System.Windows.Data;

namespace BluePrints.Common.ViewModel.Converters
{
    [ValueConversion(typeof(decimal), typeof(decimal), ParameterType = typeof(bool))]
    public class NumericCompareToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            var invert = bool.Parse(parameter.ToString());
            Visibility valueMatchedVisibility;
            Visibility valueUnmatchedVisibility;
            if (invert)
            {
                valueMatchedVisibility = Visibility.Hidden;
                valueUnmatchedVisibility = Visibility.Visible;
            }
            else
            {
                valueMatchedVisibility = Visibility.Visible;
                valueUnmatchedVisibility = Visibility.Hidden;
            }

            if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue)
                return valueUnmatchedVisibility;

            if (values[0] == null || values[1] == null)
                return valueUnmatchedVisibility;

            try
            {
                var Value1 = (decimal) values[0];
                var Value2 = (decimal) values[1];

                if (Value1 == Value2)
                    return valueMatchedVisibility;
                else
                    return valueUnmatchedVisibility;
            }
            catch
            {
                return valueUnmatchedVisibility;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}