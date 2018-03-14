using BluePrints.Common.ViewModel.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BluePrints.Data;
using DevExpress.Mvvm.POCO;
using BluePrints.Common.Base;

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
                return P6_Assignments.Sum(x => (x.HIGH_VALUE - (x.LOW_VALUE - 0.01m)));
            }
        }

        public virtual decimal P6_Assignment_Total_Quantity => Deliverables == null ? 0 : Deliverables.Count == 0 ? 0 : Deliverables.Sum(x => x.Total_Units);

        public virtual string P6_Assignment_UOM => "Hrs";

        public Guid OriginalEntityKey { get => Entity.GUID; set { } }

        public decimal Budget_Units => Deliverables == null ? 0 : Deliverables.Count == 0 ? 0 : Deliverables.Sum(x => x.Budget_Units);

        public decimal Total_Units => Deliverables == null ? 0 : Deliverables.Count == 0 ? 0 : Deliverables.Sum(x => x.Total_Units);

        public decimal Variation_Units => Deliverables == null ? 0 : Deliverables.Count == 0 ? 0 : Deliverables.Sum(x => x.Variation_Units);

        public Guid? P6_WorkpackGuid => Entity.GUID;

        public string P6AssignmentDescription => Entity.TITLE;

        public string P6AssignmentDescription2 => Entity.DISCIPLINE == null ? string.Empty : Entity.DISCIPLINE.NAME;

        public Guid DeliverableKey => Entity.EntityKey;

        public void SetOriginalEntityKey(Guid newGuid)
        {
        }

        public override void Update()
        {
            this.RaisePropertiesChanged();
            Deliverables.ForEach(x => x.Update());
        }
    }

    public static class WORKPACKQueries
    {
        public static IQueryable<WORKPACKProjection> WORKPACKProjectionOffsiteTransormation(
            IQueryable<BASELINE_ITEM> BASELINE_ITEMS,
            IEnumerable<WORKPACK> WORKPACKS,
            PROJECT PROJECT,
            PROGRESS PROGRESS,
            IEnumerable<RATE> RATES,
            IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS,
            IEnumerable<VARIATION> VARIATIONS = null, bool buildStats = false, IEnumerable<P6_ASSIGNMENT> P6_ASSIGNMENTS = null, bool isInternalNumberAlwaysEditable = false)
        {
            List<BASELINE_ITEMProgress> baseline_item_progresses = ProgressQueries.OffsiteDirectProgressItemTransformation(BASELINE_ITEMS, PROJECT, PROGRESS, RATES, PROGRESS_ITEMS, null, true, null).ToList();

            List<WORKPACKProjection> workpacks = new List<WORKPACKProjection>();
            var progress_item_by_subjobs = baseline_item_progresses.GroupBy(x => x.Entity.Entity.GUID_WORKPACK).Select(group => new { SubjobName = group.Key, Progresses = group.ToList() });
            foreach (WORKPACK workpack in WORKPACKS)
            {
                WORKPACKProjection workpackProjection = new WORKPACKProjection();
                workpackProjection.Entity = workpack;
                workpackProjection.Deliverables = baseline_item_progresses.Where(x => x.Workpack_Guid == workpack.GUID).Select(x => (ICanAssignP6)x).ToList();
                workpackProjection.P6_Assignments = P6_ASSIGNMENTS == null ? null : P6_ASSIGNMENTS.Where(assignment => assignment.GUID_ORIGINAL == workpack.GUID).ToList();
                workpacks.Add(workpackProjection);
            }

            return workpacks.AsQueryable();
        }

        public static IQueryable<WORKPACKProjection> WORKPACKProjectionSiteTransormation(
            IQueryable<ESTIMATE_ITEM> ESTIMATE_ITEMS,
            IEnumerable<WORKPACK> WORKPACKS,
            PROJECT PROJECT,
            PROGRESS PROGRESS,
            IEnumerable<RATE> RATES,
            IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS,
            IEnumerable<STOCK_GROUP> STOCK_GROUPS,
            IEnumerable<STOCK_CODE> STOCK_CODES,
            IEnumerable<VARIATION> VARIATIONS = null, bool buildStats = false, IEnumerable<P6_ASSIGNMENT> P6_ASSIGNMENTS = null, bool isInternalNumberAlwaysEditable = false)
        {
            IEnumerable<ESTIMATE_ITEMProgress> estimation_direct_item_progresses = ESTIMATE_ITEMProjectionQueries.IDeliverable_Progress_Transformation(ESTIMATE_ITEMS, PROJECT, RATES, PROGRESS, PROGRESS_ITEMS, false, STOCK_CODES, STOCK_GROUPS).AsEnumerable();

            List<WORKPACKProjection> workpacks = new List<WORKPACKProjection>();
            var progress_item_by_subjobs = estimation_direct_item_progresses.GroupBy(x => x.Workpack_Guid).Select(group => new { SubjobName = group.Key, Progresses = group.ToList() });
            foreach (WORKPACK workpack in WORKPACKS)
            {
                WORKPACKProjection workpackProjection = new WORKPACKProjection();
                workpackProjection.Entity = workpack;
                workpackProjection.Deliverables = estimation_direct_item_progresses.Where(x => x.Workpack_Guid == workpack.GUID).Select(x => (ICanAssignP6)x).ToList();
                workpackProjection.P6_Assignments = P6_ASSIGNMENTS == null ? null : P6_ASSIGNMENTS.Where(assignment => assignment.GUID_ORIGINAL == workpack.GUID).ToList();
                workpacks.Add(workpackProjection);
            }

            return workpacks.AsQueryable();
        }
    }


}
