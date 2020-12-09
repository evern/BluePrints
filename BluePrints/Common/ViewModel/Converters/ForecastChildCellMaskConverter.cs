using BluePrints.Common.Projections;
using System;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace BluePrints.Common.ViewModel.Converters
{
    public class ForecastChildCellMaskConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string defaultFormat = "c0";
            try
            {
                if (value == null)
                    return defaultFormat;

                if (value == DependencyProperty.UnsetValue)
                    return defaultFormat;

                DataRow dataRow = (DataRow)value;
                if (!dataRow.Table.Columns.Contains("Entity"))
                    return defaultFormat;

                if (dataRow["Entity"] != DBNull.Value)
                {
                    ForecastJobData job = (ForecastJobData)dataRow["Entity"];
                    return job.CompareMask;
                }
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
            }

            return defaultFormat;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}