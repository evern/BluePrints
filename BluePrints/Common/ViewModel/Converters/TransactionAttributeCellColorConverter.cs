using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace BluePrints.Common.ViewModel.Converters
{
    public class TransactionAttributeCellColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush transparentColor = new SolidColorBrush(Colors.Transparent);

            if (value == DependencyProperty.UnsetValue || value == null)
                return transparentColor;

            SolidColorBrush valuePendingColor = new SolidColorBrush(Colors.LemonChiffon);
            SolidColorBrush valueApprovedColor = new SolidColorBrush(Colors.Chartreuse);
            TransactionAttributeStatus status = (TransactionAttributeStatus)value;
            if (status == TransactionAttributeStatus.Original)
                return transparentColor;
            else if (status == TransactionAttributeStatus.Pending)
                return valuePendingColor;
            else
                return valueApprovedColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}