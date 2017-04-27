using System.Collections.Generic;
using System.Linq;

namespace BluePrints.Common.ViewModel.Filtering
{
    public interface IFilterTreeModelPageSpecificSettings<TFilterEntity>
    {
        string StaticFiltersTitle { get; }
        FilterInfoList StaticFilters { get; set; }
        FilterInfoList CustomFilters { get; set; }
        IQueryable<TFilterEntity> FilterEntities { get; }
        IEnumerable<string> HiddenFilterProperties { get; }
        IEnumerable<string> AdditionalFilterProperties { get; }
        void SaveSettings();
    }
}