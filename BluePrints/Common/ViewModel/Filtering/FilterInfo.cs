using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel.Filtering
{
    public class FilterInfo
    {
        public string Name { get; set; }
        public string FilterCriteria { get; set; }
        public string ImageUri { get; set; }
    }

    public class FilterInfoList : List<FilterInfo>
    {
        public FilterInfoList()
        {
        }

        public FilterInfoList(IEnumerable<FilterInfo> filters)
            : base(filters)
        {
        }

        public void AddUnique(FilterInfo filterInfo)
        {
            if (!this.Any(x => x.Name == filterInfo.Name))
                Add(filterInfo);
        }
    }
}