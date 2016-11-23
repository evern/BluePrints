namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLACCS
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ACCNO { get; set; }

        [StringLength(40)]
        public string NAME { get; set; }

        [StringLength(1)]
        public string DRCR { get; set; }

        public int? SECTION { get; set; }

        public double? OPENINGBAL { get; set; }

        public double? BALANCE { get; set; }

        [StringLength(1)]
        public string USESUBCODES { get; set; }

        [StringLength(15)]
        public string REPORTCODE { get; set; }

        public int? TAXSTATUS { get; set; }

        public int? CURRENCYNO { get; set; }

        [StringLength(1)]
        public string ISACTIVE { get; set; }

        public int? ACCGROUP { get; set; }

        [StringLength(1)]
        public string ALLOWJOURNAL { get; set; }

        public DateTime? LAST_UPDATED { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? BALSHEETACCTOT { get; set; }

        public int BALANCE_SHEET_TYPE { get; set; }

        [StringLength(4096)]
        public string NOTES { get; set; }

        [Required]
        [StringLength(1)]
        public string PRIVATE_ACC { get; set; }

        public DateTime? CREATED_DATE { get; set; }
    }
}
