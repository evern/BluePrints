namespace BluePrints.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class ESTIMATION_SETTING
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        public decimal? COST_PER_FREIGHT { get; set; }

        public decimal? ADD_FREIGHT_ALLOWANCE { get; set; }

        public decimal? MAN_HOUR_PER_SHIFT { get; set; }

        public decimal? AVERAGE_DAYS_PER_WEEK { get; set; }

        public decimal? DIRECT_DAYS_ON_SITE { get; set; }

        public decimal? INDIRECT_DAYS_ON_SITE { get; set; }

        public decimal? DIRECT_DAYS_ON_RNR { get; set; }

        public decimal? INDIRECT_DAYS_ON_RNR { get; set; }

        public decimal? FLIGHT_COST { get; set; }

        public decimal? ADDITIONAL_FLIGHT { get; set; }

        public decimal? ONSITE_ACC_COST { get; set; }

        public decimal? ONSITE_MAN_ACC_COST { get; set; }

        public decimal? RNR_ACC_COST { get; set; }

        public decimal? CONTRACT_VALUE { get; set; }

        public decimal? DEFECTS_LIABILITY_PERIOD { get; set; }

        public decimal? TENDER_COST { get; set; }

        public decimal? SMALL_TOOLS_COST { get; set; }

        public decimal? WEEKLY_SITE_HOURS { get; set; }

        public decimal? WEEKLY_OFFSITE_HOURS { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual PROJECT PROJECT { get; set; }
    }
}
