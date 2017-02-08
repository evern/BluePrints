using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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