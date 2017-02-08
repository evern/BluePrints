namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PAYROLL_INFO
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DEBITACCOUNTNO { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CREDITACCOUNTNO { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int COMPANYNO { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int BRANCHNO { get; set; }

        [Key]
        [Column(Order = 4)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PAYROLLSOFTWARE { get; set; }

        public DateTime? DATEOFLASTPOST { get; set; }

        [StringLength(255)]
        public string COMACCPATH { get; set; }

        [Key]
        [Column(Order = 5)]
        [StringLength(1)]
        public string ENABLEMULTIBRANCH { get; set; }

        [Key]
        [Column(Order = 6)]
        [StringLength(1)]
        public string ENABLEAPPSUBACCOUNT { get; set; }

        [Key]
        [Column(Order = 7)]
        [StringLength(1)]
        public string ENFORCEPEROIDNO { get; set; }

        [Key]
        [Column(Order = 8)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SUBACCOUNTNO { get; set; }

        [Key]
        [Column(Order = 9)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PERIODNO { get; set; }

        [Key]
        [Column(Order = 10)]
        [StringLength(1)]
        public string RENAMEFILE { get; set; }
    }
}