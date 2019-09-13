using BluePrints.Common.Projections;
using System;
using System.Data;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace BluePrints.Common.ViewModel.Converters
{
    public class ForecastFutureChildCellColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            SolidColorBrush transparentColor = new System.Windows.Media.SolidColorBrush(Colors.Transparent);
            try
            {
                if (values[0] == null || values[1] == null || values[2] == null)
                    return transparentColor;

                if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue || values[2] == DependencyProperty.UnsetValue)
                    return transparentColor;

                DataRow dataRow = (DataRow)values[0];
                if(!dataRow.Table.Columns.Contains("CompareEntities"))
                    return transparentColor;

                ForecastJobData jobData = (ForecastJobData)dataRow["Entity"];
                if (!jobData.IsP6HoursRow)
                    return transparentColor;

                if (dataRow["CompareEntities"] != DBNull.Value)
                {
                    DataTable compareEntity = (DataTable)dataRow["CompareEntities"];
                    if (compareEntity.Rows.Count > 0)
                    {
                        string fieldname = values[1].ToString();
                        DateTime parseDateTime;
                        if (DateTime.TryParse(fieldname, out parseDateTime))
                        {
                            decimal p6RemainingHours = (decimal)compareEntity.Rows[1][fieldname];
                            decimal totalCosts = Math.Round(p6RemainingHours);
                            decimal currentValue = (decimal)values[2];

                            if(totalCosts != 0)
                            {
                                currentValue = Math.Round(currentValue);
                                if(currentValue > totalCosts)
                                    return new System.Windows.Media.SolidColorBrush(Colors.Chartreuse);
                                else if (currentValue < totalCosts)
                                    return new System.Windows.Media.SolidColorBrush(Colors.LightSalmon);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                string s = ex.ToString();
            }

             return transparentColor;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}