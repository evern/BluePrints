using BluePrints.Common.ViewModel.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BluePrints.Data;
using DevExpress.Mvvm.POCO;
using BluePrints.Common.Base;
using BluePrints.P6Data;

namespace BluePrints.Common.Projections
{
    public class WORKPACKProjection : BluePrintsProjectionBase<WORKPACK>, ICanAssignP6
    {
        public string Name => Entity.NAME;
        public List<ICanAssignP6> Deliverables { get; set; }
        public IEnumerable<PROGRESS_ITEM> Progresses => throw new NotImplementedException();
        public decimal EarnedUnitsAccountedFor { get; set; }
        public DateTime? TaskAssignmentStartDate { get; set; }

        private List<P6_ASSIGNMENT> p6_assignments;
        public List<P6_ASSIGNMENT> P6_Assignments
        {
            get
            {
                if (p6_assignments == null)
                    p6_assignments = new List<P6_ASSIGNMENT>();

                return p6_assignments;
            }
            set
            {
                p6_assignments = value;
            }
        }

        public bool IHaveP6AssignmentProjection => P6_Assignment_Projection != null && P6_Assignment_Projection.Count > 0;

        private List<P6_AssignmentProjection> p6_assignment_projection;
        public List<P6_AssignmentProjection> P6_Assignment_Projection
        {
            get
            {
                if (p6_assignment_projection == null)
                    p6_assignment_projection = new List<P6_AssignmentProjection>();

                return p6_assignment_projection;
            }
            set
            {
                p6_assignment_projection = value;
            }
        }


        public string P6AssignmentName => Entity.NAME;


        public decimal Remaining_Percentage
        {
            get
            {
                return 1 - Assigned_Percentage;
            }
        }

        public decimal Assigned_Percentage
        {
            get
            {
                return P6_Assignments.Sum(x => (x.HIGH_VALUE - (x.LOW_VALUE - 0.0001m)));
            }
        }

        public virtual decimal P6_Assignment_Total_Quantity => Deliverables == null ? 0 : Deliverables.Count == 0 ? 0 : Deliverables.Sum(x => x.Total_Units);

        public virtual string P6_Assignment_UOM => "Hrs";

        public Guid OriginalEntityKey { get => Entity.GUID; set { } }

        public decimal Budget_Units => Deliverables == null ? 0 : Deliverables.Count == 0 ? 0 : Deliverables.Sum(x => x.Budget_Units);

        public decimal Total_Units => Deliverables == null ? 0 : Deliverables.Count == 0 ? 0 : Deliverables.Sum(x => x.Total_Units);

        public string Variation_Code => string.Empty;

        public decimal Variation_Units => Deliverables == null ? 0 : Deliverables.Count == 0 ? 0 : Deliverables.Sum(x => x.Variation_Units);

        public Guid? P6_WorkpackGuid => Entity.GUID;

        public string P6AssignmentDescription => Entity.TITLE;

        public string P6AssignmentDescription2 => Entity.DISCIPLINE == null ? string.Empty : Entity.DISCIPLINE.NAME;

        public Guid DeliverableKey => Entity.GUID;

        public decimal Budget_Quantity => Deliverables == null ? 0 : Deliverables.Count == 0 ? 0 : Deliverables.Sum(x => x.Budget_Quantity);

        public decimal Total_Quantity => Deliverables == null ? 0 : Deliverables.Count == 0 ? 0 : Deliverables.Sum(x => x.Total_Quantity);

        public decimal Earned_Units_ToDate => throw new NotImplementedException();

        public decimal Variation_Costs => Deliverables == null ? 0 : Deliverables.Count == 0 ? 0 : Deliverables.Sum(x => x.Variation_Costs);

        public decimal Total_Costs => Deliverables == null ? 0 : Deliverables.Count == 0 ? 0 : Deliverables.Sum(x => x.Total_Costs);

        public decimal Budget_Adjustment_Units => Deliverables.Count == 0 ? 0 : Deliverables.Sum(x => x.Budget_Adjustment_Units);

        public decimal Budget_Adjustment_Costs => Deliverables.Count == 0 ? 0 : Deliverables.Sum(x => x.Budget_Adjustment_Costs);

        public decimal Variation_InternalCosts => Deliverables == null ? 0 : Deliverables.Count == 0 ? 0 : Deliverables.Sum(x => x.Variation_InternalCosts);

        public decimal Total_InternalCosts => Deliverables == null ? 0 : Deliverables.Count == 0 ? 0 : Deliverables.Sum(x => x.Total_InternalCosts);

