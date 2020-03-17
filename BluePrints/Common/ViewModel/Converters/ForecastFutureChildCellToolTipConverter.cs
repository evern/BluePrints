using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using System;
using System.Data;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace BluePrints.Common.ViewModel.Converters
{
    public class ForecastFutureChildCellToolTipConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            try
            {
                if (values[0] == null || values[1] == null || values[2] == null)
                    return null;

                if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue || values[2] == DependencyProperty.UnsetValue)
                    return null;

                DataRow dataRow = (DataRow)values[0];
                if(!dataRow.Table.Columns.Contains("CompareEntities"))
                    return null;

                ForecastJobData jobData = (ForecastJobData)dataRow["Entity"];
                if (!jobData.IsP6HoursRow)
                    return null;

                if (dataRow["CompareEntities"] != DBNull.Value)
                {
                    DataTable compareEntity = (DataTable)dataRow["CompareEntities"];
                    if (compareEntity.Rows.Count > 0)
                    {
                        string fieldname = values[1].ToString();
                        DateTime parseDateTime;
                        if (DateTime.TryParse(fieldname, out parseDateTime))
                        {
                            decimal p6RemainingHours = (decimal)compareEntity.Rows[System.Convert.ToInt32(BluePrintsResources.ForecastCompareChild_P6HourRowIndex)][fieldname];
                            decimal totalHours = Math.Round(p6RemainingHours);
                            decimal currentValue = (decimal)values[2];

                            currentValue = Math.Round(currentValue);
                            if(currentValue > totalHours)
                                return "Color is green because value has been overridden and it's higher than P6 calculated value of " + totalHours;
                            else if (currentValue < totalHours)
                                return "Color is red because value has been overridden and it's lower than P6 calculated value of " + totalHours;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                string s = ex.ToString();
            }

             return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}