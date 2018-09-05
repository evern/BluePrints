using System;
using System.Data;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace BluePrints.Common.ViewModel.Converters
{
    public class TagToColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            try
            {
                if (values[0] == null || values[1] == null || values[2] == null)
                    return new System.Windows.Media.SolidColorBrush(Colors.LimeGreen);

                if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue || values[2] == DependencyProperty.UnsetValue)
                    return new System.Windows.Media.SolidColorBrush(Colors.LimeGreen);

                DataRow dataRow = (DataRow)values[0];
                if (dataRow["ChildEntities"] != DBNull.Value)
                {
                    DataTable childEntity = (DataTable)dataRow["ChildEntities"];
                    if (childEntity.Rows.Count > 0)
                    {
                        string fieldname = values[1].ToString();
                        DateTime parseDateTime;
                        if (DateTime.TryParse(fieldname, out parseDateTime))
                        {
                            decimal previousValue = (decimal)childEntity.Rows[0][fieldname];
                            decimal currentValue = (decimal)values[2];

                            if (currentValue >= previousValue)
                                return new System.Windows.Media.SolidColorBrush(Colors.LimeGreen);
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


            SolidColorBrush solidColor = new System.Windows.Media.SolidColorBrush(Colors.LimeGreen);
            return solidColor;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}