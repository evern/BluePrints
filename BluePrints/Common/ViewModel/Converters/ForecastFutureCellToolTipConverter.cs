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
    public class ForecastFutureCellToolTipConverter : IMultiValueConverter
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

                if (dataRow["CompareEntities"] != DBNull.Value)
                {
                    DataTable compareDataTable = (DataTable)dataRow["CompareEntities"];
                    ForecastJobData commodityJob = (ForecastJobData)dataRow["Entity"];

                    if (compareDataTable.TableName == BluePrintsResources.ForecastCompareTableName)
                    {
                        string fieldname = values[1].ToString();
                        DateTime parseDateTime;
                        if (DateTime.TryParse(fieldname, out parseDateTime))
                        {
                            ForecastDateCost dateCost = commodityJob.DateCosts.FirstOrDefault(x => x.Date.Date == parseDateTime.Date);
                            if(dateCost != null)
                            {
                                decimal p6RemainingCosts = dateCost.P6Costs;
                                decimal poForecastCosts = dateCost.POForecastCosts;
                                decimal indirectCosts = dateCost.IndirectForecastCosts;
                                decimal materialCosts = dateCost.MaterialCosts;
                                decimal actualCosts = dateCost.ActualCosts;

                                decimal totalCosts = poForecastCosts + p6RemainingCosts + indirectCosts + materialCosts + actualCosts;
                                totalCosts = Math.Round(totalCosts);
                                decimal currentValue = (decimal)values[2];

                                string P6RemainingStr = "P6 remaining";
                                string associatedCosts = string.Empty;
                                if (p6RemainingCosts > 0)
                                    associatedCosts += P6RemainingStr + ", ";
                                if (poForecastCosts > 0)
                                    associatedCosts += "PO forecast, ";
                                if (indirectCosts > 0)
                                    associatedCosts += "Indirect forecast, ";
                                if (materialCosts > 0)
                                    associatedCosts += "Material, ";
                                if (actualCosts > 0)
                                    associatedCosts += "Actual, ";

                                if (associatedCosts != string.Empty)
                                    associatedCosts = associatedCosts.Substring(0, associatedCosts.Length - 2);

                                if (totalCosts != 0)
                                {
                                    currentValue = Math.Round(currentValue);
                                    if (currentValue > totalCosts)
                                        return "Color is green because cell value is higher than the cost of " + associatedCosts;
                                    else if (currentValue < totalCosts)
                                    {
                                        if (associatedCosts == P6RemainingStr)
                                            return "Color is red because cell value is lower than P6 remaining cost, please press 'Del' key on the cell to reset it";

                                        return "Color is red because cell value is lower than the cost of " + associatedCosts;
                                    }
                                }
                                else if (currentValue > 0)
                                    return "Color is green because there aren't any system cost on this date and cell value is more than zero";
                            }
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