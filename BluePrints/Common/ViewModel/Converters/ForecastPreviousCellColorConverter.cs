using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
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
                    ForecastJobData commodityJob = (ForecastJobData)dataRow["Entity"];

                    if (childEntity.TableName == BluePrintsResources.ForecastCompareTableName)
                    {
                        string fieldname = values[1].ToString();
                        DateTime parseDateTime;
                        if (DateTime.TryParse(fieldname, out parseDateTime))
                        {
                            ForecastDateCost dateCost = commodityJob.DateCosts.FirstOrDefault(x => x.Date.Date == parseDateTime.Date);
                            if (dateCost != null)
                            {
                                decimal poForecastCosts = dateCost.POForecastCosts;
                                decimal p6RemainingCosts = dateCost.P6Costs;
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