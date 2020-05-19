namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class X_JOB_TIMESHEETS
    {
        [Key]
        public Guid SEQNO { get; set; }

        public string MASTER_JOBCODE { get; set; }

        public string SUB_JOBCODE { get; set; }

        public string TITLE { get; set; }

        public string RESOURCENAME { get; set; }

        public string X_VARIATIONCODE { get; set; }

        public DateTime DAY1DATE { get; set; }

        public double SUM_DAY1 { get; set; }

        public double SUM_DAY2 { get; set; }

        public double SUM_DAY3 { get; set; }

        public double SUM_DAY4 { get; set; }

        public double SUM_DAY5 { get; set; }

        public double SUM_DAY6 { get; set; }

        public double SUM_DAY7 { get; set; }
    }
}