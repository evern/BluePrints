namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CL_END_OF_PERIOD
    {
        [Key]
        public int SEQNO { get; set; }

        public int WIDGETID { get; set; }

        public int? CTX_SEQNO { get; set; }

        public int? DEBTORS_CHECKLIST { get; set; }

        public int? DR_COMPLETE_DEBTOR_PROCESSING { get; set; }

        public int? DR_DEBTORS_AGED_BALANCES { get; set; }

        public int? DR_DATA_VERIFICATION { get; set; }

        public int? DR_PRINT_DEBTOR_STATEMENTS { get; set; }

        public int? CREDITORS_LEDGER_CHECKLIST { get; set; }

        public int? CR_COMPLETE_ALL_TRANSACTIONS { get; set; }

        public int? CR_CREDITORS_AGED_BALANCES { get; set; }

        public int? CR_DATA_VERIFICATION { get; set; }

        public int? STOCK_LEDGER_CHECKLIST { get; set; }

        public int? ST_COMPLETE_ALL_TRANSACTIONS { get; set; }

        public int? ST_STOCK_VALUATION { get; set; }

        public int? ST_DATA_VERIFICATION { get; set; }

        public int? GENERAL_LEDGER_CHECKLIST { get; set; }

        public int? GL_COMPLETE_ALL_TRANSACTIONS { get; set; }

        public int? GL_BANK_RECONCILIATION { get; set; }

        public int? GL_UPDATE_CURRENCIES { get; set; }

        public int? GL_POST_FOREX_VARIANCES { get; set; }

        public int? GL_POST_CLOSING_STOCK { get; set; }

        public int? GL_POST_LEDGERS_TO_GL { get; set; }

        public int? GL_DATA_VERIFICATION { get; set; }

        public int? GL_LEDGER_RECONCILIATION { get; set; }

        public int? GL_CHECK_TRIAL_BALANCE { get; set; }

        public int? END_OF_PERIOD_CHECKLIST { get; set; }

        public int? EP_PRINT_FINANCIAL_REPORTS { get; set; }

        public int? EP_END_OF_PERIOD { get; set; }

        public int? EP_END_OF_YEAR { get; set; }

        public int? EP_SETUP_PERIOD_STATUS { get; set; }
    }
}
