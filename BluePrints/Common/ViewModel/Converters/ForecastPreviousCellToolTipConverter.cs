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
    public class ForecastPreviousCellToolTipConverter : IMultiValueConverter
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
                                    return "Color is green because actuals are less than EAC";
                                else
                                    return "Color is red because actuals are more than EAC";
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