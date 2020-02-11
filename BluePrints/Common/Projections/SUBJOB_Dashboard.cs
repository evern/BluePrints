using BluePrints.Common.Base;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.Common.Projections
{
    public class SUBJOB_Dashboard : BluePrintsProjectionBase<SUBJOB>, IHaveStats
    {
        public ProgressStats Stats { get; set; }
        public IEnumerable<AREA> AvailableSubAreas { get; set; }


        public void SetAvailableSubAreas(IEnumerable<AREA> SUBAREACollection)
        {
            AvailableSubAreas = SUBAREACollection.Where(x => x.GUID_PARENT == Entity.GUID_DAREA);
            this.RaisePropertyChanged();
        }

        public void GroupProjectStats(SummaryStats project_summary_stats)
        {
            Stats = SummaryStatsHelpers.Group_Summary_Stats(project_summary_stats, x => x.Subjob_Guid == Entity.GUID, x => x.Subjob_Name == Entity.INTERNAL_NAME1);
        }

        #region SUBJOB Mapping
        public bool IsGetModifiedSUBJOB_ASSIGNMENTS { get; set; }
        public ICollection<SUBJOB_ASSIGNMENT> ObservableSUBJOB_ASSIGNMENTS
        {
            get
            {
                return
                    Entity.SUBJOB_ASSIGNMENT.Where(x => x.ISMODIFIEDBASELINE == IsGetModifiedSUBJOB_ASSIGNMENTS).OrderBy(x => x.LOW_VALUE)
                        .ToList();
            }
        }

        public virtual ICollection<BASELINE_ITEM> BASELINE_ITEMS { get; set; }

        public decimal ASSIGNED_UNITS
        {
            get
            {
                return Stats.totalUnits * ObservableSUBJOB_ASSIGNMENTS.Sum(x => (x.HIGH_VALUE - x.LOW_VALUE) + 0.01m);
            }
        }

        public decimal ASSIGNED_PERCENTAGE
        {
            get
            {
                return ObservableSUBJOB_ASSIGNMENTS.Sum(x => (x.HIGH_VALUE - x.LOW_VALUE) + 0.01m);
            }
        }
        #endregion
    }
}