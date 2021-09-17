using BluePrints.Common.Resources;
using System;
using System.Windows;
using System.Windows.Data;

namespace BluePrints.Common.ViewModel.Converters
{
    public class MaxPercentageMessageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == DependencyProperty.UnsetValue || value == null)
                return false;

            bool isMaxIsMaxPercentageRestrictedByFuturePercentage = (bool)value;
            if (isMaxIsMaxPercentageRestrictedByFuturePercentage)
                return "Max % is 100% and is restricted by earned units in the future to avoid going beyond 100%, please refer to " + NavigationResources.Menu_Project_DesignProgressDistribution_Title + " for more info";
            else
                return "Max % is restricted by gate's max %";
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}