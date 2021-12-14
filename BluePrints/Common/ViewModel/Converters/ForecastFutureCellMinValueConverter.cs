using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace BluePrints.Common.ViewModel.Converters
{
    public class ForecastFutureCellMinValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            try
            {
                if (values[0] == null || values[1] == null)
                    return 0;

                if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue)
                    return 0;

                DataRow dataRow = (DataRow)values[0];
                if(!dataRow.Table.Columns.Contains("CompareEntities"))
                    return 0;

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
                                decimal poForecastCosts = dateCost.POOutstandingCosts;
                                decimal p6RemainingCosts = dateCost.P6Costs;
                                decimal indirectCosts = dateCost.IndirectForecastCosts;
                                decimal actualCosts = dateCost.ActualCosts;

                                //decimal totalCosts = actualCosts + materialCosts + poForecastCosts + p6RemainingCosts;
                                decimal totalCosts = poForecastCosts + p6RemainingCosts + indirectCosts + actualCosts;
                                totalCosts = Math.Round(totalCosts);
                                return totalCosts;
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                string s = ex.ToString();
            }

             return 0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}