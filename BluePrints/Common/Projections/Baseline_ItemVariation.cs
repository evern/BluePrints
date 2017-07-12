using BluePrints.Common.Base;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.Common.Projections
{
    public class BASELINE_ITEMVariation : BluePrintsProjectionBase<BASELINE_ITEMProgress>
    {
        public BASELINE_ITEMVariation()
        {
            VARIATION_ITEM = new VARIATION_ITEM();
            VARIATION_ITEM.ACTION = VariationAction.NoAction;
        }

        //variation item cannot be null, because it is used by the view to insert units for saving, also need to retain variation default action
        VARIATION_ITEM variation_item;
        public VARIATION_ITEM VARIATION_ITEM
        {
            get { return variation_item; }
            set { if (value != null) variation_item = value; }
        }

        public DateTime? SubmittedDate { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public bool AdjustUnitsReadOnly => (SubmittedDate != null || Entity.Entity.Entity.BY_DURATION);

        public bool IsSubmitted => SubmittedDate != null;

        public bool IsApproved => ApprovedDate != null;

        public decimal Total_Cost => (Entity.Entity.Total_Units + VARIATION_ITEM.VARIATION_UNITS) * Entity.Entity.ItemRate;

        public decimal Variation_Cost => Forecast_Units * Entity.Entity.ItemRate;

        //use to show what the units will be after approval
        public decimal Forecast_Units
        {
            get
            {
                //When variation item is approved minunits will be 0 because there will be no more value to contra in progress
                if (IsApproved)
                    return VARIATION_ITEM.VARIATION_UNITS;

                if (VARIATION_ITEM.ACTION == VariationAction.Cancel)
                    return MinNegativeUnits;

                return VARIATION_ITEM.VARIATION_UNITS;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                if (IsSubmitted)
                    return true;

                if (GUID == Guid.Empty)
                    return false;

                if (VARIATION_ITEM.ACTION != VariationAction.Add)
                    return true;

                return false;
            }
        }

        public bool IsCancellable
        {
            get
            {
                if (IsSubmitted || IsApproved)
                    return false;

                if (VARIATION_ITEM.ACTION != VariationAction.Add)
                    return true;

                return false;
            }
        }

        public bool IsEnabled
        {
            get
            {
                return !IsReadOnly;
            }
        }

        public decimal MinNegativeUnits
        {
            get
            {
                //when variation is apporved MINUNITS should not cause a warning
                if (IsSubmitted)
                    return -100000;

                if (Entity.PROGRESS_ITEM_BeforeDataDate == null || Entity.Total_Units == 0)
                    return 0;
                if (Entity.PROGRESS_ITEM_Current == null)
                    return -1 * Entity.Total_Units;
                else
                    return -1 * (Entity.Total_Units - Entity.Earned_Units_ToDate);
            }
        }

        public bool CanToggleCancellation
        {
            get { return !IsSubmitted && VARIATION_ITEM.ACTION != VariationAction.Add; }
        }
    }

    public static class Baseline_ItemVariationQuery
    {
        public static IQueryable<BASELINE_ITEMVariation> OffsiteDirectVariationItemTransformation(
            IQueryable<BASELINE_ITEM> BASELINE_ITEMS, PROJECT PROJECT, PROGRESS PROGRESS, IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS, BASELINE BASELINE, VARIATION VARIATION,
            IEnumerable<VARIATION_ITEM> VARIATION_ITEMS, IEnumerable<RATE> RATES)
        {
            IQueryable<BASELINE_ITEM> contextBASELINE_ITEMS;

            if (VARIATION.APPROVED == null)
                //When variation is not approved, retrieve current live deliverables and variation deliverables
                contextBASELINE_ITEMS = BASELINE_ITEMS.Where(x => x.GUID_BASELINE == BASELINE.GUID || x.GUID_VARIATION == VARIATION.GUID && x.GUID_BASELINE == null);
            else
                //When variation is approved, retrieve deliverables from variation connected baseline
                contextBASELINE_ITEMS = BASELINE_ITEMS.Where(x => x.GUID_VARIATION == VARIATION.GUID && x.GUID_BASELINE == VARIATION.GUID_BASELINE);

            //when either live progress or variation doesn't exists don't return anything
            IQueryable<BASELINE_ITEMProgress> Baseline_ItemProgresses;
            if (PROGRESS == null || VARIATION == null)
                Baseline_ItemProgresses = new List<BASELINE_ITEMProgress>().AsQueryable();
            else
                Baseline_ItemProgresses = ProgressItemQueries.OffsiteDirectProgressItemTransformation(contextBASELINE_ITEMS, PROJECT, PROGRESS, RATES, PROGRESS_ITEMS);

            return
                Baseline_ItemProgresses.OrderBy(x => x.Entity.Entity.CREATED).ToArray()
                    .Select(x => new BASELINE_ITEMVariation()
                    {
                        Entity = x,
                        VARIATION_ITEM = VARIATION_ITEMS.Where(y => y.GUID_ORIBASEITEM == x.Entity.Entity.GUID_ORIGINAL).FirstOrDefault(),
                        SubmittedDate = VARIATION.SUBMITTED,
                        ApprovedDate = VARIATION.APPROVED
                    }).AsQueryable();
        }
    }
}