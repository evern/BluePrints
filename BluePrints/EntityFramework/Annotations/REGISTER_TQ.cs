namespace BluePrints.Data
{
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common;
    using BluePrints.Common.Base;
    using DevExpress.Mvvm;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class REGISTER_TQ : EntityBase, IGuidEntityKey, ICanSync, IEntityNumber, IHaveCreatedDate
    {
        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        [NotMapped]
        public string EntityNumber
        {
            get { return NUMBER; }
            set { NUMBER = value; }
        }

        public int ResponseTime
        {
            get
            {
                if (DATE_RESPONSE == null || DATE_REQUESTED == null)
                    return 0;

                double responseTime = (((DateTime)DATE_RESPONSE) - ((DateTime)DATE_REQUESTED)).TotalDays;
                return Convert.ToInt32(responseTime);
            }
        }

        public int DaysOpen
        {
            get
            {
                if (OPENCLOSE == null || DATE_REQUESTED == null)
                    return 0;

                if(((RegisterTQ_OpenClose)OPENCLOSE) == RegisterTQ_OpenClose.Open)
                {
                    double daysOpen = (DateTime.Now - ((DateTime)DATE_REQUESTED)).TotalDays;
                    return Convert.ToInt32(daysOpen);
                }

                return 0;
            }
        }

        public string EntityGroup => string.Empty;

        public string Office => this.PROJECT.NUMBER + " " + this.PROJECT.OfficeName;
    }
}