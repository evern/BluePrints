using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BluePrints.Common
{
    public class RowNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;

            if (value is IDeliverable)
            {
                var deliverable = value as IDeliverable;

                return deliverable.Deliverable_Name;
            }

            if (value is P6_Activity)
            {
                var p6Activity = value as P6_Activity;

                return p6Activity.Description;
            }

            if (value is IReportable)
            {
                var reportable = value as IReportable;
                return reportable.Deliverable_Name;
            }

            var listTreeListNodes = new List<Object>((IEnumerable<object>)value);
            if (listTreeListNodes.Count == 1)
            {
                var deliverable = listTreeListNodes.FirstOrDefault() as IDeliverable;
                if (deliverable != null)
                {
                    return deliverable.Deliverable_Name;
                }
            }
            else
            {
                return "Multiple rows selected";
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }
}
