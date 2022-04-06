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
                if (!dataRow.Table.Columns.Contains("CompareEntities"))
                    return null;

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
                            IForecastDateComments dateComment = dateCost as IForecastDateComments;
                            if (dateComment != null && dateComment.Comment != null && dateComment.Comment != string.Empty)
                                return dateComment.Comment;
                            else if(dateCost != null)
                            {
                                decimal p6RemainingCosts = dateCost.P6Costs;
                                decimal poForecastCosts = dateCost.POOutstandingCosts;
                                decimal indirectCosts = dateCost.IndirectForecastCosts;
                                decimal actualCosts = dateCost.ActualCosts;

                                decimal totalCosts = poForecastCosts + p6RemainingCosts + indirectCosts + actualCosts;
                                totalCosts = Math.Round(totalCosts);
                                decimal currentValue = (decimal)values[2];

                                string P6RemainingStr = "P6 forecast";
                                string associatedCosts = string.Empty;
                                if (p6RemainingCosts > 0)
                                    associatedCosts += P6RemainingStr + "\n";
                                if (poForecastCosts > 0)
                                    associatedCosts += "PO forecast\n";
                                if (indirectCosts > 0)
                                    associatedCosts += "Indirect forecast\n";
                                if (actualCosts > 0)
                                    associatedCosts += "Actual\n";

                                if (associatedCosts != string.Empty)
                                    associatedCosts = associatedCosts.Substring(0, associatedCosts.Length - 1);

                                currentValue = Math.Round(currentValue);
                                if (totalCosts != 0)
                                {
                                    if (currentValue > totalCosts)
                                        return "Color is green because edited cell value is higher than the cost of " + associatedCosts;
                                    else if (currentValue < totalCosts)
                                    {
                                        if (associatedCosts == P6RemainingStr)
                                            return "Color is red because edited cell value is superseded by higher P6 forecast cost" + "\nPlease single click this cell and press 'Del' key to reset it";

                                        return "Color is red because edited cell value is lower than the cost of " + associatedCosts;
                                    }
                                }
                                else if (currentValue > 0)
                                    return "Color is green to indicate that this cell has been edited";
                                else if (p6RemainingCosts != 0 && currentValue == 0)
                                    return "Color is yellow to indicate that this cell have P6 hours but no costs";
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