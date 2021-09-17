using BluePrints.Common.Resources;
using System;
using System.Windows;
using System.Windows.Data;

namespace BluePrints.Common.ViewModel.Converters
{
    public class ValueOutOfRangeMessageConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            string errorMessage = "Value is out of range";

            if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue || values[2] == DependencyProperty.UnsetValue || values[3] == DependencyProperty.UnsetValue)
                return errorMessage;

            if (values[0] == null || values[1] == null || values[2] == null || values[3] == null)
                return errorMessage;

            try
            {
                var maxValue = (decimal)values[0];
                var minValue = (decimal)values[1];
                var currentValue = (decimal)values[2];

                //round current value because view has P0 mask
                currentValue = Math.Round(currentValue);
                bool IsMaxPercentageRestrictedByFuturePercentage = (bool)values[3];

                if (currentValue > maxValue)
                {
                    if(currentValue > 1)
                        return "Max % is 100%";
                    else if(IsMaxPercentageRestrictedByFuturePercentage)
                        return "Total % exceeds 100%, please check % on future data dates in Design -> " + NavigationResources.Menu_Project_DesignProgressDistribution_Title;
                    else
                        return "Total % exceeds gate max %";
                }
                else if (currentValue < minValue)
                    return "Higher % detected on previous data date, please check % on previous data dates in Design -> " + NavigationResources.Menu_Project_DesignProgressDistribution_Title;
            }
            catch
            {
                return errorMessage;
            }

            return errorMessage;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}