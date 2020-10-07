namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common;
    using BluePrints.Common.Base;
    using BluePrints.Common.Projections;
    using BluePrints.Common.Resources;
    using DevExpress.Mvvm;
    using DevExpress.XtraEditors.DXErrorProvider;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class VARIATION_CONSTRUCTION : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate, IDXDataErrorInfo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public VARIATION_CONSTRUCTION()
        {
            VARIATION_CONSTRUCTION_ITEM = new HashSet<VARIATION_CONSTRUCTION_ITEM>();
            VARIATION_CONSTRUCTION_IMPACT = new HashSet<VARIATION_CONSTRUCTION_IMPACT>();
            SUBMISSION_DATE = DateTime.Now;
        }

        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }


        [NotMapped]
        private IEnumerable<object> multipleAssignedImpactObject;

        [NotMapped]
        public object MultipleAssignedImpactObject
        {
            get { return multipleAssignedImpactObject; }
            set
            {
                if (value != multipleAssignedImpactObject)
                {
                    multipleAssignedImpactObject = value as IEnumerable<object>;
                }
            }
        }

        [NotMapped]
        public string DocumentNumber => PROJECT.NUMBER + "-VAR-PM-" + NUMBER;

        public List<VARIATION_CONSTRUCTION_IMPACT> GetAssignedImpacts()
        {
            List<VARIATION_CONSTRUCTION_IMPACT> tempAssignedImpacts;
            if (MultipleAssignedImpactObject == null)
                tempAssignedImpacts = null;
            else if (MultipleAssignedImpactObject.GetType() == typeof(List<VARIATION_CONSTRUCTION_IMPACT>))
                tempAssignedImpacts = (List<VARIATION_CONSTRUCTION_IMPACT>)MultipleAssignedImpactObject;
            else
                tempAssignedImpacts = ((List<object>)MultipleAssignedImpactObject).Select(x => (VARIATION_CONSTRUCTION_IMPACT)x).ToList();

            return tempAssignedImpacts;
        }

        [NotMapped]
        public IEnumerable<VARIATION_CONSTRUCTION_IMPACT> MultipleAssignedImpacts
        {
            get
            {
                if (multipleAssignedImpactObject == null)
                    return null;

                return multipleAssignedImpactObject.Select(x => (VARIATION_CONSTRUCTION_IMPACT)x);
            }
        }

        public void SetAssignedImpacts(IEnumerable<VARIATION_CONSTRUCTION_IMPACT> AllVARIATION_CONSTRUCTION_IMPACTCollection, IEnumerable<VARIATION_CONSTRUCTION_IMPACT> VARIATION_CONSTRUCTION_IMPACTCollection)
        {
            MultipleAssignedImpactObject = AllVARIATION_CONSTRUCTION_IMPACTCollection.Where(allImpact => VARIATION_CONSTRUCTION_IMPACTCollection.Any(assignedImpact => allImpact.IMPACT == assignedImpact.IMPACT)).ToList();
        }

        [NotMapped]
        private List<VARIATION_CONSTRUCTION_ITEM> updatedVARIATION_CONSTRUCTION_ITEMS = null;
        public void UpdateVariationConstructionItems(IEnumerable<VARIATION_CONSTRUCTION_ITEM> VARIATION_CONSTRUCTION_ITEMS)
        {
            updatedVARIATION_CONSTRUCTION_ITEMS = VARIATION_CONSTRUCTION_ITEMS.ToList();
        }

        [NotMapped]
        public IEnumerable<VARIATION_CONSTRUCTION_ITEM> UpdatableVariationConstructionItems
        {
            get
            {
                //cannot use this because once VARIATION_CONSTRUCTION_ITEM is loaded and this is deleted EF will try to set FK as null
                //if (updatedVARIATION_CONSTRUCTION_ITEMS == null)
                //    return VARIATION_CONSTRUCTION_ITEM;

                return updatedVARIATION_CONSTRUCTION_ITEMS;
            }
        }

        [NotMapped]
        public decimal ManagementTotal
        {
            get
            {
                if (UpdatableVariationConstructionItems == null)
                    return 0;

                return UpdatableVariationConstructionItems.Where(x => x.TYPE == VariationConstructionItemType.Management).Sum(x => x.TotalCosts);
            }
        }

        [NotMapped]
        public decimal EngineeringTotal
        {
            get
            {
                if (UpdatableVariationConstructionItems == null)
                    return 0;

                return UpdatableVariationConstructionItems.Where(x => x.TYPE == VariationConstructionItemType.Engineering).Sum(x => x.TotalCosts);
            }
        }

        [NotMapped]
        public decimal TradesAndLabourTotal
        {
            get
            {
                if (UpdatableVariationConstructionItems == null)
                    return 0;

                return UpdatableVariationConstructionItems.Where(x => x.TYPE == VariationConstructionItemType.TradesAndLabour).Sum(x => x.TotalCosts);
            }
        }

        [NotMapped]
        public decimal EquipmentTotal
        {
            get
            {
                if (UpdatableVariationConstructionItems == null)
                    return 0;

                return UpdatableVariationConstructionItems.Where(x => x.TYPE == VariationConstructionItemType.Equipment).Sum(x => x.TotalCosts);
            }
        }

        [NotMapped]
        public decimal MaterialTotal
        {
            get
            {
                if (UpdatableVariationConstructionItems == null)
                    return 0;

                return UpdatableVariationConstructionItems.Where(x => x.TYPE == VariationConstructionItemType.Material).Sum(x => x.TotalCosts);
            }
        }

        [NotMapped]
        public decimal TotalEstimatedValue => ManagementTotal + EngineeringTotal + TradesAndLabourTotal + EquipmentTotal + MaterialTotal;

        [NotMapped]
        public string Office => this.PROJECT.NUMBER + " " + this.PROJECT.OfficeName;

        [NotMapped]
        public bool FlagManualApprovedPct => ManualApprovedEstimatedValue > CalculatedApprovedEstimatedValue;

        [NotMapped]
        public decimal CalculatedApprovedEstimatedValue => CalculatedRiskAssessedPercentage * APPROVED_VALUE;

        [NotMapped]
        public decimal ManualApprovedEstimatedValue
        {
            get
            {
                if (STATUS >= VariationConstructionStatus.Submitted)
                    return APPROVE_PERCENTAGE * APPROVED_VALUE;

                return 0;
            }
        }

        [NotMapped]
        public decimal CalculatedRiskAssessedPercentage
        {
            get
            {
                decimal variationClientAcknowledgementPct = GetVariationCategoriesPercentage(CLIENT_ACKNOWLEDGEMENT);
                decimal variationNaturePct = GetVariationCategoriesPercentage(NATURE);
                decimal variationSubstantiationPct = GetVariationCategoriesPercentage(SUBSTANTIATION);
                decimal variationTimelinessPct = GetVariationCategoriesPercentage(TIMELINESS);
                decimal variationDiscretionaryPct = GetVariationCategoriesPercentage(DISCRETIONARY_ADJUSTMENT);

                decimal variationClientAcknowledgementWeightage = variationClientAcknowledgementPct * 0.35m;
                decimal variationNatureWeightage = variationNaturePct * 0.20m;
                decimal variationSubstantiationWeightage = variationSubstantiationPct * 0.20m;
                decimal variationTimelinessWeightage = variationTimelinessPct * 0.20m;
                decimal variationDiscretionaryWeightage = variationDiscretionaryPct * 0.05m;

                return variationClientAcknowledgementWeightage + variationNatureWeightage + variationSubstantiationWeightage + variationTimelinessWeightage + variationDiscretionaryWeightage;
            }
        }

        public decimal GetVariationCategoriesPercentage(VariationClientAcknowledgement variationClientAcknowledgement)
        {
            if (variationClientAcknowledgement == VariationClientAcknowledgement.None)
                return 0;
            else if (variationClientAcknowledgement == VariationClientAcknowledgement.Verbal)
                return 0.05m;
            else if (variationClientAcknowledgement == VariationClientAcknowledgement.Email)
                return 0.2m;
            else //written
                return 0.9m;
        }

        public decimal GetVariationCategoriesPercentage(VariationNature variationNature)
        {
            if (variationNature == VariationNature.ConstructiveAcceleration)
                return 0.1m;
            else if (variationNature == VariationNature.Distruption)
                return 0.2m;
            else if (variationNature == VariationNature.CriticalDelay)
                return 0.4m;
            else if (variationNature == VariationNature.DirectedAcceleration)
                return 0.5m;
            else //variation under contract
                return 0.9m;
        }

        public decimal GetVariationCategoriesPercentage(VariationSubstantiation variationSubstantiation)
        {
            if (variationSubstantiation == VariationSubstantiation.SpreadsheetsOnly)
                return 0.05m;
            else if (variationSubstantiation == VariationSubstantiation.SubmittedContemporaryRecords)
                return 0.2m;
            else if (variationSubstantiation == VariationSubstantiation.SignedDayworks)
                return 0.6m;
            else //SubmittedContemporaryRecordsDayworks
                return 0.9m;
        }

        public decimal GetVariationCategoriesPercentage(VariationTimeliness variationTimeliness)
        {
            if (variationTimeliness == VariationTimeliness.NoNoticeClaimProvided)
                return 0.0m;
            else if (variationTimeliness == VariationTimeliness.NoticeClaimProvidedLate)
                return 0.2m;
            else //NoticeClaimProvidedOnTime
                return 0.9m;
        }

        public decimal GetVariationCategoriesPercentage(VariationDiscretionaryAdjustment variationDiscretionaryAdjustment)
        {
            if (variationDiscretionaryAdjustment == VariationDiscretionaryAdjustment.LowConfidence)
                return 0.0m;
            else if (variationDiscretionaryAdjustment == VariationDiscretionaryAdjustment.MediumConfidence)
                return 0.1m;
            else //HighConfidence
                return 0.9m;
        }

        public decimal GetVariationCategoriesWeightage(Enum variationCategory)
        {
            if (variationCategory.GetType() == typeof(VariationClientAcknowledgement))
                return 0.35m;
            if (variationCategory.GetType() == typeof(VariationNature))
                return 0.20m;
            if (variationCategory.GetType() == typeof(VariationSubstantiation))
                return 0.20m;
            if (variationCategory.GetType() == typeof(VariationTimeliness))
                return 0.20m;
            if (variationCategory.GetType() == typeof(VariationDiscretionaryAdjustment))
                return 0.05m;

            return 0;
        }

        public List<ExoSubJobProjection> GetConstructionItemsForExoCommit(bool includeBudget)
        {
            List<ExoSubJobProjection> tempExoVariations = new List<ExoSubJobProjection>();
            if (updatedVARIATION_CONSTRUCTION_ITEMS != null && updatedVARIATION_CONSTRUCTION_ITEMS.Count > 0)
            {
                var groupedItems = updatedVARIATION_CONSTRUCTION_ITEMS.GroupBy(x => new { SubJob = x.SUBJOB })
              .Select(group => new { group.Key.SubJob, BudgetInternalCosts = group.Sum(x => x.TotalCosts) });

                foreach (var groupedItem in groupedItems)
                {
                    ExoSubJobProjection exoJob = new ExoSubJobProjection();
                    exoJob.SubJobCode = groupedItem.SubJob;
                    exoJob.StockCode = BluePrintsResources.VariationStockCode;

                    if(includeBudget)
                        exoJob.ExoBudget = groupedItem.BudgetInternalCosts;

                    exoJob.VariationCode = this.NUMBER;
                    tempExoVariations.Add(exoJob);
                }
            }

            return tempExoVariations;
        }

        public void GetPropertyError(string propertyName, ErrorInfo info)
        {
            if (propertyName == BindableBase.GetPropertyName(() => new VARIATION_CONSTRUCTION().APPROVED_VALUE))
            {
                if(APPROVED_VALUE == 0 && STATUS == VariationConstructionStatus.Approved)
                    info.ErrorText = "Approved variation must have approved value";
            }
        }

        public void GetError(ErrorInfo info)
        {
        }
    }
}