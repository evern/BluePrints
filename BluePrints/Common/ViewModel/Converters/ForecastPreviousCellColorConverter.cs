using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using System;
using System.Data;
using System.Linq;
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
            SolidColorBrush goodColor = new System.Windows.Media.SolidColorBrush(Colors.Transparent);
            SolidColorBrush badColor = new System.Windows.Media.SolidColorBrush(Colors.Transparent);

            SolidColorBrush transparentColor = new System.Windows.Media.SolidColorBrush(Colors.Transparent);
            try
            {
                if (values[0] == null || values[1] == null || values[2] == null)
                    return transparentColor;

                if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue || values[2] == DependencyProperty.UnsetValue)
                    return transparentColor;

                DataRow dataRow = (DataRow)values[0];
                if (!dataRow.Table.Columns.Contains("CompareEntities"))
                    return transparentColor;

                //return transparent color if it's got nothing to compare with
                if (dataRow["CompareEntities"] != DBNull.Value)
                {
                    DataTable childEntity = (DataTable)dataRow["CompareEntities"];
                    IForecastViewModel commodityJob = (IForecastViewModel)dataRow["Entity"];

                    if (childEntity.TableName == BluePrintsResources.ForecastCompareTableName)
                    {
                        string fieldname = values[1].ToString();
                        DateTime parseDateTime;
                        if (DateTime.TryParse(fieldname, out parseDateTime))
                        {
                            decimal previouslyForecastedCosts = (decimal)childEntity.Rows[System.Convert.ToInt32(BluePrintsResources.ForecastCompare_UncommittedRowIndex)][fieldname];

                            //parentValue will be actual cost e.g. Actuals + Materials + PO Forecast + Indirect Forecast
                            decimal parentValue = (decimal)values[2];

                            if (parentValue <= previouslyForecastedCosts)
                                return goodColor;
                            else
                                return badColor;
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