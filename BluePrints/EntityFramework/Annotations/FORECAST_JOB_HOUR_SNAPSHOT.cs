namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.BluePrintsEntitiesDataModel;
    using BluePrints.Common.Base;
    using BluePrints.Common.Projections;
    using BluePrints.Common.Resources;
    using BluePrints.Common.ViewModel.Reporting;
    using DevExpress.Mvvm;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    
    public partial class FORECAST_JOB_HOUR_SNAPSHOT : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate
    {
        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        //used for collection mapping which allows null values
        [NotMapped]
        public Guid? MappingEntityKey
        {
            get
            {
                if (GUID == Guid.Empty)
                    return null;

                return GUID;
            }
        }

        public string Office => BluePrintsResources.GlobalOffice;
    }
}