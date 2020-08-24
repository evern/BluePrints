using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using System;
using System.Linq;
using System.Data;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace BluePrints.Common.ViewModel.Converters
{
    public class ForecastFutureCellColorConverter : IMultiValueConverter
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

                if (dataRow["CompareEntities"] != DBNull.Value)
                {
                    DataTable compareDataTable = (DataTable)dataRow["CompareEntities"];
                    ForecastJobData commodityJob = (ForecastJobData)dataRow["Entity"];

                    if (commodityJob.P6RemainingUnitsOverride == 0)
                    {
                        if (compareDataTable.TableName == BluePrintsResources.ForecastCompareTableName)
                        {
                            string fieldname = values[1].ToString();
                            DateTime parseDateTime;
                            if (DateTime.TryParse(fieldname, out parseDateTime))
                            {
                                ForecastDateCost dateCost = commodityJob.DateCosts.FirstOrDefault(x => x.Date.Date == parseDateTime.Date);
                                if (dateCost != null)
                                {
                                    decimal p6RemainingCosts = dateCost.P6Costs;
                                    decimal poForecastCosts = dateCost.POForecastCosts;
                                    decimal indirectCosts = dateCost.IndirectForecastCosts;
                                    decimal materialCosts = dateCost.MaterialCosts;
                                    decimal actualCosts = dateCost.ActualCosts;

                                    decimal totalCosts = poForecastCosts + indirectCosts + materialCosts + actualCosts;
                                    totalCosts = Math.Round(totalCosts);
                                    decimal currentValue = (decimal)values[2];

                                    if (p6RemainingCosts <= 0)
                                    {
                                        if (totalCosts != 0)
                                        {
                                            currentValue = Math.Round(currentValue);
                                            if (currentValue > totalCosts)
                                                return new System.Windows.Media.SolidColorBrush(Colors.Chartreuse);
                                            else if (currentValue < totalCosts)
                                                return new System.Windows.Media.SolidColorBrush(Colors.LightSalmon);
                                        }
                                        else if (currentValue > 0)
                                            return new System.Windows.Media.SolidColorBrush(Colors.Chartreuse);
                                    }
                                }
                            }
                        }
                    }
                    else
                        return transparentColor;
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