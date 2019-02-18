using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace BluePrints.Common.ViewModel.Converters
{
    public class ProjectSummaryCellReadOnlyConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            try
            {
                if (values[0] == null || values[1] == null)
                    return true;

                if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue)
                    return true;

                DataRow dataRow = (DataRow)values[0];
                string fieldName = values[1].ToString();
                List<Tuple<string, string>> fieldNamesLookup = (List<Tuple<string, string>>)dataRow["Lookup"];
                Tuple<string, string> currentFieldName = fieldNamesLookup.FirstOrDefault(x => x.Item1 == fieldName);

                if(currentFieldName != null)
                {
                    string readOnlyFieldName = currentFieldName.Item2;
                    bool isReadOnly = (bool)dataRow[readOnlyFieldName];

                    return isReadOnly;
                }
            }
            catch(Exception ex)
            {
                string s = ex.ToString();
            }

             return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}