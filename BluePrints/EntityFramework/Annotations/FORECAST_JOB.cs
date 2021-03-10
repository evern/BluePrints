namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.BluePrintsEntitiesDataModel;
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

        [NotMapped]
        public IBluePrintsEntitiesUnitOfWork BluePrintsEntitiesUnitOfWork { get; set; }

        [NotMapped]
        public List<FORECAST_JOB_HOUR> ForecastJobHours { get; set; }

        public void PrepareForSaveChanges(bool commitToDb)
        {
            if (BluePrintsEntitiesUnitOfWork == null)
                return;

            if (this.GUID == Guid.Empty)
            {
                BluePrintsEntitiesUnitOfWork.FORECAST_JOBS.Add(this);
            }
            //BluePrintsEntitiesUnitOfWork.SaveChanges();

            foreach(FORECAST_JOB_HOUR forecastJobHour in ForecastJobHours)
            {
                if(forecastJobHour.GUID == Guid.Empty)
                {
                    this.FORECAST_JOB_HOUR.Add(forecastJobHour);
                    //forecastJobHour.GUID_FORECAST_JOB = this.GUID;
                    //BluePrintsEntitiesUnitOfWork.FORECAST_JOB_HOURS.Add(forecastJobHour);
                }
            }

            if (commitToDb)
                BluePrintsEntitiesUnitOfWork.SaveChanges();
        }
    }
}