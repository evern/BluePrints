using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace BluePrints.Common.ViewModel.Converters
{
    [ValueConversion(typeof(decimal), typeof(decimal), ParameterType = typeof(bool))]
    public class NumericCompareToBooleanConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            var invert = bool.Parse(parameter.ToString());
            bool valueMatchedBoolean;
            bool valueUnmatchedBoolean;
            if (invert)
            {
                valueMatchedBoolean = true;
                valueUnmatchedBoolean = false;
            }
            else
            {
                valueMatchedBoolean = false;
                valueUnmatchedBoolean = true;
            }

            if (values[0] == DependencyProperty.UnsetValue && values[1] == DependencyProperty.UnsetValue)
                return valueUnmatchedBoolean;

            try
            {
                var Value1 = (decimal) values[0];
                var Value2 = (decimal) values[1];

                if (Value1 == Value2)
                    return valueMatchedBoolean;
                else
                    return valueUnmatchedBoolean;
            }
            catch
            {
                return valueUnmatchedBoolean;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}