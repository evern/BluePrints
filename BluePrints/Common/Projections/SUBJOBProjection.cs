using BaseModel.Attributes;
using BluePrints.Common.Base;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BluePrints.Common.Projections
{
    [ConstraintAttributes("Entity.GUID_PROJECT, Entity.INTERNAL_NAME1")]
    public class SUBJOBProjection : BluePrintsProjectionBase<SUBJOB>, IReportable_Group
    {
        public IEnumerable<IReportable> DeliverableRates { get; set; }

        public IEnumerable<IReportable> Reportables => DeliverableRates;

        public SingleObjectSummarizer StatSummarizer => throw new NotImplementedException();

        public decimal Current_Productivity => throw new NotImplementedException();

        public decimal? Override_Productivity { get => 0; set => throw new NotImplementedException(); }

        public decimal? Remaining_Productivity => throw new NotImplementedException();

        public IEnumerable<User_Weight> AssignedUsers => Reportables == null ? new List<User_Weight>() : Reportables.SelectMany(x => x.AssignedUsers);

        public string Project_Number => Entity.PROJECT == null ? string.Empty : Entity.PROJECT.NUMBER;

        public string Subjob_Name => Entity.INTERNAL_NAME1;

        public PhaseType? Phase => Entity.PHASE.PHASE_TYPE;

        public ChargeType? Charge => Entity.PHASE.CHARGE_TYPE;

        public string Phase_Code => Entity.INTERNAL_NAME1;

        public string Department_Code => string.Empty;

        public string Discipline_Code => string.Empty;

        public string Deliverable_Name => string.Empty;

        public Guid? Phase_Guid { get => Entity.GUID_DPHASE; set => Entity.GUID_DPHASE = value; }

        public Guid? Subjob_Guid { get; set; }

        public Guid? Area_Guid => Entity.GUID_DAREA;

        public Guid? SubArea_Guid => Entity.GUID_DSUBAREA;

        public Guid? Discipline_Guid => Guid.Empty;

        public decimal Discipline_Number => 0;

        public Guid? Workpack_Guid { get; set; }
        public bool IsByDuration { get; set; }

        public Guid OriginalEntityKey => Guid.Empty;

        public string Commodity_Code => string.Empty;

        public decimal Budget_Units => Reportables == null ? 0 : Reportables.Sum(x => x.Budget_Units);

        public decimal Budget_Quantity => 0;

        public decimal Total_Quantity => 0;

        public decimal Budget_ItemRate => Reportables == null ? 0 : Reportables.Sum(x => x.Budget_ItemRate);

        public decimal Budget_Costs => Reportables == null ? 0 : Reportables.Sum(x => x.Budget_Costs);

        public ProgressStats Stats { get; set; }

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_BeforeDataDate => null;

        public PROGRESS_ITEM PROGRESS_ITEM_Current => null;

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_UpToCurrentDataDate => null;

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_AfterDataDate => null;

        public DateTime ReportingDataDate => DateTime.Now;

        public List<PROGRESS_ITEM> PROGRESS_ITEMS => null;

        public decimal Earned_Units_Total => 0;

        public decimal Earned_Costs_Total => 0;

        public decimal Earned_Units_BeforeDataDate => 0;

        public decimal Earned_Units_OnDataDate => 0;

        public decimal Earned_Units_ToDate => 0;

        public decimal Earned_Costs_ToDate => 0;

        public decimal Earned_Costs_OnDataDate => 0;

        public decimal Earned_Units_AfterDataDate => 0;

        public decimal Total_Earned_Percentage { get; set; }

        public decimal Total_Percentage => 0;

        public decimal Total_Percentage_ToDate => 0;

        public decimal Baseline_Percentage => 0;

        public decimal SchedulePercentage => 0;

        public decimal ScheduleCurrentPeriodPercentage => 0;

        public decimal MinPercentage => 0;

        public decimal MaxPercentage => 0;

        public bool ShouldSaveProgress => false;

        public decimal MinEstimateUnits => 0;

        public decimal Variation_Units => 0;

        public decimal Variation_Costs => 0;

        public decimal Total_Units => Reportables == null ? 0 : Reportables.Sum(x => x.Total_Units);

        public decimal Total_Costs => Reportables == null ? 0 : Reportables.Sum(x => x.Total_Costs);

        public void AppendProgressItem(PROGRESS_ITEM currentProgress)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PROGRESS_ITEM> GetExistingOrNewEditedProgresses(Func<Expression<Func<PROGRESS_ITEM, bool>>, PROGRESS_ITEM> repository_find_actual_func)
        {
            throw new NotImplementedException();
        }

        public void SetOriginalEntityKey(Guid newGuid)
        {
            throw new NotImplementedException();
        }

        public void SetProgressItems(List<PROGRESS_ITEM> progresses)
        {
            throw new NotImplementedException();
        }

        public void SetReportingDataDate(DateTime dataDate)
        {
            throw new NotImplementedException();
        }
    }

    public static class SUBJOBProjectionQueries
    {
        public static IQueryable<SUBJOBProjection> IDeliverable_Rates_Group_Transformation(
            IQueryable<SUBJOB> SUBJOBS, IEnumerable<BASELINE_ITEM> BASELINE_ITEMS, PROJECT PROJECT, PROGRESS PROGRESS, BASELINE BASELINE,
            IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS, IEnumerable<RATE> RATES, IEnumerable<VARIATION> VARIATIONS)
        {
            IQueryable<BASELINE_ITEMProgress> baseline_rateProjection;
            if (PROGRESS == null)
                baseline_rateProjection = new List<BASELINE_ITEMProgress>().AsQueryable();
            else
                baseline_rateProjection = ProgressQueries.OffsiteDirectProgressItemTransformation(BASELINE_ITEMS.AsQueryable(), PROJECT, PROGRESS, RATES, PROGRESS_ITEMS, VARIATIONS);

            var reportingDate = PROGRESS == null ? new DateTime() : PROGRESS.DATA_DATE;
            return
                SUBJOBS.ToArray().Select(x => new SUBJOBProjection()
                {
                    Entity = x,
                    DeliverableRates = baseline_rateProjection.Where(rateProjection => rateProjection.Subjob_Guid == x.GUID)
                }).AsQueryable();
        }
    }
}
