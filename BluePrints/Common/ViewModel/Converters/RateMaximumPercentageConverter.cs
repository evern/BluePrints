using BaseModel.Misc;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace BluePrints.Common.ViewModel.Converters
{
    public class RateMaximumPercentageConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            decimal defaultMaxValue = 1m;
            if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue || values[2] == DependencyProperty.UnsetValue)
                return defaultMaxValue;

            if (values[0] == null || values[1] == null || values[2] == null)
                return defaultMaxValue;

            try
            {
                decimal? currentPercentage = (decimal?)values[0];
                RATE projection = values[1] as RATE;
                LightweightCellEditor cellEditor = values[2] as LightweightCellEditor;
                if (projection == null)
                    return defaultMaxValue;

                //make sure property is already set so total percentage will always be correct;
                string fieldName = cellEditor.Column.FieldName;
                decimal? excludePercentage = (decimal?)projection.GetType().GetProperty(fieldName).GetValue(projection);
                
                decimal managerPercent = projection.MANAGER_PERCENT == null ? 0 : (decimal)projection.MANAGER_PERCENT;
                decimal principalPercent = projection.PRINCIPAL_PERCENT == null ? 0 : (decimal)projection.PRINCIPAL_PERCENT;
                decimal leadPercent = projection.LEAD_PERCENT == null ? 0 : (decimal)projection.LEAD_PERCENT;
                decimal seniorPercent = projection.SENIOR_PERCENT == null ? 0 : (decimal)projection.SENIOR_PERCENT;
                decimal engineerPercent = projection.ENGINEER_PERCENT == null ? 0 : (decimal)projection.ENGINEER_PERCENT;
                decimal graduatePercent = projection.GRADUATE_PERCENT == null ? 0 : (decimal)projection.GRADUATE_PERCENT;
                decimal undergraduatePercent = projection.UNDERGRADUATE_PERCENT == null ? 0 : (decimal)projection.UNDERGRADUATE_PERCENT;
                decimal excludePercent = excludePercentage == null ? 0 : (decimal)excludePercentage;

                decimal totalPercentIncludeCurrent = projection.TotalPercent;
                decimal totalPercentageExcludeCurrent = totalPercentIncludeCurrent - excludePercent;

                decimal remainingPercent = 1 - totalPercentageExcludeCurrent;
                if (remainingPercent > 1)
                    return defaultMaxValue;
                else
                    return remainingPercent;
            }
            catch
            {
                return defaultMaxValue;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}