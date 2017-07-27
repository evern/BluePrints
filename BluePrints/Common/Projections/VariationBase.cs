using BaseModel.Misc;
using BluePrints.Common.Base;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    public class BluePrintsQuantityVariationBase<TEntity> : BluePrintsVariationBase<TEntity>
        where TEntity : class, IReportable_Quantity, new()
    {
        public decimal Adjust_Quantity
        {
            get
            {
                return variation_item.VARIATION_UNITS * Entity.QuantityPerUnit;
            }
            set
            {
                variation_item.VARIATION_UNITS = value * Entity.QuantityPerUnit;
            }
        }

        public decimal MinNegativeQuantity
        {
            get
            {
                return MinNegativeUnits * Entity.QuantityPerUnit;
            }
        }
    }

    public class BluePrintsVariationBase<TEntity> : BluePrintsProjectionBase<TEntity>
        where TEntity : class, IReportable, new()
    {
        public BluePrintsVariationBase()
        {
            VARIATION_ITEM = new VARIATION_ITEM();
            VARIATION_ITEM.ACTION = VariationAction.NoAction;
        }

        //variation item cannot be null, because it is used by the view to insert units for saving, also need to retain variation default action
        protected VARIATION_ITEM variation_item;
        public VARIATION_ITEM VARIATION_ITEM
        {
            get { return variation_item; }
            set { if (value != null) variation_item = value; }
        }

        public DateTime? SubmittedDate { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public bool IsByDuration
        {
            get
            {
                ISupportByDuration support_by_duration_entity = Entity as ISupportByDuration;
                if (support_by_duration_entity != null)
                {
                    return support_by_duration_entity.IsByDuration;
                }
                else
                    return false;
            }
        }

        public bool AdjustUnitsReadOnly => (IsSubmitted || IsByDuration);

        public bool IsSubmitted => SubmittedDate != null;

        public bool IsApproved => ApprovedDate != null;

        public decimal Total_Units => Entity.Total_Units;

        public decimal Total_Cost => (Entity.Total_Units + VARIATION_ITEM.VARIATION_UNITS) * Entity.ItemRate;

        public decimal Variation_Cost => Forecast_Units * Entity.ItemRate;

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
}
