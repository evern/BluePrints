namespace BluePrints.Data
{
    using BluePrints.Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PROJECT")]
    public partial class PROJECT
    {
        public PROJECT()
        {
            AREA = new HashSet<AREA>();
            BASELINE = new HashSet<BASELINE>();
            ESTIMATION = new HashSet<ESTIMATION>();
            PHASE = new HashSet<PHASE>();
            PROGRESS = new HashSet<PROGRESS>();
            REGISTER = new HashSet<REGISTER>();
            PROJECT_REPORT = new HashSet<PROJECT_REPORT>();
            RATE = new HashSet<RATE>();
            VARIATION = new HashSet<VARIATION>();
            WORKPACK = new HashSet<WORKPACK>();
            COMMODITY_GROUP_DIRECT = new HashSet<COMMODITY_GROUP_DIRECT>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid GUID { get; set; }

        [Required]
        [StringLength(100)]
        public string NUMBER { get; set; }

        [StringLength(100)]
        public string NAME { get; set; }

        [StringLength(100)]
        public string CLIENT { get; set; }

        public ProjectStatus STATUS { get; set; }

        public ContractType CONTRACTTYPE { get; set; }

        public ProjectType TYPE { get; set; }

        public DesignManager DESIGNMANAGER { get; set; }

        public decimal CURRENCYCONVERSION { get; set; }

        public decimal REVIEWPERCENTAGE { get; set; }

        public decimal REVIEWPERIOD { get; set; }

        public bool USELEGACYWORKPACK { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual ICollection<AREA> AREA { get; set; }

        public virtual ICollection<BASELINE> BASELINE { get; set; }

        public virtual ICollection<ESTIMATION> ESTIMATION { get; set; }

        public virtual ICollection<PHASE> PHASE { get; set; }

        public virtual ICollection<PROGRESS> PROGRESS { get; set; }

        public virtual ICollection<REGISTER> REGISTER { get; set; }

        public virtual ICollection<PROJECT_REPORT> PROJECT_REPORT { get; set; }

        public virtual ICollection<RATE> RATE { get; set; }

        public virtual ICollection<VARIATION> VARIATION { get; set; }

        public virtual ICollection<WORKPACK> WORKPACK { get; set; }

        public virtual ICollection<COMMODITY_GROUP_DIRECT> COMMODITY_GROUP_DIRECT { get; set; }
    }
}
