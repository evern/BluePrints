using BluePrints.Common.Resources;
using System;
using System.Data;
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
                    DataTable compareEntity = (DataTable)dataRow["CompareEntities"];
                    if (compareEntity.Rows.Count > 3)
                    {
                        string fieldname = values[1].ToString();
                        DateTime parseDateTime;
                        if (DateTime.TryParse(fieldname, out parseDateTime))
                        {
                            //decimal actualCosts = (decimal)compareEntity.Rows[0][fieldname];
                            //decimal materialCosts = (decimal)compareEntity.Rows[1][fieldname];
                            decimal poForecastCosts = (decimal)compareEntity.Rows[System.Convert.ToInt32(BluePrintsResources.ForecastCompare_POCostRow)][fieldname];
                            decimal p6RemainingCosts = (decimal)compareEntity.Rows[System.Convert.ToInt32(BluePrintsResources.ForecastCompareChild_P6CostRow)][fieldname];
                            //decimal totalCosts = actualCosts + materialCosts + poForecastCosts + p6RemainingCosts;
                            decimal totalCosts = poForecastCosts + p6RemainingCosts;
                            totalCosts = Math.Round(totalCosts);

                            return totalCosts;
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