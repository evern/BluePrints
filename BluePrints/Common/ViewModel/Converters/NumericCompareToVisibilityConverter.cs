using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace BluePrints.Common.ViewModel.Converters
{
    [ValueConversion(typeof(decimal), typeof(decimal), ParameterType=typeof(bool))]
    public class NumericCompareToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool invert = bool.Parse(parameter.ToString());
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

            if (values[0] == DependencyProperty.UnsetValue && values[1] == DependencyProperty.UnsetValue)
            {
                return valueUnmatchedVisibility;
            }

            try
            {
                decimal Value1 = (decimal)values[0];
                decimal Value2 = (decimal)values[1];

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

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
