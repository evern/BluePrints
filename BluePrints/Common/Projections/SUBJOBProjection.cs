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

        public decimal? Override_Productivity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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

        public Guid? Subjob_Guid { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Guid? Area_Guid => throw new NotImplementedException();

        public Guid? SubArea_Guid => throw new NotImplementedException();

        public Guid? Discipline_Guid => throw new NotImplementedException();

        public decimal Discipline_Number => throw new NotImplementedException();

        public Guid? Workpack_Guid { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsByDuration { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Guid OriginalEntityKey => throw new NotImplementedException();

        public string Commodity_Code => throw new NotImplementedException();

        public decimal Budget_Units => Reportables == null ? 0 : Reportables.Sum(x => x.Budget_Units);

        public decimal Budget_Quantity => throw new NotImplementedException();

        public decimal Total_Quantity => throw new NotImplementedException();

        public decimal Budget_ItemRate => Reportables == null ? 0 : Reportables.Sum(x => x.Budget_ItemRate);

        public decimal Budget_Costs => Reportables == null ? 0 : Reportables.Sum(x => x.Budget_Costs);

        public ProgressStats Stats { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_BeforeDataDate => throw new NotImplementedException();

        public PROGRESS_ITEM PROGRESS_ITEM_Current => throw new NotImplementedException();

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_UpToCurrentDataDate => throw new NotImplementedException();

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_AfterDataDate => throw new NotImplementedException();

        public DateTime ReportingDataDate => throw new NotImplementedException();

        public List<PROGRESS_ITEM> PROGRESS_ITEMS => throw new NotImplementedException();

        public decimal Earned_Units_Total => throw new NotImplementedException();

        public decimal Earned_Costs_Total => throw new NotImplementedException();

        public decimal Earned_Units_BeforeDataDate => throw new NotImplementedException();

        public decimal Earned_Units_OnDataDate => throw new NotImplementedException();

        public decimal Earned_Units_ToDate => throw new NotImplementedException();

        public decimal Earned_Costs_ToDate => throw new NotImplementedException();

        public decimal Earned_Costs_OnDataDate => throw new NotImplementedException();

        public decimal Earned_Units_AfterDataDate => throw new NotImplementedException();

        public decimal Total_Earned_Percentage { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public decimal Total_Percentage => throw new NotImplementedException();

        public decimal Total_Percentage_ToDate => throw new NotImplementedException();

        public decimal Baseline_Percentage => throw new NotImplementedException();

        public decimal SchedulePercentage => throw new NotImplementedException();

        public decimal ScheduleCurrentPeriodPercentage => throw new NotImplementedException();

        public decimal MinPercentage => throw new NotImplementedException();

        public decimal MaxPercentage => throw new NotImplementedException();

        public bool ShouldSaveProgress => throw new NotImplementedException();

        public decimal MinEstimateUnits => throw new NotImplementedException();

        public decimal Variation_Units => throw new NotImplementedException();

        public decimal Variation_Costs => throw new NotImplementedException();

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