        public decimal Unadjusted_Budget_Units => Deliverables == null ? 0 : Deliverables.Count == 0 ? 0 : Deliverables.Sum(x => x.Unadjusted_Budget_Units);

        public List<VariationAdjustment> ApprovedVariations => Deliverables == null ? new List<VariationAdjustment>() : Deliverables.Count == 0 ? new List<VariationAdjustment>() : Deliverables.SelectMany(x => x.ApprovedVariations).ToList();

        public decimal UnitsPerQuantity => 1;

        public string UOM => "h";

        public decimal Variation_Quantity => Variation_Units / UnitsPerQuantity;

        public void SetOriginalEntityKey(Guid newGuid)
        {
        }

        public override void Update()
        {
            this.RaisePropertiesChanged();
            Deliverables.ForEach(x => x.Update());
        }

        public void BuildStats(decimal weightingPortion = 1, List<StatsCalculationType> calcTypes = null)
        {

        }
    }

    public static class WORKPACKQueries
    {
        public static IQueryable<WORKPACKProjection> WORKPACKProjectionSiteAndOffsiteTransformation(
            IQueryable<WORKPACK> WORKPACKS,
            IEnumerable<BASELINE_ITEM> BASELINE_ITEMS,
            IEnumerable<ESTIMATE_ITEM> ESTIMATE_ITEMS,
            IEnumerable<P6_ASSIGNMENT> P6_ASSIGNMENTS,
            IEnumerable<RATE> RATES,
            IEnumerable<VARIATION> VARIATIONS, 
            IEnumerable<PROGRESS> PROGRESSES, 
            IEnumerable<TASK> P6TASKS,
            Data.PROJECT PROJECT
            )
        {
            PROGRESS designPROGRESS = PROGRESSES.FirstOrDefault(x => x.TYPE == PhaseType.Design);
            PROGRESS constructPROGRESS = PROGRESSES.FirstOrDefault(x => x.TYPE == PhaseType.Construct);

            List<BASELINE_ITEMProgress> baseline_item_progresses = new List<BASELINE_ITEMProgress>();
            List<ESTIMATE_ITEMProgress> estimation_direct_item_progresses = new List<ESTIMATE_ITEMProgress>();
            if (designPROGRESS != null)
                baseline_item_progresses = ProgressQueries.OffsiteDirectProgressItemTransformation(BASELINE_ITEMS.AsQueryable(), PROJECT, designPROGRESS, RATES, designPROGRESS.PROGRESS_ITEM, VARIATIONS, true, null).ToList();

            if(constructPROGRESS != null)
                estimation_direct_item_progresses = ESTIMATE_ITEMProjectionQueries.IDeliverable_Progress_Transformation(ESTIMATE_ITEMS.AsQueryable(), PROJECT, RATES, constructPROGRESS, constructPROGRESS.PROGRESS_ITEM.ToList(), false).ToList();

            List<WORKPACKProjection> workpacks = new List<WORKPACKProjection>();
            foreach (WORKPACK workpack in WORKPACKS)
            {
                WORKPACKProjection workpackProjection = new WORKPACKProjection();
                workpackProjection.Entity = workpack;

                workpackProjection.Deliverables = baseline_item_progresses.Where(x => x.Workpack_Guid == workpack.GUID).Select(x => (ICanAssignP6)x).ToList();
                workpackProjection.Deliverables.AddRange(estimation_direct_item_progresses.Where(x => x.Workpack_Guid == workpack.GUID).Select(x => (ICanAssignP6)x).ToList());

                List<P6_AssignmentProjection> P6_AssignmentProjection = new List<Common.P6_AssignmentProjection>();
                List<P6_ASSIGNMENT> p6_assignments = P6_ASSIGNMENTS == null ? null : P6_ASSIGNMENTS.Where(assignment => assignment.GUID_ORIGINAL == workpack.GUID).ToList();
                if(p6_assignments != null)
                    foreach(P6_ASSIGNMENT p6_assignment in p6_assignments)
                    {
                        P6_AssignmentProjection projection = new P6_AssignmentProjection(workpackProjection, p6_assignment, false);
                        projection.P6_TASK = P6TASKS.FirstOrDefault(x => x.task_code == p6_assignment.P6_ACTIVITYID);
                        P6_AssignmentProjection.Add(projection);
                    }

                workpackProjection.P6_Assignment_Projection = P6_AssignmentProjection;
                workpacks.Add(workpackProjection);
            }

            return workpacks.AsQueryable();
        }


