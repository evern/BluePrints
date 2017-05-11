using BluePrints.Common.Projections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace BluePrints.Common.ViewModel.Converters
{
    public class BaselineItemMaximumUnitsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue || values[2] == DependencyProperty.UnsetValue)
                return 0;

            if (values[0] == null || values[1] == null || values[2] == null)
                return 0;

            try
            {
                var totalAllowedUnits = (decimal)values[0];
                if (totalAllowedUnits == 0)
                    return (int)1000000;

                var AllEntities = (IEnumerable<PROGRESS_ITEMProjection>)values[1];
                PROGRESS_ITEMProjection currentRow = (PROGRESS_ITEMProjection)values[2];

                IEnumerable<PROGRESS_ITEMProjection> allEntitiesExcludingCurrent = AllEntities.Where(x => x.EntityKey != currentRow.EntityKey);
                decimal currentAssignedUnits = allEntitiesExcludingCurrent.Count() == 0 ? (int)0 : allEntitiesExcludingCurrent.Sum(x => x.Entity.Entity.ESTIMATED_HOURS);
                decimal spareUnits = totalAllowedUnits - currentAssignedUnits;
                return spareUnits > 0 ? spareUnits : (int)0;
            }
            catch
            {
                return (int)0;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}