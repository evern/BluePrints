using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel.Reporting;
using System;
using System.Data;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace BluePrints.Common.ViewModel.Converters
{
    public class ForecastChildCellReadOnlyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == DependencyProperty.UnsetValue || value == null)
                return false;

            DataRow dataRow = (DataRow)value;
            if (!dataRow.Table.Columns.Contains("Entity"))
                return true;

            if (dataRow["Entity"] != DBNull.Value)
            {
                IForecastViewModel jobData = (IForecastViewModel)dataRow["Entity"];
                return !jobData.IsP6HoursRow;
            }

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}