        public static IQueryable<WORKPACKProjection> WORKPACKProjectionOffsiteTransformation(
            IQueryable<BASELINE_ITEM> BASELINE_ITEMS,
            IEnumerable<WORKPACK> WORKPACKS,
            Data.PROJECT PROJECT,
            PROGRESS PROGRESS,
            IEnumerable<RATE> RATES,
            IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS,
            IEnumerable<VARIATION> VARIATIONS, bool buildStats = false, IEnumerable<P6_ASSIGNMENT> P6_ASSIGNMENTS = null, bool isInternalNumberAlwaysEditable = false)
        {
            List<BASELINE_ITEMProgress> baseline_item_progresses = ProgressQueries.OffsiteDirectProgressItemTransformation(BASELINE_ITEMS, PROJECT, PROGRESS, RATES, PROGRESS_ITEMS, VARIATIONS, true, null).ToList();

            List<WORKPACKProjection> workpacks = new List<WORKPACKProjection>();
            var progress_item_by_subjobs = baseline_item_progresses.GroupBy(x => x.Entity.Entity.GUID_WORKPACK).Select(group => new { SubjobName = group.Key, Progresses = group.ToList() });
            foreach (WORKPACK workpack in WORKPACKS)
            {
                WORKPACKProjection workpackProjection = new WORKPACKProjection();
                workpackProjection.Entity = workpack;

                List<ICanAssignP6> deliverables = baseline_item_progresses.Where(x => x.Workpack_Guid == workpack.GUID).Select(x => (ICanAssignP6)x).ToList();
                if (workpackProjection.Deliverables != null)
                    workpackProjection.Deliverables.AddRange(deliverables);   
                else
                    workpackProjection.Deliverables = baseline_item_progresses.Where(x => x.Workpack_Guid == workpack.GUID).Select(x => (ICanAssignP6)x).ToList();

                workpackProjection.P6_Assignments = P6_ASSIGNMENTS == null ? null : P6_ASSIGNMENTS.Where(assignment => assignment.GUID_ORIGINAL == workpack.GUID).ToList();
                workpacks.Add(workpackProjection);
            }

            return workpacks.AsQueryable();
        }

        public static IQueryable<WORKPACKProjection> WORKPACKProjectionSiteTransormation(
            IQueryable<ESTIMATE_ITEM> ESTIMATE_ITEMS,
            IEnumerable<WORKPACK> WORKPACKS,
            Data.PROJECT PROJECT,
            PROGRESS PROGRESS,
            IEnumerable<RATE> RATES,
            IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS,
            IEnumerable<VARIATION> VARIATIONS = null, bool buildStats = false, IEnumerable<P6_ASSIGNMENT> P6_ASSIGNMENTS = null, bool isInternalNumberAlwaysEditable = false)
        {
            IEnumerable<ESTIMATE_ITEMProgress> estimation_direct_item_progresses = ESTIMATE_ITEMProjectionQueries.IDeliverable_Progress_Transformation(ESTIMATE_ITEMS, PROJECT, RATES, PROGRESS, PROGRESS_ITEMS, false).AsEnumerable();

            List<WORKPACKProjection> workpacks = new List<WORKPACKProjection>();
            var progress_item_by_subjobs = estimation_direct_item_progresses.GroupBy(x => x.Workpack_Guid).Select(group => new { SubjobName = group.Key, Progresses = group.ToList() });
            foreach (WORKPACK workpack in WORKPACKS)
            {
                WORKPACKProjection workpackProjection = new WORKPACKProjection();
                workpackProjection.Entity = workpack;

                List<ICanAssignP6> deliverables = estimation_direct_item_progresses.Where(x => x.Workpack_Guid == workpack.GUID).Select(x => (ICanAssignP6)x).ToList();
                if (workpackProjection.Deliverables != null)
                    workpackProjection.Deliverables.AddRange(deliverables);
                else
                    workpackProjection.Deliverables = estimation_direct_item_progresses.Where(x => x.Workpack_Guid == workpack.GUID).Select(x => (ICanAssignP6)x).ToList();

                workpackProjection.P6_Assignments = P6_ASSIGNMENTS == null ? null : P6_ASSIGNMENTS.Where(assignment => assignment.GUID_ORIGINAL == workpack.GUID).ToList();
                workpacks.Add(workpackProjection);
            }

            return workpacks.AsQueryable();
        }
    }


}
