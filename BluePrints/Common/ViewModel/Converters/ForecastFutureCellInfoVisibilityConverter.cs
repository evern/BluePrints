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
    public class ForecastFutureCellInfoVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            try
            {
                if (values[0] == null || values[1] == null || values[2] == null)
                    return Visibility.Collapsed;

                if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue || values[2] == DependencyProperty.UnsetValue)
                    return Visibility.Collapsed;

                DataRow dataRow = (DataRow)values[0];
                IForecastViewModel commodityJob = (IForecastViewModel)dataRow["Entity"];

                string fieldname = values[1].ToString();
                DateTime parseDateTime;
                if (DateTime.TryParse(fieldname, out parseDateTime))
                {
                    IForecastDateCostViewModel dateCost = commodityJob.ForecastDateCosts.FirstOrDefault(x => x.QueryDate.Date == parseDateTime.Date);
                    IForecastDateComments dateComment = dateCost as IForecastDateComments;
                    if (dateComment != null && dateComment.Comment != null && dateComment.Comment != string.Empty)
                        return Visibility.Visible;
                }

                return Visibility.Collapsed;
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