namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class CL_FINANCIAL_REVIEW
    {
        [Key]
        public int SEQNO { get; set; }

        public int WIDGETID { get; set; }

        public int? CTX_SEQNO { get; set; }

        public int? GENERAL_LEDGER_REPORTS { get; set; }

        public int? GLREPORT_REPORTID10 { get; set; }

        public int? GLREPORT_REPORTID11 { get; set; }

        public int? GLREPORT_REPORTID12 { get; set; }

        public int? GLREPORT_REPORTID2 { get; set; }

        public int? GLREPORT_REPORTID5 { get; set; }

        public int? GLREPORT_REPORTID6 { get; set; }

        public int? GLREPORT_REPORTID7 { get; set; }

        public int? GLREPORT_REPORTID8 { get; set; }

        public int? GLREPORT_REPORTID9 { get; set; }

        public int? TRIAL_BALANCE { get; set; }

        public int? GL_GRAPHS { get; set; }

        public int? OTHER_REPORTS { get; set; }

        public int? DR_DEBTORS_AGED_BALANCES { get; set; }

        public int? CR_CREDITORS_AGED_BALANCES { get; set; }

        public int? STOCK_VALUATION { get; set; }

        public int? EXPORT_TO_ACCOUNTANTS_OFFICE { get; set; }
    }
}