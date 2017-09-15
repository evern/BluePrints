using BluePrints.Common.Projections;
using DevExpress.Xpf.Grid;
using System;
using System.Windows;
using System.Windows.Data;

namespace BluePrints.Common.ViewModel.Converters
{
    public class MeetingMasterDetailNewRowTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == DependencyProperty.UnsetValue || value == null)
                return string.Empty;

            TableView view = value as TableView;
            MINUTE_AGENDAMasterDetailProjection rowData = view.DataControl.CurrentItem as MINUTE_AGENDAMasterDetailProjection;
            if (rowData == null)
                return string.Empty;
            else
            {
                int stringLength = rowData.Entity.NAME.Length;
                if (stringLength > 20)
                    stringLength = 20;

                return @"Click here to add new comment to """ + rowData.Entity.NAME.Substring(0, stringLength) + @""" ... push enter when completed";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}