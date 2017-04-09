using BluePrints.Common.Projections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace BluePrints.Common.ViewModel.Converters
{
    public class BaselineItemMaximumUnitsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue || values[2] == DependencyProperty.UnsetValue || values[3] == DependencyProperty.UnsetValue)
                return 0;

            try
            {
                var totalAllowedUnits = (decimal)values[0];
                var AllEntities = (IEnumerable<PROGRESS_ITEMProjection>)values[1];
                PROGRESS_ITEMProjection currentRow = (PROGRESS_ITEMProjection)values[2];
                decimal currentUnits = (decimal)values[3];

                IEnumerable<PROGRESS_ITEMProjection> allEntitiesExcludingCurrent = AllEntities.Where(x => x.GUID != currentRow.GUID);
                decimal currentAssignedUnits = allEntitiesExcludingCurrent.Count() == 0 ? 0 : allEntitiesExcludingCurrent.Sum(x => x.Entity.Entity.ESTIMATED_HOURS);
                decimal spareUnits = totalAllowedUnits - currentAssignedUnits;
                return spareUnits > 0 ? spareUnits : 0;
            }
            catch
            {
                return 0;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}