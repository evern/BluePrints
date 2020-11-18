namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using DevExpress.Mvvm;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using BluePrints.Common.Base;
    using BaseModel.DataModel;
    using BluePrints.Common.Resources;
    using BluePrints.Common.ViewModel.Reporting;
    using BluePrints.Common.Projections;
    using DevExpress.XtraEditors.DXErrorProvider;

    public partial class TENDER_PROFILE_ITEM : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate
    {
        public TENDER_PROFILE_ITEM()
        {
            BELLCURVESHAPE = Common.BellCurveShape.Balanced;
        }

        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        public string Office
        {
            get
            {
                if (this.TENDER_PROFILE != null && this.TENDER_PROFILE.PROJECT != null)
                    return this.TENDER_PROFILE.PROJECT.NUMBER + " " + this.TENDER_PROFILE.PROJECT.OfficeName;

                return BluePrintsResources.GlobalOffice;
            }
        }

        [NotMapped]
        public PROJECTTenderProfile PROJECTTenderProfile { get; set; }

        [NotMapped]
        public List<Common.ViewModel.Reporting.DataPoint> DataPoints { get; set; }

        [NotMapped]
        public bool IsPercentageError { get; set; }
        public decimal IsPercentageErrorImageWidth => IsPercentageError ? 15 : 0;

        public Tuple<DateTime, DateTime> GetProRatedStartEndDate(int durationInDays, DateTime startDate, DateTime endDate)
        {
            //pro-rate the dates of the deliverable based on tender item
            int startProrateDurationInDays = Convert.ToInt32(durationInDays * this.SCHEDULE_START_PERCENTAGE);
            DateTime proRatedStartDate = startDate.AddDays(startProrateDurationInDays);
            int endProrateDurationInDays = Convert.ToInt32(durationInDays * (1 - this.SCHEDULE_FINISH_PERCENTAGE));
            DateTime proRatedEndDate = endDate.AddDays(-1 * endProrateDurationInDays);

            return new Tuple<DateTime, DateTime>(proRatedStartDate, proRatedEndDate);
        }
    }
}