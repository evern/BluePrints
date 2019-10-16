using BluePrints.Common.Resources;
using System;
using System.Data;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace BluePrints.Common.ViewModel.Converters
{
    public class ForecastPreviousCellColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            SolidColorBrush paleGreenColor = new System.Windows.Media.SolidColorBrush(Colors.Chartreuse);
            try
            {
                if (values[0] == null || values[1] == null || values[2] == null)
                    return paleGreenColor;

                if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue || values[2] == DependencyProperty.UnsetValue)
                    return paleGreenColor;

                DataRow dataRow = (DataRow)values[0];
                if (!dataRow.Table.Columns.Contains("CompareEntities"))
                    return paleGreenColor;

                if (dataRow["CompareEntities"] != DBNull.Value)
                {
                    DataTable childEntity = (DataTable)dataRow["CompareEntities"];
                    if (childEntity.Rows.Count > 3)
                    {
                        string fieldname = values[1].ToString();
                        DateTime parseDateTime;
                        if (DateTime.TryParse(fieldname, out parseDateTime))
                        {
                            //decimal actualCosts = (decimal)childEntity.Rows[0][fieldname];
                            //decimal materialCosts = (decimal)childEntity.Rows[1][fieldname];
                            decimal poForecastCosts = (decimal)childEntity.Rows[System.Convert.ToInt32(BluePrintsResources.ForecastCompare_POCostRow)][fieldname];
                            decimal p6RemainingCosts = (decimal)childEntity.Rows[System.Convert.ToInt32(BluePrintsResources.ForecastCompare_P6CostRow)][fieldname];
                            //decimal totalCosts = actualCosts + materialCosts + poForecastCosts + p6RemainingCosts;
                            decimal totalCosts = poForecastCosts + p6RemainingCosts;
                            decimal parentValue = (decimal)values[2];

                            if (parentValue <= totalCosts)
                                return paleGreenColor;
                            else
                                return new System.Windows.Media.SolidColorBrush(Colors.LightSalmon);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                string s = ex.ToString();
            }

            return paleGreenColor;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}