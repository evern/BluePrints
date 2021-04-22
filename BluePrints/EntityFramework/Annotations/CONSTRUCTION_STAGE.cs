namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Helpers;
    using BaseModel.Misc;
    using BluePrints.Common.Base;
    using BluePrints.Common.Resources;
    using DevExpress.Mvvm;
    using DevExpress.XtraEditors.DXErrorProvider;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    [ConstraintAttributes("SCORE_CARD_DISCIPLINE, NAME")]
    public partial class CONSTRUCTION_STAGE : EntityBase, IGuidEntityKey, IEntityNumber, ICanSync, IHaveCreatedDate, IDXDataErrorInfo
    {
        [NotMapped]
        public string EntityNumber
        {
            get { return SORT_ORDER.ToString(); }
            set { SORT_ORDER = Int32.Parse(value); }
        }

        public string EntityGroup => SCORE_CARD_DISCIPLINE.ToString();

        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        public string Office => BluePrintsResources.GlobalOffice;

        public long EntitySortNumber => SORT_ORDER;

        [NotMapped]
        public IEnumerable<CONSTRUCTION_STAGE> OtherConstructionStage { get; set; }

        public void GetError(ErrorInfo info)
        {
        }

        public void GetPropertyError(string propertyName, ErrorInfo info)
        {
            if (propertyName.Contains(BindableBase.GetPropertyName(() => new CONSTRUCTION_STAGE().WEIGHT_PERCENTAGE)))
            {
                if (OtherConstructionStage != null)
                {
                    IEnumerable<CONSTRUCTION_STAGE> filteredOtherConstructionStages = OtherConstructionStage.Where(x => x.GUID != this.GUID).Where(x => x.EntityGroup == EntityGroup);
                    decimal otherFilteredConstructionStagesPercentage = filteredOtherConstructionStages.Sum(x => x.WEIGHT_PERCENTAGE);
                    decimal totalFilteredConstructionStagePercentage = WEIGHT_PERCENTAGE + otherFilteredConstructionStagesPercentage;

                    if (totalFilteredConstructionStagePercentage > 1)
                        info.ErrorText = "Discipline total percentage exceeding 100% at " + (totalFilteredConstructionStagePercentage * 100) + "%";
                    else
                        info.ErrorText = string.Empty;
                }
            }
        }
    }
}