using BaseModel.Misc;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace BluePrints.Common.ViewModel.Converters
{
    public class IReportableStatsMaximumUnitsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue || values[2] == DependencyProperty.UnsetValue)
                return 0m;

            if (values[0] == null || values[1] == null || values[2] == null)
                return 0m;

            try
            {
                IReportable projection = values[2] as IReportable;
                if (projection == null)
                    return 0m;

                var totalAllowedUnits = (decimal)values[0];
                if (totalAllowedUnits == 0)
                    return 10000m;

                var AllEntities = (IEnumerable<IReportable>)values[1];

                IEnumerable<IReportable> allEntitiesExcludingCurrent = AllEntities.Where(x => x.GUID != projection.GUID);
                decimal currentAssignedUnits = allEntitiesExcludingCurrent.Count() == 0 ? 0 : allEntitiesExcludingCurrent.Sum(x => x.Budget_Units);
                decimal spareUnits = totalAllowedUnits - currentAssignedUnits;
                return spareUnits > 0 ? spareUnits : 0m;
            }
            catch
            {
                return 0m;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}