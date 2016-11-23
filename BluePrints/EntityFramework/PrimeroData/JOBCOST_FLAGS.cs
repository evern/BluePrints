namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class JOBCOST_FLAGS
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int JOBNO { get; set; }

        [Required]
        [StringLength(1)]
        public string INVOICEREADY { get; set; }

        [Required]
        [StringLength(1)]
        public string ISACTIVE { get; set; }

        [Required]
        [StringLength(1)]
        public string ISCOMPLETE { get; set; }

        [Required]
        [StringLength(1)]
        public string ISARCHIVED { get; set; }

        [Required]
        [StringLength(1)]
        public string FLAG01 { get; set; }

        [Required]
        [StringLength(1)]
        public string FLAG02 { get; set; }

        [Required]
        [StringLength(1)]
        public string FLAG03 { get; set; }

        [Required]
        [StringLength(1)]
        public string FLAG04 { get; set; }

        [Required]
        [StringLength(1)]
        public string FLAG05 { get; set; }

        [Required]
        [StringLength(1)]
        public string FLAG06 { get; set; }

        [Required]
        [StringLength(1)]
        public string FLAG07 { get; set; }

        [Required]
        [StringLength(1)]
        public string FLAG08 { get; set; }

        [Required]
        [StringLength(1)]
        public string FLAG09 { get; set; }

        [Required]
        [StringLength(1)]
        public string FLAG10 { get; set; }

        [Required]
        [StringLength(1)]
        public string FLAG11 { get; set; }

        [Required]
        [StringLength(1)]
        public string FLAG12 { get; set; }

        [Required]
        [StringLength(1)]
        public string FLAG13 { get; set; }

        [Required]
        [StringLength(1)]
        public string FLAG14 { get; set; }

        [Required]
        [StringLength(1)]
        public string FLAG15 { get; set; }
    }
}
