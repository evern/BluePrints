using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using System;
using System.Linq;
using System.Data;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using BluePrints.Common.ViewModel.Reporting;

namespace BluePrints.Common.ViewModel.Converters
{
    public class ForecastFutureCellColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            SolidColorBrush transparentColor = new SolidColorBrush(Colors.Transparent);
            try
            {
                if (values[0] == null || values[1] == null || values[2] == null)
                    return transparentColor;

                if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue || values[2] == DependencyProperty.UnsetValue)
                    return transparentColor;

                DataRow dataRow = (DataRow)values[0];
                if(!dataRow.Table.Columns.Contains("CompareEntities"))
                    return transparentColor;

                if (dataRow["CompareEntities"] != DBNull.Value)
                {
                    DataTable compareDataTable = (DataTable)dataRow["CompareEntities"];
                    IForecastViewModel commodityJob = (IForecastViewModel)dataRow["Entity"];

                    if (compareDataTable.TableName == BluePrintsResources.ForecastCompareTableName)
                    {
                        string fieldname = values[1].ToString();
                        DateTime parseDateTime;
                        if (DateTime.TryParse(fieldname, out parseDateTime))
                        {
                            IForecastDateCostViewModel dateCost = commodityJob.ForecastDateCosts.FirstOrDefault(x => x.QueryDate.Date == parseDateTime.Date);
                            if (dateCost != null)
                            {
                                decimal p6RemainingCosts = dateCost.P6Costs;
                                decimal totalCosts = dateCost.TotalCosts;
                                totalCosts = Math.Round(totalCosts);
                                decimal currentValue = (decimal)values[2];

                                currentValue = Math.Round(currentValue);
                                if (totalCosts != 0)
                                {
                                    if (currentValue > totalCosts)
                                        return new SolidColorBrush(Colors.Chartreuse);
                                    else if (currentValue < totalCosts)
                                        return new SolidColorBrush(Colors.LightSalmon);
                                }
                                else if (currentValue > 0)
                                    return new SolidColorBrush(Colors.Chartreuse);
                                else if(p6RemainingCosts != 0 && currentValue == 0)
                                    return new SolidColorBrush(Colors.Yellow);

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