using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using System;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace BluePrints.Common.ViewModel.Converters
{
    public class CellNumericUOMMaskConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string defaultFormat = "n0";
            try
            {
                if (value == null)
                    return defaultFormat;

                if (value == DependencyProperty.UnsetValue)
                    return defaultFormat;

                string UOMColumnName = BluePrintsResources.ForecastUOMColumnName;
                DataRow dataRow = (DataRow)value;
                if (!dataRow.Table.Columns.Contains(UOMColumnName))
                    return defaultFormat;

                if (dataRow[UOMColumnName] != DBNull.Value)
                {
                    string uom = dataRow[UOMColumnName].ToString();
                    return "###,##0 " + uom;
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