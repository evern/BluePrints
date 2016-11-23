namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GL_CONTROL
    {
        public int? BANK01 { get; set; }

        public int? BANK02 { get; set; }

        public int? BANK03 { get; set; }

        public int? DEBTORS { get; set; }

        public int? DEBTORSTAX { get; set; }

        public int? CREDITORS { get; set; }

        public int? CREDITORSTAX { get; set; }

        public int? ADJUSTMENTS { get; set; }

        public int? ERRORS { get; set; }

        public int? PROFITFWD { get; set; }

        public int? PERIODSTHISYEAR { get; set; }

        public int? DEF_SALES { get; set; }

        public int? DEF_PURCH { get; set; }

        public int? STOCKBAL { get; set; }

        public int? COSTOFSALE { get; set; }

        public int? SUB_BANK01 { get; set; }

        public int? SUB_BANK02 { get; set; }

        public int? SUB_BANK03 { get; set; }

        public int? SUB_DEBTORS { get; set; }

        public int? SUB_DEBTORSTAX { get; set; }

        public int? SUB_CREDITORS { get; set; }

        public int? SUB_CREDITORSTAX { get; set; }

        public int? SUB_ADJUSTMENTS { get; set; }

        public int? SUB_ERRORS { get; set; }

        public int? SUB_PROFITFWD { get; set; }

        public int? SUB_DEF_SALES { get; set; }

        public int? SUB_DEF_PURCH { get; set; }

        public int? SUB_STOCKBAL { get; set; }

        public int? SUB_COSTOFSALE { get; set; }

        public int? BSBRANCH { get; set; }

        public int? DEBTORS_EXCHVAR { get; set; }

        public int? CREDITORS_EXCHVAR { get; set; }

        public int? CURRENT_EXCHVAR { get; set; }

        public int? WITHHOLDINGTAX { get; set; }

        public int? SUB_WITHHOLDINGTAX { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SEQNO { get; set; }

        public int? STOCKSUSPENSE { get; set; }

        public int? SUB_STOCKSUSPENSE { get; set; }

        public int? STOCKADJUSTMENTS { get; set; }

        public int? SUB_STOCKADJUSTMENTS { get; set; }

        public int? DEPOSITS { get; set; }

        public int? SUB_DEPOSITS { get; set; }

        public int SUB_DEBTORS_EXCHVAR { get; set; }

        public int SUB_CREDITORS_EXCHVAR { get; set; }

        public int SUB_CURRENT_EXCHVAR { get; set; }

        public int? DISCOUNTPROVISION { get; set; }

        public int? SUB_DISCOUNTPROVISION { get; set; }

        public int? SUB_ROUNDING { get; set; }

        public int? ROUNDING { get; set; }

        public int? ON_COST_CLEARING { get; set; }

        public int? SUB_ON_COST_CLEARING { get; set; }

        public int ON_COST_VARIATION { get; set; }

        public int SUB_ON_COST_VARIATION { get; set; }

        public int CR_PPDISC { get; set; }

        public int SUB_CR_PPDISC { get; set; }

        public int? STOCK_VARIATION { get; set; }

        public int? SUB_STOCK_VARIATION { get; set; }

        public int CR_INVMANUALROUNDING { get; set; }

        public int SUB_CR_INVMANUALROUNDING { get; set; }

        public int DR_RETENTION { get; set; }

        public int SUB_DR_RETENTION { get; set; }

        public int PAYG { get; set; }

        public int SUB_PAYG { get; set; }

        public int WORKCOVER { get; set; }

        public int SUB_WORKCOVER { get; set; }

        public int SUPER { get; set; }

        public int SUB_SUPER { get; set; }

        public int NONCASHBENEFIT { get; set; }

        public int SUB_NONCASHBENEFIT { get; set; }

        public int PAYMENT_DC { get; set; }

        public int SUB_PAYMENT_DC { get; set; }

        public int PAYMENT_CASH { get; set; }

        public int SUB_PAYMENT_CASH { get; set; }

        public int PAYMENT_CHEQUE { get; set; }

        public int SUB_PAYMENT_CHEQUE { get; set; }

        public int PAYMENT_ROUNDING { get; set; }

        public int SUB_PAYMENT_ROUNDING { get; set; }

        public int LEAVE_ANNUAL { get; set; }

        public int SUB_LEAVE_ANNUAL { get; set; }

        public int LEAVE_PERSONAL { get; set; }

        public int SUB_LEAVE_PERSONAL { get; set; }

        public int LEAVE_LONGSERVICE { get; set; }

        public int SUB_LEAVE_LONGSERVICE { get; set; }

        public int LEAVE_OTHER1 { get; set; }

        public int SUB_LEAVE_OTHER1 { get; set; }

        public int LEAVE_OTHER2 { get; set; }

        public int SUB_LEAVE_OTHER2 { get; set; }

        public int DEBTORSTAXROUNDING { get; set; }

        public int SUB_DEBTORSTAXROUNDING { get; set; }

        public int CREDITORSADJUSTMENTS { get; set; }

        public int SUB_CREDITORSADJUSTMENTS { get; set; }

        public int DEBTORS_EXCHVAR_UNREALISED { get; set; }

        public int SUB_DEBTORS_EXCHVAR_UNREALISED { get; set; }

        public int CREDITORS_EXCHVAR_UNREALISED { get; set; }

        public int SUB_CREDITORS_EXCHVAR_UNREALISED { get; set; }
    }
}
