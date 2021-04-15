namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Helpers;
    using BaseModel.Misc;
    using BluePrints.Common;
    using BluePrints.Common.Base;
    using BluePrints.Common.Resources;
    using DevExpress.Mvvm;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    [ConstraintAttributes("NUMBER")]
    public partial class REGISTER_CHANGE : EntityBase, IGuidEntityKey, ICanSync, IEntityNumber, IHaveCreatedDate
    {
        public REGISTER_CHANGE()
        {
            Documents = new List<REGISTER_CHANGE_ATTACHMENT>();
        }

        [NotMapped]
        public string EntityNumber
        {
            get { return NUMBER; }
            //set sort number to null so it refreshes the next time get is called
            set { NUMBER = value; entitySortNumber = null; }
        }

        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        public string ApprovedStr
        {
            get
            {
                return APPROVED == null ? BluePrintsResources.FalseConversionString : ((bool)APPROVED) ? BluePrintsResources.TrueConversionString : BluePrintsResources.FalseConversionString;
            }
        }

        public string InterdisciplinaryCheckStr
        {
            get
            {
                return INTERDISC_CHECK_COMPLETE ? BluePrintsResources.TrueConversionString : BluePrintsResources.FalseConversionString;
            }
        }

        public decimal EPCM_CostImpact
        {
            get
            {
                if (COST_IMPACT == null || COST_IMPACT == ScheduleImpact.No)
                    return 0;

                decimal epcmHoursImpact = EPCM_HOURS_IMPACT == null ? 0 : ((decimal)EPCM_HOURS_IMPACT);
                decimal avgHoursRate = AVG_HR_RATE == null ? 0 : ((decimal)AVG_HR_RATE);

                return epcmHoursImpact * avgHoursRate;
            }
        }

        public decimal TotalCostImpact
        {
            get
            {
                decimal capexImpact = CAPEX_IMPACT == null ? 0 : ((decimal)CAPEX_IMPACT);
                return EPCM_CostImpact + capexImpact;
            }
        }

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
        public IEnumerable<REGISTER_CHANGE_ATTACHMENT> DocumentAssignments
        {
            get
            {
                if (documents == null)
                    return null;

                return documents.Select(x => (REGISTER_CHANGE_ATTACHMENT)x);
            }
        }

        [NotMapped]
        public string ReferenceDocuments
        {
            get
            {
                if (DocumentAssignments.Count() == 0)
                    return string.Empty;

                string documentAssignments = string.Empty;
                foreach (REGISTER_CHANGE_ATTACHMENT documentAssignment in DocumentAssignments)
                {
                    documentAssignments = string.Concat(documentAssignments, "\n", documentAssignment);
                }

                //remove carriage return from the beginning
                documentAssignments = documentAssignments.Substring(1, documentAssignments.Length - 1);
                return documentAssignments;
            }
        }

        public decimal PathImageWidth => CHANGE_PATH == null || CHANGE_PATH == string.Empty ? 0 : 15;

        public string EntityGroup => string.Empty;

        public string Office => this.PROJECT.NUMBER + " " + this.PROJECT.OfficeName;

        long? entitySortNumber;
        public long EntitySortNumber
        {
            get
            {
                if(entitySortNumber == null)
                {
                    long sortNumber = 0;
                    int dummyFieldLength = 0;
                    string dummyString;
                    dummyString = StringFormatUtils.ParseStringIntoComponents(this.EntityNumber, out dummyFieldLength, out sortNumber);
                    entitySortNumber = sortNumber;
                }

                return (long)entitySortNumber;
            }
        }
    }
}