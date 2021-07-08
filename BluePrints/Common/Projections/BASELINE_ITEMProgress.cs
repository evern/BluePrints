using BaseModel.Attributes;
using BaseModel.Misc;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.XtraEditors.DXErrorProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    [BulkEditDisabledAttributes("DeliverableStatusProgressGuid, DeliverableStatusGuid")]
    public class BASELINE_ITEMProgress : BluePrintsProgressableProjectionBase<BASELINE_ITEMProjection>, ICanAssignP6, ISupportVariation<BASELINE_ITEM>, IHaveDBProductivityOverride, IEntityNumber, IBookable, IDXDataErrorInfo, IHaveTrueP6Dates
    {
        public BASELINE_ITEMProgress()
        {
        }

        public BASELINE_ITEMProgress(PROJECT PROJECT, PROGRESS LivePROGRESS, IDeliverable_Rates entity, IEnumerable<VariationAdjustment> projectVariationAdjustments, bool useReportDate, DateTime? extrapolateDate = null)
            : base(PROJECT, LivePROGRESS, entity, projectVariationAdjustments, useReportDate, extrapolateDate)
        {
        }

        BASELINE_ITEM ISupportVariation<BASELINE_ITEM>.Entity => this.Entity.Entity;
        public Guid GUID_ORIGINAL { get => Entity.GUID_ORIGINAL; set => Entity.GUID_ORIGINAL = value; }

        public VARIATION_ITEM VARIATION_ITEM { get; private set; }

        public void UpdateVariationItem(VARIATION_ITEM variationItem)
        {
            VARIATION_ITEM = variationItem;
            if (VARIATION_ITEM != null)
                uncommittedVariationAction = VARIATION_ITEM.ACTION;
        }

        public VARIATION_ITEM UpdateVariationItem(Guid variationGuid)
        {
            if (VARIATION_ITEM == null)
                VARIATION_ITEM = new VARIATION_ITEM();

            VARIATION_ITEM.GUID_ORIBASEITEM = OriginalEntityKey;
            VARIATION_ITEM.GUID_VARIATION = variationGuid;
            VARIATION_ITEM.VARIATION_UNITS = DisplayVariationUnits;
            VARIATION_ITEM.ACTION = (VariationAction)uncommittedVariationAction;
            return VARIATION_ITEM;
        }

        public DateTime? SubmittedDate { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public bool ShouldSaveVariation => VARIATION_ITEM == null || uncommittedVariationAction != committedVariationAction || uncommittedVariationUnits != committedVariationUnits;

        private decimal? uncommittedVariationUnits;
        // Unapproved variation units, can only be seen in variation view
        public decimal DisplayVariationUnits
        {
            get
            {
                if (uncommittedVariationUnits == null)
                    uncommittedVariationUnits = committedVariationUnits;

                return (decimal)uncommittedVariationUnits;
            }
            set { uncommittedVariationUnits = value; }
        }

        private decimal committedVariationUnits => VARIATION_ITEM == null ? 0 : VARIATION_ITEM.VARIATION_UNITS;

        private VariationAction uncommittedVariationAction;

        public VariationAction DisplayVariationAction { get => committedVariationAction != null ? (VariationAction)committedVariationAction : uncommittedVariationAction; set => uncommittedVariationAction = value; }

        private VariationAction? committedVariationAction => VARIATION_ITEM == null ? (VariationAction?)null : VARIATION_ITEM.ACTION;

        public bool AdjustUnitsReadOnly => DisplayVariationAction == VariationAction.Cancel || (IsSubmitted || IsByDuration);

        public bool IsSubmitted => SubmittedDate != null;

        public bool IsApproved => ApprovedDate != null;

        public decimal DisplayTotalUnits => IsByDuration ? 0 : IsApproved ? (base.Budget_Units + Variation_Units) : (base.Budget_Units + Variation_Units + Forecast_Units);

        public virtual decimal Forecast_Total_Costs => IsByDuration ? 0 : (base.Budget_Units + Variation_Units + Forecast_Units) * Budget_ItemRate;

        public virtual decimal Forecast_Costs => IsByDuration ? 0 : Forecast_Units * base.Entity.Budget_ItemRate;

        public virtual decimal Forecast_Total_InternalCosts => IsByDuration ? 0 : (base.Budget_Units + Variation_Units + Forecast_Units) * Budget_ItemInternalRate;

        public virtual decimal Forecast_InternalCosts => IsByDuration ? 0 : Forecast_Units * base.Entity.Budget_ItemInternalRate;

        public decimal Forecast_Units => DisplayVariationUnits;

        public bool IsReadOnly
        {
            get
            {
                if (IsSubmitted)
                    return true;

                if (GUID == Guid.Empty)
                    return false;

                if (DisplayVariationAction != VariationAction.Add)
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

                if (DisplayVariationAction != VariationAction.Add)
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

                if (base.PROGRESS_ITEM_BeforeDataDate == null || Total_Units == 0)
                    return 0;
                else
                    return -1 * Total_Units;
                //restrict max negative value to account for total earned
                //if (base.PROGRESS_ITEM_Current == null)
                //    return -1 * Total_Units;
                //else
                //    return -1 * (Total_Units - base.Earned_Units_ToDate);
            }
        }

        public bool CanToggleCancellation
        {
            get { return !IsSubmitted && DisplayVariationAction != VariationAction.Add; }
        }

        public override void Update()
        {
            //variation_units = null;
            //variation_action = null;
            base.Update();
        }

        public override string ToString()
        {
            return EntityNumber;
        }

        public DateTime? StartDate
        {
            get
            {
                if (Stats == null || Stats.Budgeted == null || Stats.Budgeted.CumulativeDataPoints == null || Stats.Budgeted.CumulativeDataPoints.Count == 0 || Stats.Budgeted.CumulativeDataPoints.Where(x => x.Units > 0).Count() == 0)
                    return null;

                return Stats.Budgeted.CumulativeDataPoints.Where(x => x.Units > 0).Min(x => x.ProgressDate);
            }
        }

        public DateTime? DueDate
        {
            get
            {
                if (Stats == null || Stats.Budgeted == null || Stats.Budgeted.CumulativeDataPoints == null || Stats.Budgeted.CumulativeDataPoints.Count == 0 || Stats.Budgeted.CumulativeDataPoints.Where(x => x.Units > 0).Count() == 0)
                    return null;

                return Stats.Budgeted.CumulativeDataPoints.Where(x => x.Units > 0).Max(x => x.ProgressDate);
            }
        }

        public DateTime? ForecastDate
        {
            get
            {
                if (Stats == null || Stats.Remaining == null || Stats.Remaining.CumulativeDataPoints == null || Stats.Remaining.CumulativeDataPoints.Count == 0 || Stats.Remaining.CumulativeDataPoints.Where(x => x.Units > 0).Count() == 0)
                    return null;

                return Stats.Remaining.CumulativeDataPoints.Where(x => x.Units > 0).Max(x => x.ProgressDate);
            }
        }

        public decimal FutureMaxPercentage
        {
            get
            {
                if (Total_Units == 0)
                    return 1;

                return ((Total_Units - Earned_Units_AfterDataDate) / Total_Units);
            }
        }

        public decimal GateMaxPercentage
        {
            get
            {
                if (Total_Units == 0)
                    return 1;

                IHaveDeliverableStatus deliverableStatusProjection = Entity as IHaveDeliverableStatus;
                if (deliverableStatusProjection != null && deliverableStatusProjection.Deliverable_Status != null)
                {
                    return deliverableStatusProjection.Deliverable_Status.MAX_PERCENTAGE;
                }

                return 1;
            }
        }

        public bool IsMaxPercentageRestrictedByFuturePercentage
        {
            get
            {
                if (Total_Units == 0)
                    return true;

                if (GateMaxPercentage < FutureMaxPercentage)
                    return false;
                else
                    return true;
            }
        }

        public override decimal MaxPercentage
        {
            get
            {
                if (Total_Units == 0)
                    return 1;

                if (FutureMaxPercentage < GateMaxPercentage)
                    return FutureMaxPercentage;
                else
                    return GateMaxPercentage;
            }
        }

        public bool CanBook { get; set; }

        public bool IsInternalNumberEditable
        {
            get
            {
                if (Entity.Entity.INTERNALNUM_STATUS == DocumentNumberStatus.Approved || Entity.Entity.INTERNALNUM_STATUS == DocumentNumberStatus.Awaiting)
                    return false;
                else
                    return IsInternalNumberAlwaysEditable ? true : Earned_Units_ToDate == 0;
            }
        }

        public bool IsClientNumberEditable
        {
            get
            {
                if (Entity.Entity.CLIENTNUM_STATUS == DocumentNumberStatus.Approved || Entity.Entity.CLIENTNUM_STATUS == DocumentNumberStatus.Awaiting)
                    return false;
                else
                    return true;
            }
        }

        //used by variation to remember which id was it duplicate from because duplicate() is called from BaselineItemProgress and doesn't have the variation informatin
        public Guid? DuplicateFromGuid { get; set; }

        public bool IsInternalNumberAlwaysEditable { get; set; }

        public bool IsInternalNumberManualOnly { get; set; }

        public DateTime? InternalNumLockDate { get; set; }

        public DateTime? ClientNumLockDate { get; set; }

        public Guid? GUID_VARIATION { get => Entity.GUID_VARIATION; set => Entity.GUID_VARIATION = value; }

        public Guid? GUID_BASELINE { get => Entity.GUID_BASELINE; set => Entity.GUID_BASELINE = value; }

        public decimal? DB_Productivity_Override { get => Entity.DB_Productivity_Override; set => Entity.DB_Productivity_Override = value; }

        public string EntityNumber { get => Entity.Entity.INTERNAL_NUM; set => Entity.Entity.INTERNAL_NUM = value; }

        public string EntityGroup => Entity.EntityGroup;

        public bool ShouldSave { get; set; }

        public Guid? DeliverableStatusProgressGuid
        {
            get
            {
                return Entity.Entity.DeliverableStatusGuid;
            }
            set
            {
                Guid? setValue = (Guid?)value;
                if (setValue == null)
                    Entity.Entity.GUID_STATUS = null;
                else if (Entity.Entity.IsDeliverableStatusValid(setValue))
                {
                    Entity.Entity.GUID_STATUS = setValue;

                    if (setValue != null)
                    {
                        Guid current_deliverable_status_guid = (Guid)setValue;
                        DELIVERABLES_STATUS current_deliverable_status = Entity.Entity.DeliverableStatusCollection.FirstOrDefault(x => x.GUID == current_deliverable_status_guid);
                        if (current_deliverable_status != null && current_deliverable_status.AUTO_PERCENTAGE != null)
                        {
                            decimal auto_percentage = (decimal)current_deliverable_status.AUTO_PERCENTAGE;
                            if (current_deliverable_status.AUTO_PERCENTAGE > Total_Percentage)
                                Total_Earned_Percentage = auto_percentage;
                        }
                    }
                }
            }
        }

        public void GetPropertyError(string propertyName, ErrorInfo info)
        {
            if (propertyName.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEM().DOCTYPE)))
            {
                if(Entity.Entity.IsDocumentTypeValid != BASELINE_ITEM.DocumentTypeValidStatus.Valid)
                {
                    string errorText = "Document type";
                    if (Entity.Entity.DOCTYPE != null)
                        errorText += " " + Entity.Entity.DOCTYPE.CODE;

                    string disciplineCode = Entity.Entity.DISCIPLINE == null ? string.Empty : Entity.Entity.DISCIPLINE.CODE;
                    errorText += " ";
                    if (Entity.Entity.IsDocumentTypeValid == BASELINE_ITEM.DocumentTypeValidStatus.NotValidByCommodityCode)
                        info.ErrorText = errorText + "is not valid for discipline of " + disciplineCode;
                }
            }
            else if(propertyName.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEM().DELIVERABLE_TYPE)))
            {
                if(!Entity.Entity.IsDeliverableTypeValid)
                {
                    info.ErrorText = Entity.Entity.DELIVERABLE_TYPE.ToString() + " is not valid";
                    if (Entity.Entity.DOCTYPE != null)
                        info.ErrorText += " for doc type of " + Entity.Entity.DOCTYPE.CODE;
                }
            }
        }

        public void GetError(ErrorInfo info)
        {
        }

        #region User Report
        public string User_Name { get; set; }
        public string User_Role { get; set; }
        #endregion

        #region IHaveTrueP6Dates
        public DateTime? ViewStartDate => TrueP6PlannedStartDate == null ? StartDate : TrueP6PlannedStartDate;
        public DateTime? ViewDueDate => TrueP6PlannedEndDate == null ? DueDate : TrueP6PlannedEndDate;
        public DateTime? ViewForecastDate => TrueP6RemainingEndDate == null ? ForecastDate : TrueP6RemainingEndDate;
        public DateTime? TrueP6PlannedStartDate { get; set; }
        public DateTime? TrueP6PlannedEndDate { get; set; }
        public DateTime? TrueP6RemainingEndDate { get; set; }
        #endregion
    }
}
