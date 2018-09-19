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
            SolidColorBrush paleGreenColor = new System.Windows.Media.SolidColorBrush(Colors.PaleGreen);
            try
            {
                if (values[0] == null || values[1] == null || values[2] == null)
                    return paleGreenColor;

                if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue || values[2] == DependencyProperty.UnsetValue)
                    return paleGreenColor;

                DataRow dataRow = (DataRow)values[0];
                if (dataRow["ChildEntities"] != DBNull.Value)
                {
                    DataTable childEntity = (DataTable)dataRow["CompareEntities"];
                    if (childEntity.Rows.Count > 0)
                    {
                        string fieldname = values[1].ToString();
                        DateTime parseDateTime;
                        if (DateTime.TryParse(fieldname, out parseDateTime))
                        {
                            decimal childValue = (decimal)childEntity.Rows[0][fieldname];
                            decimal parentValue = (decimal)values[2];

                            if (parentValue <= childValue)
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