namespace BluePrints.Data
{
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common;
    using BluePrints.Common.Base;
    using DevExpress.Mvvm;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

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

                double responseTime = (((DateTime)DATE_REQUESTED) - ((DateTime)DATE_RESPONSE)).TotalDays;
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

        [NotMapped]
        private IEnumerable<object> documents;

        [NotMapped]
        public object Documents
        {
            get { return documents; }
            set
            {
                if (value != documents)
                {
                    documents = value as IEnumerable<object>;
                }
            }
        }

        [NotMapped]
        public IEnumerable<REGISTER_TQ_ATTACHMENT> DocumentAssignments
        {
            get
            {
                if (documents == null)
                    return null;

                return documents.Select(x => (REGISTER_TQ_ATTACHMENT)x);
            }
        }
    }
}