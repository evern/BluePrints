namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common.Base;
    using BluePrints.Common.Projections;
    using BluePrints.Common.ViewModel.Reporting;
    using DevExpress.Mvvm;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    
    public partial class FORECAST_JOB : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate, IHaveWBSCodeString
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

        [NotMapped]
        public ExoSubJobProjection ExoJob { get; set; }

        [NotMapped]
        public List<KeyValuePair<string, decimal>> DatesForecasts = new List<KeyValuePair<string, decimal>>();
        public string Office => this.PROJECT.NUMBER + " " + this.PROJECT.OfficeName;
    }
}