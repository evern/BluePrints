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
    public class BluePrintsQuantityVariationBase<TEntity> : BluePrintsVariationBase<TEntity>, IBluePrintsQuantityVariationBase<TEntity>, IVariation_Quantity
        where TEntity : class, IReportable_Quantity, ISupportVariation, new()
    {
        public decimal Quantity { get => Variation_Units * Entity.QuantityPerUnit; set => Variation_Units = value * Entity.QuantityPerUnit; }

        public decimal MinNegativeQuantity
        {
            get
            {
                return MinNegativeUnits * Entity.QuantityPerUnit;
            }
        }

        public decimal Variation_Install_Cost => Variation_Cost;

        public decimal Variation_Supply_Cost => Variation_Units * Entity.Stock_Code_Supply_Rate;

        public decimal Variation_Install_Hours => Variation_Units * Entity.Stock_Code_Install_Hours;

        public decimal Total_Install_Hours => Total_Units * Entity.Stock_Code_Install_Hours;

        public decimal Total_Install_Cost => Total_Install_Hours * base.Entity.ItemRate;

        public decimal Total_Supply_Cost => Total_Units * Entity.Stock_Code_Supply_Rate;

        public override decimal Total_Cost => Total_Install_Cost + Total_Supply_Cost;
    }

    public class BluePrintsVariationBase<TEntity> : BluePrintsProjectionBase<TEntity>, IBluePrintsVariationBase<TEntity>
        where TEntity : class, IReportable, ISupportVariation, new()
    {
        public BluePrintsVariationBase()
        {
        }

        public VARIATION_ITEM VARIATION_ITEM { get; set; }

        public DateTime? SubmittedDate { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public bool ShouldSaveVariation => (VARIATION_ITEM == null && Variation_Action != VariationAction.NoAction) || (VARIATION_ITEM != null && (VARIATION_ITEM.GUID == Guid.Empty || VARIATION_ITEM.ACTION != Variation_Action || VARIATION_ITEM.VARIATION_UNITS != Variation_Units));

        decimal? variation_units;
        public decimal Variation_Units
        {
            get
            {
                if (variation_units == null)
                    variation_units = get_actual_variation_units();

                return (decimal)variation_units;
            }
            set { variation_units = value; }
        }

        private decimal get_actual_variation_units()
        {
            if (VARIATION_ITEM == null)
                return 0;

            return VARIATION_ITEM.VARIATION_UNITS;
        }

        VariationAction? variation_action;
        public VariationAction Variation_Action
        {
            get
            {
                if (variation_action == null)
                    variation_action = get_actual_variation_action();

                return (VariationAction)variation_action;
            }
            set { variation_action = value; }
        }

        private VariationAction get_actual_variation_action()
        {
            if (VARIATION_ITEM == null)
            {
                if (base.Entity.Baseline_Guid == null)
                    return VariationAction.Add;
                else
                    return VariationAction.NoAction;
            }

            return VARIATION_ITEM.ACTION;
        }

        public bool IsByDuration
        {
            get
            {
                ISupportByDuration support_by_duration_entity = base.Entity as ISupportByDuration;
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

        public decimal Total_Units => IsByDuration ? 0 : (base.Entity.Total_Units + Variation_Units);

        public virtual decimal Total_Cost => IsByDuration ? 0 : (base.Entity.Total_Units + Variation_Units) * base.Entity.ItemRate;

        public decimal Variation_Cost => IsByDuration ? 0 : Forecast_Units * base.Entity.ItemRate;

        //use to show what the units will be after approval
        public decimal Forecast_Units
        {
            get
            {
                //When variation item is approved minunits will be 0 because there will be no more value to contra in progress
                if (IsApproved)
                    return Variation_Units;

                if (Variation_Action == VariationAction.Cancel)
                    return MinNegativeUnits;

                return Variation_Units;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                if (IsSubmitted)
                    return true;

                if (EntityKey == Guid.Empty)
                    return false;

                if (Variation_Action != VariationAction.Add)
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

                if (Variation_Action != VariationAction.Add)
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
                if (IsApproved)
                    return -100000;

                if (base.Entity.PROGRESS_ITEM_BeforeDataDate == null || base.Entity.Total_Units == 0)
                    return 0;
                if (base.Entity.PROGRESS_ITEM_Current == null)
                    return -1 * base.Entity.Total_Units;
                else
                    return -1 * (base.Entity.Total_Units - base.Entity.Earned_Units_ToDate);
            }
        }

        public bool CanToggleCancellation
        {
            get { return !IsSubmitted && Variation_Action != VariationAction.Add; }
        }

        public Guid OriginalEntityKey => base.Entity.OriginalEntityKey;

        /// <summary>
        /// projection will not be saved
        /// </summary>
        public void SetOriginalEntityKey(Guid newGuid)
        {
            throw new NotImplementedException();
        }

        public override void Update()
        {
            variation_units = null;
            variation_action = null;
            base.Update();
        }
    }

    public interface IBluePrintsQuantityVariationBase<TEntity> : IBluePrintsVariationBase<TEntity>
        where TEntity : class, IReportable_Quantity, new()
    {
        decimal Quantity { get; set; }
        decimal MinNegativeQuantity { get; }
    }

    public interface IBluePrintsVariationBase<TEntity> : IGuidEntityKey, IOriginalGuidEntityKey, ICanUpdate
        where TEntity : class, IReportable, new()
    {
        TEntity Entity { get; set; }
        VARIATION_ITEM VARIATION_ITEM { get; set; }
        DateTime? SubmittedDate { get; set; }
        DateTime? ApprovedDate { get; set; }
        bool IsByDuration { get; }
        bool AdjustUnitsReadOnly { get; }
        bool IsSubmitted { get; }
        bool IsApproved { get; }
        decimal Total_Units { get; }
        decimal Total_Cost { get; }
        decimal Variation_Cost { get; }
        //use to show what the units will be after approval
        decimal Forecast_Units { get; }
        bool IsReadOnly { get; }
        bool IsCancellable { get; }
        bool IsEnabled { get; }
        decimal MinNegativeUnits { get; }
        bool CanToggleCancellation { get; }
        decimal Variation_Units { get; set; }
        VariationAction Variation_Action { get; set; }
        bool ShouldSaveVariation { get; }
    }
}
