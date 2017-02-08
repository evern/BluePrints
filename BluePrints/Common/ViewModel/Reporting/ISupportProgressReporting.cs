using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm.POCO;

namespace BluePrints.Common.ViewModel.Reporting
{
    /// <summary>
    /// Describe classes that are able to produce S-Curve, Histogram, Cumulative/Period Stats
    /// </summary>
    public interface ISupportProgressReporting
    {
        #region Summary Item Stats

        ProgressInfo Summary_CumulativeOriginal { get; }

        ProgressInfo Summary_CumulativePlanned { get; }

        ProgressInfo Summary_CumulativeEarned { get; }

        ProgressInfo Summary_CumulativeBurned { get; }

        ProgressInfo Summary_CumulativeActual { get; }

        ProgressInfo Summary_PeriodOriginal { get; }

        ProgressInfo Summary_PeriodPlanned { get; }

        ProgressInfo Summary_PeriodEarned { get; }

        ProgressInfo Summary_PeriodBurned { get; }

        ProgressInfo Summary_PeriodActual { get; }

        #endregion

        #region Data Points Collection

        /// <summary>
        /// Non cumulative variation points with date and baseline item original guid
        /// </summary>
        ObservableCollection<VariationAdjustment> NonCumulative_VariationAdjustments { get; set; }

        /// <summary>
        /// Summary of the individual remaining based on planned productivity in the project
        /// </summary>
        ObservableCollection<ProgressInfo> NonCumulative_RemainingPlannedDataPoints { get; set; }

        /// <summary>
        /// Summary of the individual remaining based on current productivity in the project
        /// </summary>
        ObservableCollection<ProgressInfo> NonCumulative_RemainingCurrentDataPoints { get; set; }

        /// <summary>
        /// Summary of the individual remaining based on variation productivity in the project
        /// </summary>
        ObservableCollection<ProgressInfo> NonCumulative_RemainingVariationDataPoints { get; set; }

        /// <summary>
        /// Summary of the individual remaining based on planned productivity in the project
        /// </summary>
        ObservableCollection<ProgressInfo> Summary_CumulativeRemainingPlannedDataPoints { get; set; }

        /// <summary>
        /// Summary of the individual remaining based on remaining productivity in the project
        /// </summary>
        ObservableCollection<ProgressInfo> Summary_CumulativeRemainingCurrentDataPoints { get; set; }

        /// <summary>
        /// Summary of the individual remaining based on variation productivity in the project
        /// </summary>
        ObservableCollection<ProgressInfo> Summary_CumulativeRemainingVariationDataPoints { get; set; }

        /// <summary>
        /// Summary of the original planned for all baselineitem in the project
        /// </summary>
        ObservableCollection<ProgressInfo> NonCumulative_OriginalDataPoints { get; set; }

        /// <summary>
        /// Summary of the individual planned for all baselineitem in the project
        /// </summary>
        ObservableCollection<ProgressInfo> NonCumulative_PlannedDataPoints { get; set; }

        /// <summary>
        /// Summary of the individual earned for all baselineitem in the project
        /// </summary>
        ObservableCollection<ProgressInfo> NonCumulative_EarnedDataPoints { get; set; }


        /// <summary>
        /// Summary of the individual burned for all baselineitem in the project
        /// </summary>
        ObservableCollection<ProgressInfo> NonCumulative_BurnedDataPoints { get; set; }


        /// <summary>
        /// Summary of the individual actual for all baselineitem in the project
        /// </summary>
        ObservableCollection<ProgressInfo> NonCumulative_ActualDataPoints { get; set; }

        /// <summary>
        /// Summary of the variation adjustment units by date
        /// </summary>
        ObservableCollection<VariationAdjustment> Cumulative_VariationAdjustments { get; set; }

        /// <summary>
        /// Summary of the original planned for all baselineitem in the project
        /// </summary>
        ObservableCollection<ProgressInfo> Summary_CumulativeOriginalDataPoints { get; set; }

        /// <summary>
        /// Summary of the planned for all baselineitem in the project
        /// </summary>
        ObservableCollection<ProgressInfo> Summary_CumulativePlannedDataPoints { get; set; }

        /// <summary>
        /// Summary of the earned for principal project
        /// </summary>
        ObservableCollection<ProgressInfo> Summary_CumulativeEarnedDataPoints { get; set; }

        /// <summary>
        /// Summary of the burned for principal project
        /// </summary>
        ObservableCollection<ProgressInfo> Summary_CumulativeBurnedDataPoints { get; set; }

        /// <summary>
        /// Summary of the actuals for principal project
        /// </summary>
        ObservableCollection<ProgressInfo> Summary_CumulativeActualDataPoints { get; set; }

        /// <summary>
        /// Summary of the planned by period for all baselineitem in the project
        /// </summary>
        ObservableCollection<ProgressInfo> Summary_PeriodPlannedDataPoints { get; }

        /// <summary>
        /// Summary of the earned by period for all baselineitem in the project
        /// </summary>
        ObservableCollection<ProgressInfo> Summary_PeriodEarnedDataPoints { get; }

        /// <summary>
        /// Summary of the burned by period for all baselineitem in the project
        /// </summary>
        ObservableCollection<ProgressInfo> Summary_PeriodBurnedDataPoints { get; }

        /// <summary>
        /// Summary of the actual by period for all baselineitem in the project
        /// </summary>
        ObservableCollection<ProgressInfo> Summary_PeriodActualDataPoints { get; }

        /// <summary>
        /// Summary of the earned by period for all baselineitem in the project
        /// </summary>
        ObservableCollection<ProgressInfo> Summary_PeriodRemainingVariationDataPoints { get; }

        /// <summary>
        /// Summary of the planned by period for all baselineitem in the project
        /// </summary>
        ObservableCollection<ProgressInfo> Summary_PeriodRemainingPlannedDataPoints { get; }

        /// <summary>
        /// Summary of the current by period for all baselineitem in the project
        /// </summary>
        ObservableCollection<ProgressInfo> Summary_PeriodRemainingCurrentDataPoints { get; }

        #endregion
    }
}