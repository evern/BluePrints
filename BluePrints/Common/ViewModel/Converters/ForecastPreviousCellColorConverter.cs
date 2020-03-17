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
                                decimal eacCosts = dateCost.EACCosts;
                                decimal parentValue = (decimal)values[2];

                                if (parentValue <= eacCosts)
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

            return transparentColor;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}