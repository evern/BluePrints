using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel;
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

            if (value is ESTIMATION_DIRECT_ITEMProjection)
            {
                var estimation_item = value as ESTIMATION_DIRECT_ITEMProjection;

                return estimation_item.ReportableItem_Name;
            }

            if (value is GanttData)
            {
                var p6Activity = value as GanttData;

                return p6Activity.Description;
            }

            var listTreeListNodes = new List<Object>((IEnumerable<object>)value);
            if (listTreeListNodes.Count == 1)
            {
                var estimation_direct_item = listTreeListNodes.FirstOrDefault() as ESTIMATION_DIRECT_ITEMProjection;
                if (estimation_direct_item != null)
                {
                    return estimation_direct_item.ReportableItem_Name;
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
