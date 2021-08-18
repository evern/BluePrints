namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class X_TIME_TRANSACTION
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        public string SUBJOB_CODE { get; set; }

        public string COST_GROUP { get; set; }

        public string COST_TYPE { get; set; }

        public string VARIATION_CODE { get; set; }

        public double TOTAL_HOURS { get; set; }

        public double TOTAL_COSTS { get; set; }

        public int Q_YEAR { get; set; }

        public int Q_WEEKNO { get; set; }

        public DateTime FIRST_WEEK_DATE { get; set; }
    }
}