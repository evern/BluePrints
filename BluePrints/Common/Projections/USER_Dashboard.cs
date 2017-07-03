using BluePrints.Common.Base;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.P6EntitiesDataModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.Common.Projections
{
    public class USER_Dashboard : BluePrintsProjectionBase<PROJECT>, IHaveStats
    {
        public PROGRESS_ITEMProjection PROGRESS_ITEMProjection { get; set; }
        public PartialStatsBuilder DataPointsBuilder { get; set; }

        /// <summary>
        /// Used for summarizing selection of multiple user dashboard
        /// </summary>
        ProgressStats aggregateProgressStats { get; set; }
        public ProgressStats Stats
        {
            get
            {
                if (PROGRESS_ITEMProjection != null)
                    return PROGRESS_ITEMProjection.Stats;
                else if (aggregateProgressStats != null)
                    return aggregateProgressStats;

                return null;
            }
            set
            {
                aggregateProgressStats = value;
            }
        }

        public decimal UnitsProgressRatio
        {
            get
            {
                if (Stats.Earned.CurrentPeriodCumulativeDataPoint != null && Stats.Budgeted.CurrentPeriodCumulativeDataPoint != null)
                {
                    decimal earnedunits = Stats.Earned.CurrentPeriodCumulativeDataPoint.Units;
                    decimal plannedunits = Stats.Budgeted.CurrentPeriodCumulativeDataPoint.Units;

                    if (plannedunits > 0 && earnedunits > 0)
                        return earnedunits / plannedunits;
                    else
                        return 0;
                }
                else
                    return 0;
            }
        }

        public decimal CostsProgressRatio
        {
            get
            {
                if (Stats.Earned.CurrentPeriodCumulativeDataPoint != null && Stats.Budgeted.CurrentPeriodCumulativeDataPoint != null)
                {
                    decimal earnedcosts = Stats.Earned.CurrentPeriodCumulativeDataPoint.Costs;
                    decimal plannedcosts = Stats.Budgeted.CurrentPeriodCumulativeDataPoint.Costs;

                    if (plannedcosts > 0 && earnedcosts > 0)
                        return earnedcosts / plannedcosts;
                    else
                        return 0;
                }
                else
                    return 0;
            }
        }
    }
}