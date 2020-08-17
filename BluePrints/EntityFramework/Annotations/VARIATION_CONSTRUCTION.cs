namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common;
    using BluePrints.Common.Base;
    using BluePrints.Common.Projections;
    using DevExpress.Mvvm;
    using DevExpress.XtraEditors.DXErrorProvider;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class VARIATION_CONSTRUCTION : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate, IDXDataErrorInfo
    {
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

        public decimal ManagementTotal
        {
            get
            {
                if (VARIATION_CONSTRUCTION_ITEM == null)
                    return 0;

                return VARIATION_CONSTRUCTION_ITEM.Where(x => x.TYPE == VariationConstructionItemType.Management).Sum(x => x.TotalCosts);
            }
        }

        public decimal EngineeringTotal
        {
            get
            {
                if (VARIATION_CONSTRUCTION_ITEM == null)
                    return 0;

                return VARIATION_CONSTRUCTION_ITEM.Where(x => x.TYPE == VariationConstructionItemType.Engineering).Sum(x => x.TotalCosts);
            }
        }

        public decimal TradesAndLabourTotal
        {
            get
            {
                if (VARIATION_CONSTRUCTION_ITEM == null)
                    return 0;

                return VARIATION_CONSTRUCTION_ITEM.Where(x => x.TYPE == VariationConstructionItemType.TradesAndLabour).Sum(x => x.TotalCosts);
            }
        }

        public decimal EquipmentTotal
        {
            get
            {
                if (VARIATION_CONSTRUCTION_ITEM == null)
                    return 0;

                return VARIATION_CONSTRUCTION_ITEM.Where(x => x.TYPE == VariationConstructionItemType.Equipment).Sum(x => x.TotalCosts);
            }
        }

        public decimal MaterialTotal
        {
            get
            {
                if (VARIATION_CONSTRUCTION_ITEM == null)
                    return 0;

                return VARIATION_CONSTRUCTION_ITEM.Where(x => x.TYPE == VariationConstructionItemType.Material).Sum(x => x.TotalCosts);
            }
        }

        public decimal TotalEstimatedValue => ManagementTotal + EngineeringTotal + TradesAndLabourTotal + EquipmentTotal + MaterialTotal;

        public string Office => this.PROJECT.NUMBER + " " + this.PROJECT.OfficeName;

        public List<ExoSubJobProjection> GetConstructionItemsForExoCommit()
        {
            List<ExoSubJobProjection> tempExoVariations = new List<ExoSubJobProjection>();
            if (VARIATION_CONSTRUCTION_ITEM != null && VARIATION_CONSTRUCTION_ITEM.Count > 0)
            {
                var groupedItems = VARIATION_CONSTRUCTION_ITEM.GroupBy(x => new { SubJob = x.SUBJOB, DisciplineCode = x.COSTGROUP, CommodityCode = x.COSTTYPE })
              .Select(group => new { group.Key.SubJob, group.Key.DisciplineCode, group.Key.CommodityCode, BudgetInternalCosts = group.Sum(x => x.TotalCosts) });

                foreach (var groupedItem in groupedItems)
                {
                    ExoSubJobProjection exoJob = new ExoSubJobProjection();
                    exoJob.SubJobCode = groupedItem.SubJob;
                    exoJob.DisciplineCode = groupedItem.DisciplineCode;
                    exoJob.CommodityCode = groupedItem.CommodityCode;
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
                if(APPROVED_VALUE <= 0 && STATUS == VariationConstructionStatus.Approved)
                    info.ErrorText = "Approved variation must have approved value";
            }
        }

        public void GetError(ErrorInfo info)
        {
        }
    }
}