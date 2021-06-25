namespace BluePrints.PrimeroData
{
    using System.Data.Entity;
    public partial class PUSAEntities : PrimeroEntities
    {
        public PUSAEntities()
            : base("name=PUSAEntities")
        {
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<PGAEntities>(null);
            Database.CommandTimeout = 100000;
            base.OnModelCreating(modelBuilder);
        }
    }

    public partial class PGAEntities : PrimeroEntities
    {
        public PGAEntities()
            : base("name=PGAEntities")
        {
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<PGAEntities>(null);
            Database.CommandTimeout = 100000;
            base.OnModelCreating(modelBuilder);
        }
    }

    public partial class PrimeroEntities : DbContext
    {
        public PrimeroEntities(string connectionString)
            : base(connectionString)
        {
            this.Database.CommandTimeout = 100000;
        }

        public PrimeroEntities()
            : base("name=PrimeroEntities")
        {
            this.Database.CommandTimeout = 100000;
        }

        public virtual DbSet<ADJUSTMENT_TYPES> ADJUSTMENT_TYPES { get; set; }
        public virtual DbSet<ADVERT_TYPES> ADVERT_TYPES { get; set; }
        public virtual DbSet<ANALYSIS> ANALYSIS { get; set; }
        public virtual DbSet<ANALYSIS_CODES> ANALYSIS_CODES { get; set; }
        public virtual DbSet<ANALYSIS_MATRIX> ANALYSIS_MATRIX { get; set; }
        public virtual DbSet<ANALYSIS_TYPES> ANALYSIS_TYPES { get; set; }
        public virtual DbSet<AUDIT_TRAIL> AUDIT_TRAIL { get; set; }
        public virtual DbSet<AUDITOR_EVENTS> AUDITOR_EVENTS { get; set; }
        public virtual DbSet<AUDITOR_SAVEPROCS> AUDITOR_SAVEPROCS { get; set; }
        public virtual DbSet<AUDITOR_SETTINGS> AUDITOR_SETTINGS { get; set; }
        public virtual DbSet<AUDITOR_TERMINALS> AUDITOR_TERMINALS { get; set; }
        public virtual DbSet<AUTO_PURCH> AUTO_PURCH { get; set; }
        public virtual DbSet<BANK_BATCH_PRINT> BANK_BATCH_PRINT { get; set; }
        public virtual DbSet<BANK_DATA_FORMAT> BANK_DATA_FORMAT { get; set; }
        public virtual DbSet<BANK_DATA_TYPE> BANK_DATA_TYPE { get; set; }
        public virtual DbSet<BANK_REC_AUTO_MATCH> BANK_REC_AUTO_MATCH { get; set; }
        public virtual DbSet<BANK_REC_LOG> BANK_REC_LOG { get; set; }
        public virtual DbSet<BANK_REC_SETUP> BANK_REC_SETUP { get; set; }
        public virtual DbSet<BANK_REC_UPLOADS> BANK_REC_UPLOADS { get; set; }
        public virtual DbSet<BANK_TEMPLATES> BANK_TEMPLATES { get; set; }
        public virtual DbSet<BANKBATCH_HDR> BANKBATCH_HDR { get; set; }
        public virtual DbSet<BANKS> BANKS { get; set; }
        public virtual DbSet<BATCH_QUANTITIES> BATCH_QUANTITIES { get; set; }
        public virtual DbSet<BATCH_STOCKTAKE> BATCH_STOCKTAKE { get; set; }
        public virtual DbSet<BILLOMAT_HDR> BILLOMAT_HDR { get; set; }
        public virtual DbSet<BILLOMAT_LINES> BILLOMAT_LINES { get; set; }
        public virtual DbSet<BILLOMAT_TEMP> BILLOMAT_TEMP { get; set; }
        public virtual DbSet<BPAY_TYPES> BPAY_TYPES { get; set; }
        public virtual DbSet<BRANCHES> BRANCHES { get; set; }
        public virtual DbSet<CAMPAIGN> CAMPAIGN { get; set; }
        public virtual DbSet<CAMPAIGN_HIST> CAMPAIGN_HIST { get; set; }
        public virtual DbSet<CAMPAIGN_STAGE> CAMPAIGN_STAGE { get; set; }
        public virtual DbSet<CAMPAIGN_STAGE_CONSTRAINT> CAMPAIGN_STAGE_CONSTRAINT { get; set; }
        public virtual DbSet<CAMPAIGN_WAVE> CAMPAIGN_WAVE { get; set; }
        public virtual DbSet<CASHBOOKHEADER> CASHBOOKHEADER { get; set; }
        public virtual DbSet<CASHBOOKLINE> CASHBOOKLINE { get; set; }
        public virtual DbSet<CASHBOOKLINEALLOC> CASHBOOKLINEALLOC { get; set; }
        public virtual DbSet<CHECKLIST_WIDGET_NODES> CHECKLIST_WIDGET_NODES { get; set; }
        public virtual DbSet<CHEQUE_AUDIT> CHEQUE_AUDIT { get; set; }
        public virtual DbSet<CL_END_OF_PERIOD> CL_END_OF_PERIOD { get; set; }
        public virtual DbSet<CL_END_OF_YEAR> CL_END_OF_YEAR { get; set; }
        public virtual DbSet<CL_FINANCIAL_REVIEW> CL_FINANCIAL_REVIEW { get; set; }
        public virtual DbSet<CL_SYSTEMS_INTEGRITY> CL_SYSTEMS_INTEGRITY { get; set; }
        public virtual DbSet<CL_TAX_RETURN> CL_TAX_RETURN { get; set; }
        public virtual DbSet<ClarityReport> ClarityReport { get; set; }
        public virtual DbSet<COMMON_PHRASES> COMMON_PHRASES { get; set; }
        public virtual DbSet<COMPANY_TYPES> COMPANY_TYPES { get; set; }
        public virtual DbSet<COMPUTER> COMPUTER { get; set; }
        public virtual DbSet<CONTACT_HIST> CONTACT_HIST { get; set; }
        public virtual DbSet<CONTACT_LIST> CONTACT_LIST { get; set; }
        public virtual DbSet<CONTACT_LIST_ITEM> CONTACT_LIST_ITEM { get; set; }
        public virtual DbSet<CONTACT_MARKETING_CLASSES> CONTACT_MARKETING_CLASSES { get; set; }
        public virtual DbSet<CONTACTS> CONTACTS { get; set; }
        public virtual DbSet<COUNTRY> COUNTRY { get; set; }
        public virtual DbSet<Courier_Location_Depot> Courier_Location_Depot { get; set; }
        public virtual DbSet<COURIER_MANIFEST> COURIER_MANIFEST { get; set; }
        public virtual DbSet<COURIERS> COURIERS { get; set; }
        public virtual DbSet<CR_ACCGROUP2S> CR_ACCGROUP2S { get; set; }
        public virtual DbSet<CR_ACCGROUPS> CR_ACCGROUPS { get; set; }
        public virtual DbSet<CR_ACCS> CR_ACCS { get; set; }
        public virtual DbSet<CR_ALLOCATIONS> CR_ALLOCATIONS { get; set; }
        public virtual DbSet<CR_CONT_HIST> CR_CONT_HIST { get; set; }
        public virtual DbSet<CR_CONTACTS> CR_CONTACTS { get; set; }
        public virtual DbSet<CR_INVLINES> CR_INVLINES { get; set; }
        public virtual DbSet<CR_INVLINES_SERIALS> CR_INVLINES_SERIALS { get; set; }
        public virtual DbSet<CR_LIST_NAME> CR_LIST_NAME { get; set; }
        public virtual DbSet<CR_LISTS> CR_LISTS { get; set; }
        public virtual DbSet<CR_PRICES> CR_PRICES { get; set; }
        public virtual DbSet<CR_TRANS> CR_TRANS { get; set; }
        public virtual DbSet<CREDIT_STATUSES> CREDIT_STATUSES { get; set; }
        public virtual DbSet<CRM_BUDGET> CRM_BUDGET { get; set; }
        public virtual DbSet<CRM_BUDGET_HDR> CRM_BUDGET_HDR { get; set; }
        public virtual DbSet<CRM_BUDGET_LINE> CRM_BUDGET_LINE { get; set; }
        public virtual DbSet<CURRENCIES> CURRENCIES { get; set; }
        public virtual DbSet<CURRENCY_CLOSING_RATES> CURRENCY_CLOSING_RATES { get; set; }
        public virtual DbSet<CURRENCY_RATECHANGES> CURRENCY_RATECHANGES { get; set; }
        public virtual DbSet<CUSTCLARITYREPORT> CUSTCLARITYREPORT { get; set; }
        public virtual DbSet<CUSTOM_REPORTS> CUSTOM_REPORTS { get; set; }
        public virtual DbSet<CUSTOM_VIEWS> CUSTOM_VIEWS { get; set; }
        public virtual DbSet<CUSTOMIZATION> CUSTOMIZATION { get; set; }
        public virtual DbSet<D_DEBIT_HDR> D_DEBIT_HDR { get; set; }
        public virtual DbSet<D_DEBIT_LINES> D_DEBIT_LINES { get; set; }
        public virtual DbSet<DASHBOARDS> DASHBOARDS { get; set; }
        public virtual DbSet<DASHBOARDS_STAFF> DASHBOARDS_STAFF { get; set; }
        public virtual DbSet<DATAEXPORT_CTRL> DATAEXPORT_CTRL { get; set; }
        public virtual DbSet<DATE_RANGES> DATE_RANGES { get; set; }
        public virtual DbSet<DISCOUNTS> DISCOUNTS { get; set; }
        public virtual DbSet<DISPLAY_NAMES> DISPLAY_NAMES { get; set; }
        public virtual DbSet<DISPMETHOD> DISPMETHOD { get; set; }
        public virtual DbSet<DOC_BATCH_HDR> DOC_BATCH_HDR { get; set; }
        public virtual DbSet<DOC_BATCH_LINES> DOC_BATCH_LINES { get; set; }
        public virtual DbSet<DOCUMENTS> DOCUMENTS { get; set; }
        public virtual DbSet<DR_ACCGROUP2S> DR_ACCGROUP2S { get; set; }
        public virtual DbSet<DR_ACCGROUPS> DR_ACCGROUPS { get; set; }
        public virtual DbSet<DR_ACCS> DR_ACCS { get; set; }
        public virtual DbSet<DR_ADDRESSES> DR_ADDRESSES { get; set; }
        public virtual DbSet<DR_ALLOCATIONS> DR_ALLOCATIONS { get; set; }
        public virtual DbSet<DR_CONT_HIST> DR_CONT_HIST { get; set; }
        public virtual DbSet<DR_CONTACTS> DR_CONTACTS { get; set; }
        public virtual DbSet<DR_INVLINES> DR_INVLINES { get; set; }
        public virtual DbSet<DR_INVLINES_SERIALS> DR_INVLINES_SERIALS { get; set; }
        public virtual DbSet<DR_PRICE_POLICY> DR_PRICE_POLICY { get; set; }
        public virtual DbSet<DR_PRICE_POLICY_ACC> DR_PRICE_POLICY_ACC { get; set; }
        public virtual DbSet<DR_PRICEGROUPS> DR_PRICEGROUPS { get; set; }
        public virtual DbSet<DR_PRICES> DR_PRICES { get; set; }
        public virtual DbSet<DR_TRANS> DR_TRANS { get; set; }
        public virtual DbSet<dtproperties> dtproperties { get; set; }
        public virtual DbSet<EFT_AUDIT> EFT_AUDIT { get; set; }
        public virtual DbSet<EXONET_CACHE_TABLES> EXONET_CACHE_TABLES { get; set; }
        public virtual DbSet<EXTRA_FIELDS> EXTRA_FIELDS { get; set; }
        public virtual DbSet<FACT_GLOSSARY> FACT_GLOSSARY { get; set; }
        public virtual DbSet<FORMULAHDR> FORMULAHDR { get; set; }
        public virtual DbSet<FORMULALINES> FORMULALINES { get; set; }
        public virtual DbSet<FREIGHT_COST_TYPES> FREIGHT_COST_TYPES { get; set; }
        public virtual DbSet<FREIGHTCOST_SPREAD_TYPE> FREIGHTCOST_SPREAD_TYPE { get; set; }
        public virtual DbSet<GENERAL_INFO> GENERAL_INFO { get; set; }
        public virtual DbSet<GL_ACCGROUPS> GL_ACCGROUPS { get; set; }
        public virtual DbSet<GL_ACCGRP_REPCODE> GL_ACCGRP_REPCODE { get; set; }
        public virtual DbSet<GL_CLOSING_STOCK> GL_CLOSING_STOCK { get; set; }
        public virtual DbSet<GL_CONT_HIST> GL_CONT_HIST { get; set; }
        public virtual DbSet<GL_CONTROL> GL_CONTROL { get; set; }
        public virtual DbSet<GL_POSTBATCH> GL_POSTBATCH { get; set; }
        public virtual DbSet<GL_REPORTCOLS> GL_REPORTCOLS { get; set; }
        public virtual DbSet<GL_REPORTROWS> GL_REPORTROWS { get; set; }
        public virtual DbSet<GL_REPORTS> GL_REPORTS { get; set; }
        public virtual DbSet<GL_SJLINES> GL_SJLINES { get; set; }
        public virtual DbSet<GL_SJPOSTRUN> GL_SJPOSTRUN { get; set; }
        public virtual DbSet<GLACCS> GLACCS { get; set; }
        public virtual DbSet<GLBATCH> GLBATCH { get; set; }
        public virtual DbSet<GLBUDGETS> GLBUDGETS { get; set; }
        public virtual DbSet<GLBUDGETS_HDR> GLBUDGETS_HDR { get; set; }
        public virtual DbSet<GLCHART_BUSINESS> GLCHART_BUSINESS { get; set; }
        public virtual DbSet<GLCHART_INDUSTRY> GLCHART_INDUSTRY { get; set; }
        public virtual DbSet<GLMOVEMENTS> GLMOVEMENTS { get; set; }
        public virtual DbSet<GLREPORT_BATCH> GLREPORT_BATCH { get; set; }
        public virtual DbSet<GLREPORT_BATCH_LINE> GLREPORT_BATCH_LINE { get; set; }
        public virtual DbSet<GLSUBACCS> GLSUBACCS { get; set; }
        public virtual DbSet<GLTRANS> GLTRANS { get; set; }
        public virtual DbSet<GLTRANS_ARCHIVE> GLTRANS_ARCHIVE { get; set; }
        public virtual DbSet<GRAPH_SERIES> GRAPH_SERIES { get; set; }
        public virtual DbSet<IDENTIFIERS> IDENTIFIERS { get; set; }
        public virtual DbSet<INV_FIELD_NAMES> INV_FIELD_NAMES { get; set; }
        public virtual DbSet<INV_NUMBERS> INV_NUMBERS { get; set; }
        public virtual DbSet<INWARDS_GOODS> INWARDS_GOODS { get; set; }
        public virtual DbSet<INWARDS_GOODS_COSTS> INWARDS_GOODS_COSTS { get; set; }
        public virtual DbSet<INWARDS_GOODS_LINES> INWARDS_GOODS_LINES { get; set; }
        public virtual DbSet<JOB_CATEGORIES> JOB_CATEGORIES { get; set; }
        public virtual DbSet<JOB_CONTRACT_BILLINGS> JOB_CONTRACT_BILLINGS { get; set; }
        public virtual DbSet<JOB_COSTGROUPS> JOB_COSTGROUPS { get; set; }
        public virtual DbSet<JOB_COSTTYPES> JOB_COSTTYPES { get; set; }
        public virtual DbSet<JOB_HIST> JOB_HIST { get; set; }
        public virtual DbSet<JOB_OTHER_REPORTS> JOB_OTHER_REPORTS { get; set; }
        public virtual DbSet<JOB_OUTPUT_ITEMS> JOB_OUTPUT_ITEMS { get; set; }
        public virtual DbSet<JOB_QUOTE_OPTIONS> JOB_QUOTE_OPTIONS { get; set; }
        public virtual DbSet<JOB_RESOURCE_ALLOCATION> JOB_RESOURCE_ALLOCATION { get; set; }
        public virtual DbSet<JOB_RETENTION_LEVELS> JOB_RETENTION_LEVELS { get; set; }
        public virtual DbSet<JOB_STATUS> JOB_STATUS { get; set; }
        public virtual DbSet<JOB_STATUS_CONSTRAINT> JOB_STATUS_CONSTRAINT { get; set; }
        public virtual DbSet<JOB_TIMESHEET_ALLOWANCE> JOB_TIMESHEET_ALLOWANCE { get; set; }
        public virtual DbSet<JOB_TIMESHEET_ALLOWANCE_TYPES> JOB_TIMESHEET_ALLOWANCE_TYPES { get; set; }
        public virtual DbSet<JOB_TIMESHEETS> JOB_TIMESHEETS { get; set; }
        public virtual DbSet<JOB_TIMESHEETS_RATES> JOB_TIMESHEETS_RATES { get; set; }
        public virtual DbSet<JOB_TRANSACTIONS> JOB_TRANSACTIONS { get; set; }
        public virtual DbSet<JOB_TYPES> JOB_TYPES { get; set; }
        public virtual DbSet<JOBCOST_FLAGS> JOBCOST_FLAGS { get; set; }
        public virtual DbSet<JOBCOST_FLAGSDESC> JOBCOST_FLAGSDESC { get; set; }
        public virtual DbSet<JOBCOST_GENERAL_INFO> JOBCOST_GENERAL_INFO { get; set; }
        public virtual DbSet<JOBCOST_HDR> JOBCOST_HDR { get; set; }
        public virtual DbSet<JOBCOST_LINES> JOBCOST_LINES { get; set; }
        public virtual DbSet<JOBCOST_PROJ> JOBCOST_PROJ { get; set; }
        public virtual DbSet<JOBCOST_RESOURCE> JOBCOST_RESOURCE { get; set; }
        public virtual DbSet<KIT_SERIAL_INFO> KIT_SERIAL_INFO { get; set; }
        public virtual DbSet<LEDGER_PERIODS> LEDGER_PERIODS { get; set; }
        public virtual DbSet<LEGENDS> LEGENDS { get; set; }
        public virtual DbSet<MANIFESTS> MANIFESTS { get; set; }
        public virtual DbSet<MANREP> MANREP { get; set; }
        public virtual DbSet<MANREP_BRANCH> MANREP_BRANCH { get; set; }
        public virtual DbSet<MANREP_BRANCH_BUDGET> MANREP_BRANCH_BUDGET { get; set; }
        public virtual DbSet<MANREP_BUDGET> MANREP_BUDGET { get; set; }
        public virtual DbSet<MANREP_CLOSING_STOCK> MANREP_CLOSING_STOCK { get; set; }
        public virtual DbSet<MANREP_DAYPLAN> MANREP_DAYPLAN { get; set; }
        public virtual DbSet<MANREP_PERIOD> MANREP_PERIOD { get; set; }
        public virtual DbSet<MANREP_SALESPERSON> MANREP_SALESPERSON { get; set; }
        public virtual DbSet<MANREP_SETUP> MANREP_SETUP { get; set; }
        public virtual DbSet<MANREP_STOCK> MANREP_STOCK { get; set; }
        public virtual DbSet<MENU_ASSIGNMENTS> MENU_ASSIGNMENTS { get; set; }
        public virtual DbSet<MENU_COLLECTION> MENU_COLLECTION { get; set; }
        public virtual DbSet<MENU_HDR> MENU_HDR { get; set; }
        public virtual DbSet<MENU_LINES> MENU_LINES { get; set; }
        public virtual DbSet<MENU_MASTER> MENU_MASTER { get; set; }
        public virtual DbSet<MENU_OPTIONS> MENU_OPTIONS { get; set; }
        public virtual DbSet<MENU_PROCEDURES> MENU_PROCEDURES { get; set; }
        public virtual DbSet<MODULE_SECURITY> MODULE_SECURITY { get; set; }
        public virtual DbSet<MONTHSTABLE> MONTHSTABLE { get; set; }
        public virtual DbSet<NARRATIVES> NARRATIVES { get; set; }
        public virtual DbSet<OBJECT_DATA> OBJECT_DATA { get; set; }
        public virtual DbSet<OBJECT_LOCK> OBJECT_LOCK { get; set; }
        public virtual DbSet<ONLINE_USERS> ONLINE_USERS { get; set; }
        public virtual DbSet<OPPORTUNITY> OPPORTUNITY { get; set; }
        public virtual DbSet<OPPORTUNITY_HIST> OPPORTUNITY_HIST { get; set; }
        public virtual DbSet<OPPORTUNITY_LEAD> OPPORTUNITY_LEAD { get; set; }
        public virtual DbSet<OPPORTUNITY_LINES> OPPORTUNITY_LINES { get; set; }
        public virtual DbSet<OPPORTUNITY_QUOTE> OPPORTUNITY_QUOTE { get; set; }
        public virtual DbSet<OPPORTUNITY_QUOTE_OPTIONS> OPPORTUNITY_QUOTE_OPTIONS { get; set; }
        public virtual DbSet<OPPORTUNITY_STAGE> OPPORTUNITY_STAGE { get; set; }
        public virtual DbSet<OPPORTUNITY_STAGE_CONSTRAINT> OPPORTUNITY_STAGE_CONSTRAINT { get; set; }
        public virtual DbSet<OPPORTUNITY_TYPE> OPPORTUNITY_TYPE { get; set; }
        public virtual DbSet<PAYMENT_DENOMINATIONS> PAYMENT_DENOMINATIONS { get; set; }
        public virtual DbSet<PAYMENT_GROUP> PAYMENT_GROUP { get; set; }
        public virtual DbSet<PAYMENT_TYPES> PAYMENT_TYPES { get; set; }
        public virtual DbSet<PAYRUN_HDR> PAYRUN_HDR { get; set; }
        public virtual DbSet<PAYRUN_LINES> PAYRUN_LINES { get; set; }
        public virtual DbSet<PERIOD_STATUS> PERIOD_STATUS { get; set; }
        public virtual DbSet<PERIODS_DEFN> PERIODS_DEFN { get; set; }
        public virtual DbSet<PERIODS_DEFN_NEXTFINYR> PERIODS_DEFN_NEXTFINYR { get; set; }
        public virtual DbSet<POS_COUNT> POS_COUNT { get; set; }
        public virtual DbSet<POS_COUNT_DENOM> POS_COUNT_DENOM { get; set; }
        public virtual DbSet<POS_SETTLEMENT> POS_SETTLEMENT { get; set; }
        public virtual DbSet<POS_SHIFTS> POS_SHIFTS { get; set; }
        public virtual DbSet<PRICE_NAMES> PRICE_NAMES { get; set; }
        public virtual DbSet<PRICE_SCHEDULE> PRICE_SCHEDULE { get; set; }
        public virtual DbSet<PRINT_LOG> PRINT_LOG { get; set; }
        public virtual DbSet<PROFILE> PROFILE { get; set; }
        public virtual DbSet<PROFILE_CONFLICT_SET> PROFILE_CONFLICT_SET { get; set; }
        public virtual DbSet<PROFILE_CONFLICT_SET_MEMBERS> PROFILE_CONFLICT_SET_MEMBERS { get; set; }
        public virtual DbSet<PROFILE_CONSTRAINT_VALUES> PROFILE_CONSTRAINT_VALUES { get; set; }
        public virtual DbSet<PROFILE_CONSTRAINTS> PROFILE_CONSTRAINTS { get; set; }
        public virtual DbSet<PROFILE_FIELDS> PROFILE_FIELDS { get; set; }
        public virtual DbSet<PROFILE_FORMS> PROFILE_FORMS { get; set; }
        public virtual DbSet<PROFILE_GRIDS> PROFILE_GRIDS { get; set; }
        public virtual DbSet<PROFILE_TYPE_INFO> PROFILE_TYPE_INFO { get; set; }
        public virtual DbSet<PROFILE_TYPES> PROFILE_TYPES { get; set; }
        public virtual DbSet<PROFILE_VALUES> PROFILE_VALUES { get; set; }
        public virtual DbSet<PROSPECT_CONTACTS> PROSPECT_CONTACTS { get; set; }
        public virtual DbSet<PROSPECT_HIST> PROSPECT_HIST { get; set; }
        public virtual DbSet<PROSPECTS> PROSPECTS { get; set; }
        public virtual DbSet<PURCHORD_HDR> PURCHORD_HDR { get; set; }
        public virtual DbSet<PURCHORD_HDR_ARCHIVE> PURCHORD_HDR_ARCHIVE { get; set; }
        public virtual DbSet<PURCHORD_HIST> PURCHORD_HIST { get; set; }
        public virtual DbSet<PURCHORD_LINES> PURCHORD_LINES { get; set; }
        public virtual DbSet<PURCHORD_LINES_ARCHIVE> PURCHORD_LINES_ARCHIVE { get; set; }
        public virtual DbSet<QUICK_INSERT_TABSHEET> QUICK_INSERT_TABSHEET { get; set; }
        public virtual DbSet<REASON_CLASS> REASON_CLASS { get; set; }
        public virtual DbSet<REASON_EVENTS> REASON_EVENTS { get; set; }
        public virtual DbSet<REASONS> REASONS { get; set; }
        public virtual DbSet<RELATIONSHIPS> RELATIONSHIPS { get; set; }
        public virtual DbSet<RELATIONSHIPS_DEFN> RELATIONSHIPS_DEFN { get; set; }
        public virtual DbSet<RELATIONSHIPS_SET> RELATIONSHIPS_SET { get; set; }
        public virtual DbSet<SALES_MESSAGES> SALES_MESSAGES { get; set; }
        public virtual DbSet<SALESORD_HDR> SALESORD_HDR { get; set; }
        public virtual DbSet<SALESORD_HDR_ARCHIVE> SALESORD_HDR_ARCHIVE { get; set; }
        public virtual DbSet<SALESORD_LINES> SALESORD_LINES { get; set; }
        public virtual DbSet<SALESORD_LINES_ARCHIVE> SALESORD_LINES_ARCHIVE { get; set; }
        public virtual DbSet<SALESORD_STATUS> SALESORD_STATUS { get; set; }
        public virtual DbSet<SALESORDHIST> SALESORDHIST { get; set; }
        public virtual DbSet<SHIPMENT_COST_DETAILS> SHIPMENT_COST_DETAILS { get; set; }
        public virtual DbSet<SHIPMENT_HDR> SHIPMENT_HDR { get; set; }
        public virtual DbSet<SHIPMENT_METHODS> SHIPMENT_METHODS { get; set; }
        public virtual DbSet<SHIPMENT_STATUS> SHIPMENT_STATUS { get; set; }
        public virtual DbSet<SMTP_ACCOUNTS> SMTP_ACCOUNTS { get; set; }
        public virtual DbSet<STAFF> STAFF { get; set; }
        public virtual DbSet<STOCK_CLASSIFICATIONS> STOCK_CLASSIFICATIONS { get; set; }
        public virtual DbSet<STOCK_COLOUR> STOCK_COLOUR { get; set; }
        public virtual DbSet<STOCK_CONT_HIST> STOCK_CONT_HIST { get; set; }
        public virtual DbSet<STOCK_GROUP2S> STOCK_GROUP2S { get; set; }
        public virtual DbSet<STOCK_GROUPS> STOCK_GROUPS { get; set; }
        public virtual DbSet<STOCK_ITEMS> STOCK_ITEMS { get; set; }
        public virtual DbSet<STOCK_LOC_INFO> STOCK_LOC_INFO { get; set; }
        public virtual DbSet<STOCK_LOCATIONS> STOCK_LOCATIONS { get; set; }
        public virtual DbSet<STOCK_PRICEGROUPS> STOCK_PRICEGROUPS { get; set; }
        public virtual DbSet<STOCK_REQUESTLINES> STOCK_REQUESTLINES { get; set; }
        public virtual DbSet<STOCK_REQUESTS> STOCK_REQUESTS { get; set; }
        public virtual DbSet<STOCK_REQUESTTYPES> STOCK_REQUESTTYPES { get; set; }
        public virtual DbSet<STOCK_SERIALNOS> STOCK_SERIALNOS { get; set; }
        public virtual DbSet<STOCK_SIZE> STOCK_SIZE { get; set; }
        public virtual DbSet<STOCK_TRANS> STOCK_TRANS { get; set; }
        public virtual DbSet<STOCK_TRANS_ARCHIVE> STOCK_TRANS_ARCHIVE { get; set; }
        public virtual DbSet<STOCK_TRANS_HDR> STOCK_TRANS_HDR { get; set; }
        public virtual DbSet<STOCK_TRANS_SERIALS> STOCK_TRANS_SERIALS { get; set; }
        public virtual DbSet<STOCK_UNITDEFINITION> STOCK_UNITDEFINITION { get; set; }
        public virtual DbSet<STOCK_WEB> STOCK_WEB { get; set; }
        public virtual DbSet<STOCKTAKE_CTRL> STOCKTAKE_CTRL { get; set; }
        public virtual DbSet<STOCKTAKE_EVENTS> STOCKTAKE_EVENTS { get; set; }
        public virtual DbSet<STOCKTAKE_TOTALS> STOCKTAKE_TOTALS { get; set; }
        public virtual DbSet<SU_WARRANTY> SU_WARRANTY { get; set; }
        public virtual DbSet<SU_WARRANTYTYPE> SU_WARRANTYTYPE { get; set; }
        public virtual DbSet<SUPPLIER_STOCK_ITEMS> SUPPLIER_STOCK_ITEMS { get; set; }
        public virtual DbSet<TASK_STATUSES> TASK_STATUSES { get; set; }
        public virtual DbSet<TASK_TYPES> TASK_TYPES { get; set; }
        public virtual DbSet<TASKS> TASKS { get; set; }
        public virtual DbSet<TAX_KEY_POINT> TAX_KEY_POINT { get; set; }
        public virtual DbSet<TAX_RATES> TAX_RATES { get; set; }
        public virtual DbSet<TAX_RETURN> TAX_RETURN { get; set; }
        public virtual DbSet<TAX_RETURN_POINT> TAX_RETURN_POINT { get; set; }
        public virtual DbSet<TERRITORYBUDGET> TERRITORYBUDGET { get; set; }
        public virtual DbSet<TERRITORYELEMENTS> TERRITORYELEMENTS { get; set; }
        public virtual DbSet<TERRITORYHDR> TERRITORYHDR { get; set; }
        public virtual DbSet<TIME_BILLING> TIME_BILLING { get; set; }
        public virtual DbSet<USER_DEF_BANK_COLS> USER_DEF_BANK_COLS { get; set; }
        public virtual DbSet<VERIFICATION_CLASS> VERIFICATION_CLASS { get; set; }
        public virtual DbSet<VERIFICATION_HDR> VERIFICATION_HDR { get; set; }
        public virtual DbSet<VERIFICATION_LINES> VERIFICATION_LINES { get; set; }
        public virtual DbSet<WEEK_DATES> WEEK_DATES { get; set; }
        public virtual DbSet<WORKSORD_HDR> WORKSORD_HDR { get; set; }
        public virtual DbSet<WORKSORD_LINES> WORKSORD_LINES { get; set; }
        public virtual DbSet<WORKSORD_STATUS> WORKSORD_STATUS { get; set; }
        public virtual DbSet<X_ER_REPORT> X_ER_REPORT { get; set; }
        public virtual DbSet<X_ER_SETUP> X_ER_SETUP { get; set; }
        public virtual DbSet<X_HBIZ_NOTIFICATIONS> X_HBIZ_NOTIFICATIONS { get; set; }
        public virtual DbSet<X_HBIZ_PURCH_REQ_COMMENTS> X_HBIZ_PURCH_REQ_COMMENTS { get; set; }
        public virtual DbSet<X_HBIZ_PURCH_REQ_HDR> X_HBIZ_PURCH_REQ_HDR { get; set; }
        public virtual DbSet<X_HBIZ_PURCH_REQ_HDR_HISTORY> X_HBIZ_PURCH_REQ_HDR_HISTORY { get; set; }
        public virtual DbSet<X_HBIZ_PURCH_REQ_LINES> X_HBIZ_PURCH_REQ_LINES { get; set; }
        public virtual DbSet<X_HBIZ_PURCH_REQ_STATUS_VALUES> X_HBIZ_PURCH_REQ_STATUS_VALUES { get; set; }
        public virtual DbSet<X_HBIZ_PURCH_REQ_SUBSCRIPTIONS> X_HBIZ_PURCH_REQ_SUBSCRIPTIONS { get; set; }
        public virtual DbSet<X_HBIZ_REPORTS_TO_STAFF> X_HBIZ_REPORTS_TO_STAFF { get; set; }
        public virtual DbSet<X_PURCH_REQ_COMMENTS_FILES> X_PURCH_REQ_COMMENTS_FILES { get; set; }
        public virtual DbSet<ACCS_BALANCE> ACCS_BALANCE { get; set; }
        public virtual DbSet<CAMPAIGN_CONTACT_LISTS> CAMPAIGN_CONTACT_LISTS { get; set; }
        public virtual DbSet<CAMPAIGN_TYPE> CAMPAIGN_TYPE { get; set; }
        public virtual DbSet<CAMPAIGN_WAVE_AUDIT> CAMPAIGN_WAVE_AUDIT { get; set; }
        public virtual DbSet<CAMPAIGN_WAVE_CONTACT_LISTS> CAMPAIGN_WAVE_CONTACT_LISTS { get; set; }
        public virtual DbSet<CHEQUE_GEN_INFO> CHEQUE_GEN_INFO { get; set; }
        public virtual DbSet<CHEQUE_PRINT> CHEQUE_PRINT { get; set; }
        public virtual DbSet<CHEQUE_PRINT_HDR> CHEQUE_PRINT_HDR { get; set; }
        public virtual DbSet<COMMUNICATION_PROCESSES> COMMUNICATION_PROCESSES { get; set; }
        public virtual DbSet<CONTACT_LIST_TYPE> CONTACT_LIST_TYPE { get; set; }
        public virtual DbSet<GL_SJHDR> GL_SJHDR { get; set; }
        public virtual DbSet<GLBATCH_NUMBERS> GLBATCH_NUMBERS { get; set; }
        public virtual DbSet<GLREPORTHEADER> GLREPORTHEADER { get; set; }
        public virtual DbSet<PAYROLL_COSTCENTRE> PAYROLL_COSTCENTRE { get; set; }
        public virtual DbSet<PAYROLL_EMPLOYEES> PAYROLL_EMPLOYEES { get; set; }
        public virtual DbSet<PAYROLL_INFO> PAYROLL_INFO { get; set; }
        public virtual DbSet<PAYROLL_PAYRATES> PAYROLL_PAYRATES { get; set; }
        public virtual DbSet<POSTCODES> POSTCODES { get; set; }
        public virtual DbSet<PREVIOUS_ITEMS> PREVIOUS_ITEMS { get; set; }
        public virtual DbSet<STOCK_RECEIPTS> STOCK_RECEIPTS { get; set; }
        public virtual DbSet<STOCKREQUIREMENT> STOCKREQUIREMENT { get; set; }
        public virtual DbSet<TAX_RETURN_ALLOC> TAX_RETURN_ALLOC { get; set; }
        public virtual DbSet<TAX_RETURN_CALC> TAX_RETURN_CALC { get; set; }
        public virtual DbSet<TAX_RETURN_TRANS> TAX_RETURN_TRANS { get; set; }
        public virtual DbSet<X_DEPARTMENT> X_DEPARTMENT { get; set; }
        public virtual DbSet<X_JOB_TRANSACTIONS_DETAIL> X_JOB_TRANSACTIONS_DETAIL { get; set; }
        public virtual DbSet<X_JOB_TRANSACTIONS_DETAIL_SeqNo> X_JOB_TRANSACTIONS_DETAIL_SeqNo { get; set; }
        public virtual DbSet<X_JOB_TIMESHEETS> X_JOB_TIMESHEETS { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<PrimeroEntities>(null);
            Database.CommandTimeout = 100000;
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<ADJUSTMENT_TYPES>()
            //    .Property(e => e.ATDESC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<ADVERT_TYPES>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<ANALYSIS>()
            //    .Property(e => e.CODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<ANALYSIS>()
            //    .Property(e => e.NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<ANALYSIS>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<ANALYSIS>()
            //    .Property(e => e.ISACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<ANALYSIS>()
            //    .Property(e => e.CODE_TEMPLATE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<ANALYSIS>()
            //    .Property(e => e.X_BUDGET)
            //    .HasPrecision(19, 4);

            //modelBuilder.Entity<ANALYSIS_CODES>()
            //    .Property(e => e.CODENAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<ANALYSIS_MATRIX>()
            //    .Property(e => e.TRAN_TYPE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<ANALYSIS_TYPES>()
            //    .Property(e => e.TRAN_TYPE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<ANALYSIS_TYPES>()
            //    .Property(e => e.TRAN_TABLE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<ANALYSIS_TYPES>()
            //    .Property(e => e.DESCRIPT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<AUDIT_TRAIL>()
            //    .Property(e => e.TABLENAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<AUDIT_TRAIL>()
            //    .Property(e => e.FIELDNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<AUDIT_TRAIL>()
            //    .Property(e => e.OLD_VALUE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<AUDIT_TRAIL>()
            //    .Property(e => e.NEW_VALUE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<AUDIT_TRAIL>()
            //    .Property(e => e.MODIFIEDBY)
            //    .IsUnicode(false);

            //modelBuilder.Entity<AUDITOR_EVENTS>()
            //    .Property(e => e.CLASSNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<AUDITOR_EVENTS>()
            //    .Property(e => e.EVENTNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<AUDITOR_EVENTS>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<AUDITOR_SAVEPROCS>()
            //    .Property(e => e.LOCATION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<AUDITOR_SAVEPROCS>()
            //    .Property(e => e.PURGE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<AUDITOR_SAVEPROCS>()
            //    .Property(e => e.ENABLED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<AUDITOR_SETTINGS>()
            //    .Property(e => e.TERMINALID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<AUDITOR_SETTINGS>()
            //    .Property(e => e.ENABLED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<AUDITOR_TERMINALS>()
            //    .Property(e => e.TERMINALID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<AUDITOR_TERMINALS>()
            //    .Property(e => e.COMPUTERID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<AUDITOR_TERMINALS>()
            //    .Property(e => e.ENABLED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<AUDITOR_TERMINALS>()
            //    .Property(e => e.EXCEPTIONLOGGING)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<AUTO_PURCH>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<AUTO_PURCH>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANK_DATA_FORMAT>()
            //    .Property(e => e.DATA_FORMAT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANK_DATA_FORMAT>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANK_DATA_TYPE>()
            //    .Property(e => e.TYPE_DESC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANK_REC_AUTO_MATCH>()
            //    .Property(e => e.GLCHEQUENO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANK_REC_AUTO_MATCH>()
            //    .Property(e => e.GLDETAILS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANK_REC_AUTO_MATCH>()
            //    .Property(e => e.BRCHEQUENO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANK_REC_AUTO_MATCH>()
            //    .Property(e => e.BRTRANSDATE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANK_REC_AUTO_MATCH>()
            //    .Property(e => e.BRDETAILS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANK_REC_AUTO_MATCH>()
            //    .Property(e => e.ISRECONCILE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANK_REC_LOG>()
            //    .Property(e => e.RECTYPE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANK_REC_LOG>()
            //    .Property(e => e.CSVFILE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANK_REC_LOG>()
            //    .Property(e => e.LOCKED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANK_REC_LOG>()
            //    .Property(e => e.PRE6180)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANK_REC_SETUP>()
            //    .Property(e => e.rectype)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANK_REC_SETUP>()
            //    .Property(e => e.LASTCSVFILE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANK_REC_SETUP>()
            //    .Property(e => e.MTS_RECEIPTS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANK_REC_SETUP>()
            //    .Property(e => e.MTS_REVERSALS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANK_REC_SETUP>()
            //    .Property(e => e.DR_MATCH_FIELD)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANK_REC_UPLOADS>()
            //    .Property(e => e.CHQNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANK_REC_UPLOADS>()
            //    .Property(e => e.DATESTR)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANK_REC_UPLOADS>()
            //    .Property(e => e.DETAILS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANK_REC_UPLOADS>()
            //    .Property(e => e.MTS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANK_REC_UPLOADS>()
            //    .Property(e => e.REFERENCE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANK_REC_UPLOADS>()
            //    .Property(e => e.PARTICULARS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANK_TEMPLATES>()
            //    .Property(e => e.BANKNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANK_TEMPLATES>()
            //    .Property(e => e.BANKFILE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANKBATCH_HDR>()
            //    .Property(e => e.NARRATIVE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANKBATCH_HDR>()
            //    .Property(e => e.GLPOSTED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANKBATCH_HDR>()
            //    .Property(e => e.TERMINAL_ID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANKBATCH_HDR>()
            //    .Property(e => e.BATCH_LOCKED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANKBATCH_HDR>()
            //    .Property(e => e.READY_TO_POST)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANKS>()
            //    .Property(e => e.NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANKS>()
            //    .Property(e => e.PATH)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANKS>()
            //    .Property(e => e.FILENAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANKS>()
            //    .Property(e => e.EXTENSION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANKS>()
            //    .Property(e => e.TRNSFR_TYPE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANKS>()
            //    .Property(e => e.USER_BANK)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANKS>()
            //    .Property(e => e.USER_NUMBER)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANKS>()
            //    .Property(e => e.PAYER_BANK_ACC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANKS>()
            //    .Property(e => e.BSB_NUMBER)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BANKS>()
            //    .Property(e => e.COMPANY_TRADING_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BATCH_QUANTITIES>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BATCH_QUANTITIES>()
            //    .Property(e => e.BATCHCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BATCH_QUANTITIES>()
            //    .Property(e => e.REFERENCE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BATCH_STOCKTAKE>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BATCH_STOCKTAKE>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BATCH_STOCKTAKE>()
            //    .Property(e => e.BATCHCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BATCH_STOCKTAKE>()
            //    .Property(e => e.BINCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BILLOMAT_HDR>()
            //    .Property(e => e.BILLCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BILLOMAT_HDR>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BILLOMAT_HDR>()
            //    .Property(e => e.OUTPUT_CODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BILLOMAT_HDR>()
            //    .Property(e => e.HIDE_LINES)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<BILLOMAT_HDR>()
            //    .Property(e => e.NOTES)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BILLOMAT_HDR>()
            //    .Property(e => e.ISACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<BILLOMAT_HDR>()
            //    .Property(e => e.BOMTYPE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<BILLOMAT_HDR>()
            //    .Property(e => e.AUTOBUILD)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<BILLOMAT_LINES>()
            //    .Property(e => e.BILLCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BILLOMAT_LINES>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BILLOMAT_LINES>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BILLOMAT_LINES>()
            //    .Property(e => e.VARIANTLINE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<BILLOMAT_TEMP>()
            //    .Property(e => e.REFERENCECODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BILLOMAT_TEMP>()
            //    .Property(e => e.BILLCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BILLOMAT_TEMP>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BPAY_TYPES>()
            //    .Property(e => e.CODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BPAY_TYPES>()
            //    .Property(e => e.NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BRANCHES>()
            //    .Property(e => e.BRANCHNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BRANCHES>()
            //    .Property(e => e.BCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<BRANCHES>()
            //    .HasMany(e => e.CRM_BUDGET)
            //    .WithMany(e => e.BRANCHES)
            //    .Map(m => m.ToTable("CRM_BUDGET_EXCLUDED_BRANCHES").MapLeftKey("BRANCHNO").MapRightKey("BUDGET_SEQNO"));

            //modelBuilder.Entity<CAMPAIGN>()
            //    .Property(e => e.TITLE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CAMPAIGN>()
            //    .Property(e => e.REFERENCE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CAMPAIGN>()
            //    .Property(e => e.DESCRIPT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CAMPAIGN_HIST>()
            //    .Property(e => e.SUBJECT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CAMPAIGN_HIST>()
            //    .Property(e => e.NOTE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CAMPAIGN_HIST>()
            //    .Property(e => e.OUTLOOK_LINK)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CAMPAIGN_STAGE>()
            //    .Property(e => e.DESCRIPT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CAMPAIGN_STAGE>()
            //    .Property(e => e.STATUSKEY)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CAMPAIGN_STAGE>()
            //    .Property(e => e.ADMIN_STAT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CAMPAIGN_STAGE>()
            //    .Property(e => e.LOCK_CAMPAIGN)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CAMPAIGN_STAGE>()
            //    .Property(e => e.ISARCHIVED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CAMPAIGN_STAGE>()
            //    .Property(e => e.ISCOMPLETE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CAMPAIGN_STAGE>()
            //    .Property(e => e.ISACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CAMPAIGN_STAGE>()
            //    .Property(e => e.WORKFLOW_CONSTRAINED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CAMPAIGN_STAGE_CONSTRAINT>()
            //    .Property(e => e.ISACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CAMPAIGN_STAGE_CONSTRAINT>()
            //    .Property(e => e.FROM_STATUS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CAMPAIGN_STAGE_CONSTRAINT>()
            //    .Property(e => e.TO_STATUS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CAMPAIGN_STAGE_CONSTRAINT>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CAMPAIGN_STAGE_CONSTRAINT>()
            //    .Property(e => e.SHORTDESC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CAMPAIGN_STAGE_CONSTRAINT>()
            //    .Property(e => e.TRACKEVENT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CAMPAIGN_WAVE>()
            //    .Property(e => e.DESCRIPT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CAMPAIGN_WAVE>()
            //    .Property(e => e.TRACKER_KEY)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CAMPAIGN_WAVE>()
            //    .Property(e => e.OPT_IN_URL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CAMPAIGN_WAVE>()
            //    .Property(e => e.OPT_OUT_URL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CAMPAIGN_WAVE>()
            //    .Property(e => e.LANDING_SITE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CAMPAIGN_WAVE>()
            //    .Property(e => e.PROCESSED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CAMPAIGN_WAVE>()
            //    .Property(e => e.COMPLETE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CAMPAIGN_WAVE>()
            //    .Property(e => e.SOCIAL_MEDIA_TEXT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CAMPAIGN_WAVE>()
            //    .Property(e => e.FACEBOOK_POST_ID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CAMPAIGN_WAVE>()
            //    .Property(e => e.TWITTER_POST_ID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CAMPAIGN_WAVE>()
            //    .Property(e => e.SETTINGS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CASHBOOKHEADER>()
            //    .Property(e => e.INITIALS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CASHBOOKHEADER>()
            //    .Property(e => e.CONSOLIDATE_GL_POSTINGS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CASHBOOKHEADER>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CASHBOOKHEADER>()
            //    .Property(e => e.TEMPONLY)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CASHBOOKHEADER>()
            //    .Property(e => e.CONSOLIDATE_DETAILS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CASHBOOKHEADER>()
            //    .Property(e => e.CHQNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CASHBOOKLINE>()
            //    .Property(e => e.LEDGER)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CASHBOOKLINE>()
            //    .Property(e => e.NAME_DETAILS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CASHBOOKLINE>()
            //    .Property(e => e.CHQ_REF)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CASHBOOKLINE>()
            //    .Property(e => e.TENDER_TYPE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CHECKLIST_WIDGET_NODES>()
            //    .Property(e => e.CAPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CHECKLIST_WIDGET_NODES>()
            //    .Property(e => e.PROC_DATA)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CHECKLIST_WIDGET_NODES>()
            //    .Property(e => e.STATUS_FIELDNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CHECKLIST_WIDGET_NODES>()
            //    .Property(e => e.STATUS_SPNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CHECKLIST_WIDGET_NODES>()
            //    .Property(e => e.EXPAND_NODE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CHECKLIST_WIDGET_NODES>()
            //    .Property(e => e.HELP_TEXT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CHEQUE_AUDIT>()
            //    .Property(e => e.PRINTOK)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CHEQUE_AUDIT>()
            //    .Property(e => e.GLPOSTED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CHEQUE_AUDIT>()
            //    .Property(e => e.MANUAL_CHQ)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CHEQUE_AUDIT>()
            //    .Property(e => e.DETAILS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<ClarityReport>()
            //    .Property(e => e.FILENAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<ClarityReport>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<ClarityReport>()
            //    .Property(e => e.PARAMS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<COMMON_PHRASES>()
            //    .Property(e => e.PHRASETEXT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<COMPANY_TYPES>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<COMPUTER>()
            //    .Property(e => e.COMPUTERID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<COMPUTER>()
            //    .Property(e => e.CLIENTNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<COMPUTER>()
            //    .Property(e => e.COMPUTERNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<COMPUTER>()
            //    .Property(e => e.EFTCAID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<COMPUTER>()
            //    .Property(e => e.EFTTRANSTATE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACT_HIST>()
            //    .Property(e => e.NOTE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACT_HIST>()
            //    .Property(e => e.SUBJECT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACT_HIST>()
            //    .Property(e => e.OUTLOOK_LINK)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACT_LIST>()
            //    .Property(e => e.TITLE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACT_LIST>()
            //    .Property(e => e.DESCRIPT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACT_LIST>()
            //    .Property(e => e.IS_PRIVATE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACT_LIST>()
            //    .Property(e => e.IS_ACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACT_LIST>()
            //    .Property(e => e.SETTINGS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACT_LIST_ITEM>()
            //    .Property(e => e.QUERY_INSERTED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACT_MARKETING_CLASSES>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.SALUTATION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.FIRSTNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.LASTNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.TITLE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.MOBILE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.DIRECTPHONE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.DIRECTFAX)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.HOMEPHONE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.EMAIL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.NOTES)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.ADDRESS1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.ADDRESS2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.ADDRESS3)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.ADDRESS4)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.ADDRESS5)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.POST_CODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.DELADDR1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.DELADDR2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.DELADDR3)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.DELADDR4)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.DELADDR5)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.DELADDR6)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.ISACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.SUB1)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.SUB2)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.SUB3)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.SUB4)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.SUB5)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.SUB6)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.SUB7)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.SUB8)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.SUB9)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.SUB10)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.SUB11)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.SUB12)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.SUB13)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.SUB14)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.SUB15)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.SUB16)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.SUB17)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.SUB18)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.SUB19)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.SUB20)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.FULLNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.SUB21)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.SUB22)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.SUB23)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.SUB24)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.SUB25)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.SUB26)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.MSN_ID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.YAHOO_ID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.SKYPE_ID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.SYNC_CONTACTS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.LINKEDIN)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.TWITTER)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.FACEBOOK)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACTS>()
            //    .Property(e => e.OPTOUT_EMARKETING)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<COUNTRY>()
            //    .Property(e => e.COUNTRY_CODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<COUNTRY>()
            //    .Property(e => e.COUNTRY_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<COUNTRY>()
            //    .Property(e => e.TAXNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<COUNTRY>()
            //    .Property(e => e.TAXNO_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<COUNTRY>()
            //    .Property(e => e.TAX_IN_PP)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<Courier_Location_Depot>()
            //    .Property(e => e.LocName)
            //    .IsUnicode(false);

            //modelBuilder.Entity<Courier_Location_Depot>()
            //    .Property(e => e.CourDepCode)
            //    .IsUnicode(false);

            //modelBuilder.Entity<COURIER_MANIFEST>()
            //    .Property(e => e.NOTES)
            //    .IsUnicode(false);

            //modelBuilder.Entity<COURIER_MANIFEST>()
            //    .Property(e => e.TICKETNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<COURIER_MANIFEST>()
            //    .Property(e => e.SERVICE_TYPE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<COURIER_MANIFEST>()
            //    .Property(e => e.ADDNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<COURIER_MANIFEST>()
            //    .Property(e => e.ADDRESS1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<COURIER_MANIFEST>()
            //    .Property(e => e.ADDRESS2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<COURIER_MANIFEST>()
            //    .Property(e => e.ADDRESS3)
            //    .IsUnicode(false);

            //modelBuilder.Entity<COURIER_MANIFEST>()
            //    .Property(e => e.ADDRESS4)
            //    .IsUnicode(false);

            //modelBuilder.Entity<COURIER_MANIFEST>()
            //    .Property(e => e.ADDRESS5)
            //    .IsUnicode(false);

            //modelBuilder.Entity<COURIER_MANIFEST>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<COURIER_MANIFEST>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<COURIER_MANIFEST>()
            //    .Property(e => e.LINK_TYPE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<COURIERS>()
            //    .Property(e => e.COURIER_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<COURIERS>()
            //    .Property(e => e.ISACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<COURIERS>()
            //    .Property(e => e.UPLOADEMAIL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<COURIERS>()
            //    .Property(e => e.UNLREF)
            //    .IsUnicode(false);

            //modelBuilder.Entity<COURIERS>()
            //    .Property(e => e.TRACK_TRACE_URL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<COURIERS>()
            //    .Property(e => e.CONNOTE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<COURIERS>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCGROUP2S>()
            //    .Property(e => e.GROUPNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCGROUP2S>()
            //    .Property(e => e.REPORTCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCGROUPS>()
            //    .Property(e => e.GROUPNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCGROUPS>()
            //    .Property(e => e.REPORTCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.ADDRESS1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.ADDRESS2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.ADDRESS3)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.ADDRESS4)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.ADDRESS5)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.PHONE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.FAX)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.EMAIL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.OPENITEM)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.DELADDR1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.DELADDR2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.DELADDR3)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.DELADDR4)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.DELADDR5)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.DELADDR6)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.NOTES)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.ALPHACODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.ALERT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.ISACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.BANK_ACCOUNT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.DEFAULT_CODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.BANK_ACC_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.TAXREG)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.POST_CODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.BSBNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.PP_TOPAY)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.STOPCREDIT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.PRIVATE_ACC)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.WEBSITE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.STATEMENT_TEXT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.REMITTANCE_METHOD)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.SEND_PAYMENT_REMITTANCE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.LINKEDIN)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.TWITTER)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.FACEBOOK)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ACCS>()
            //    .Property(e => e.X_PHONE2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_ALLOCATIONS>()
            //    .Property(e => e.TAKENUP)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONT_HIST>()
            //    .Property(e => e.NOTE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONT_HIST>()
            //    .Property(e => e.SUBJECT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONT_HIST>()
            //    .Property(e => e.OUTLOOK_LINK)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.DEFCONTACT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.SALUTATION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.FIRSTNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.LASTNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.TITLE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.MOBILE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.DIRECTPHONE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.DIRECTFAX)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.EMAIL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.HOMEPHONE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.NOTES)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.ADDRESS1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.ADDRESS2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.ADDRESS3)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.DELADDR1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.DELADDR2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.DELADDR3)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.DELADDR4)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.ISACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.SUB1)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.SUB2)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.SUB3)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.SUB4)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.POST_CODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.SUB5)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.SUB6)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.SUB7)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.SUB8)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.SUB9)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.SUB10)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.SUB11)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.SUB12)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.SUB13)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.SUB14)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.SUB15)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.SUB16)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.SUB17)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.SUB18)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.SUB19)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.SUB20)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.ADDRESS4)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.DELADDR5)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.DELADDR6)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.SUB21)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.SUB22)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.SUB23)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.SUB24)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.SUB25)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.SUB26)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.DEFACCOUNT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_CONTACTS>()
            //    .Property(e => e.ADDRESS5)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_INVLINES>()
            //    .Property(e => e.INVNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_INVLINES>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_INVLINES>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_INVLINES>()
            //    .Property(e => e.UPDATE_STOCK)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_INVLINES>()
            //    .Property(e => e.JOBCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_INVLINES>()
            //    .Property(e => e.CODETYPE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_INVLINES>()
            //    .Property(e => e.LINETAX_OVERRIDDEN)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_INVLINES>()
            //    .Property(e => e.BATCHCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_INVLINES>()
            //    .Property(e => e.KITCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_INVLINES>()
            //    .Property(e => e.LINKED_STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_INVLINES>()
            //    .Property(e => e.BOMTYPE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_INVLINES>()
            //    .Property(e => e.SHOWLINE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_INVLINES>()
            //    .Property(e => e.LINKEDSTATUS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_INVLINES>()
            //    .Property(e => e.BOMPRICING)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_INVLINES_SERIALS>()
            //    .Property(e => e.SERIALNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_LIST_NAME>()
            //    .Property(e => e.LIST_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_PRICES>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_PRICES>()
            //    .Property(e => e.ISACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_PRICES>()
            //    .Property(e => e.FIXED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_TRANS>()
            //    .Property(e => e.INVNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_TRANS>()
            //    .Property(e => e.REF1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_TRANS>()
            //    .Property(e => e.REF2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_TRANS>()
            //    .Property(e => e.REF3)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_TRANS>()
            //    .Property(e => e.NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_TRANS>()
            //    .Property(e => e.TAXINC)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_TRANS>()
            //    .Property(e => e.ANALYSIS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_TRANS>()
            //    .Property(e => e.ALLOCATED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_TRANS>()
            //    .Property(e => e.GLPOSTED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_TRANS>()
            //    .Property(e => e.AUTHORISED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_TRANS>()
            //    .Property(e => e.TAXRETCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_TRANS>()
            //    .Property(e => e.IMAGE_URL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CR_TRANS>()
            //    .Property(e => e.PP_BATCHREF)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CREDIT_STATUSES>()
            //    .Property(e => e.STATUSDESC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CREDIT_STATUSES>()
            //    .Property(e => e.ACTIVE_DR)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CREDIT_STATUSES>()
            //    .Property(e => e.ACTIVE_CR)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CREDIT_STATUSES>()
            //    .Property(e => e.BAL_WARNING_SQL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CREDIT_STATUSES>()
            //    .Property(e => e.WARNING_TEXT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CRM_BUDGET>()
            //    .Property(e => e.NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CRM_BUDGET>()
            //    .HasMany(e => e.CRM_BUDGET_LINE)
            //    .WithRequired(e => e.CRM_BUDGET)
            //    .HasForeignKey(e => e.BUDGET_SEQNO)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<CRM_BUDGET>()
            //    .HasMany(e => e.DR_ACCGROUP2S)
            //    .WithMany(e => e.CRM_BUDGET)
            //    .Map(m => m.ToTable("CRM_BUDGET_EXCLUDED_ACCGROUP2S").MapLeftKey("BUDGET_SEQNO").MapRightKey("ACCGROUP"));

            //modelBuilder.Entity<CRM_BUDGET>()
            //    .HasMany(e => e.DR_ACCGROUPS)
            //    .WithMany(e => e.CRM_BUDGET)
            //    .Map(m => m.ToTable("CRM_BUDGET_EXCLUDED_ACCGROUPS").MapLeftKey("BUDGET_SEQNO").MapRightKey("ACCGROUP"));

            //modelBuilder.Entity<CRM_BUDGET>()
            //    .HasMany(e => e.DR_ACCS)
            //    .WithMany(e => e.CRM_BUDGET)
            //    .Map(m => m.ToTable("CRM_BUDGET_EXCLUDED_DR_ACCS").MapLeftKey("BUDGET_SEQNO").MapRightKey("ACCNO"));

            //modelBuilder.Entity<CRM_BUDGET>()
            //    .HasMany(e => e.STAFF)
            //    .WithMany(e => e.CRM_BUDGET)
            //    .Map(m => m.ToTable("CRM_BUDGET_EXCLUDED_STAFF").MapLeftKey("BUDGET_SEQNO").MapRightKey("STAFFNO"));

            //modelBuilder.Entity<CRM_BUDGET>()
            //    .HasMany(e => e.STOCK_GROUP2S)
            //    .WithMany(e => e.CRM_BUDGET)
            //    .Map(m => m.ToTable("CRM_BUDGET_EXCLUDED_STOCK_GROUP2S").MapLeftKey("BUDGET_SEQNO").MapRightKey("GROUPNO"));

            //modelBuilder.Entity<CRM_BUDGET>()
            //    .HasMany(e => e.STOCK_GROUPS)
            //    .WithMany(e => e.CRM_BUDGET)
            //    .Map(m => m.ToTable("CRM_BUDGET_EXCLUDED_STOCK_GROUPS").MapLeftKey("BUDGET_SEQNO").MapRightKey("GROUPNO"));

            //modelBuilder.Entity<CRM_BUDGET>()
            //    .HasMany(e => e.STOCK_ITEMS)
            //    .WithMany(e => e.CRM_BUDGET)
            //    .Map(m => m.ToTable("CRM_BUDGET_EXCLUDED_STOCK_ITEMS").MapLeftKey("BUDGET_SEQNO").MapRightKey("STOCKCODE"));

            //modelBuilder.Entity<CRM_BUDGET_HDR>()
            //    .Property(e => e.NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CRM_BUDGET_HDR>()
            //    .HasMany(e => e.CRM_BUDGET)
            //    .WithRequired(e => e.CRM_BUDGET_HDR)
            //    .HasForeignKey(e => e.HEADER_SEQNO)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<CRM_BUDGET_LINE>()
            //    .Property(e => e.VALUE)
            //    .HasPrecision(19, 4);

            //modelBuilder.Entity<CRM_BUDGET_LINE>()
            //    .Property(e => e.LYACTUAL)
            //    .HasPrecision(19, 4);

            //modelBuilder.Entity<CRM_BUDGET_LINE>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CRM_BUDGET_LINE>()
            //    .Property(e => e.ACTUAL)
            //    .HasPrecision(19, 4);

            //modelBuilder.Entity<CURRENCIES>()
            //    .Property(e => e.CURRCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CURRENCIES>()
            //    .Property(e => e.CURRNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CURRENCIES>()
            //    .Property(e => e.CURRSYMBOL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CURRENCY_RATECHANGES>()
            //    .Property(e => e.CURRCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CUSTCLARITYREPORT>()
            //    .Property(e => e.FILENAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CUSTCLARITYREPORT>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CUSTCLARITYREPORT>()
            //    .Property(e => e.PARAMS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CUSTOM_REPORTS>()
            //    .Property(e => e.REPT_CODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CUSTOM_REPORTS>()
            //    .Property(e => e.REPT_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CUSTOM_REPORTS>()
            //    .Property(e => e.REPT_SQL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CUSTOM_REPORTS>()
            //    .Property(e => e.REPT_LOCKED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CUSTOM_REPORTS>()
            //    .Property(e => e.AUTO_TOTAL)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CUSTOM_REPORTS>()
            //    .Property(e => e.BLANK_ZEROS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CUSTOM_REPORTS>()
            //    .Property(e => e.LANDSCAPE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CUSTOM_REPORTS>()
            //    .Property(e => e.SHOWMAXIMISED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CUSTOM_REPORTS>()
            //    .Property(e => e.CLICK_TO)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CUSTOM_REPORTS>()
            //    .Property(e => e.SUBGROUP)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CUSTOM_REPORTS>()
            //    .Property(e => e.COLUMN_WIDTHS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CUSTOM_REPORTS>()
            //    .Property(e => e.COLUMN_NAMES)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CUSTOM_REPORTS>()
            //    .Property(e => e.GROUP_HEADER_SQL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CUSTOM_VIEWS>()
            //    .Property(e => e.VIEW_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CUSTOM_VIEWS>()
            //    .Property(e => e.VIEW_DETAILS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CUSTOMIZATION>()
            //    .Property(e => e.FORMNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CUSTOMIZATION>()
            //    .Property(e => e.TYPE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<D_DEBIT_HDR>()
            //    .Property(e => e.REF1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<D_DEBIT_HDR>()
            //    .Property(e => e.REF2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<D_DEBIT_LINES>()
            //    .Property(e => e.INVNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<D_DEBIT_LINES>()
            //    .Property(e => e.REF1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<D_DEBIT_LINES>()
            //    .Property(e => e.REF2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DASHBOARDS>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DASHBOARDS>()
            //    .Property(e => e.SETTINGS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DATE_RANGES>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DISPLAY_NAMES>()
            //    .Property(e => e.NAMEID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DISPLAY_NAMES>()
            //    .Property(e => e.DISPLAYNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DISPLAY_NAMES>()
            //    .Property(e => e.DISPLAYHINT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DISPMETHOD>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DOC_BATCH_HDR>()
            //    .Property(e => e.DOC_TYPE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DOC_BATCH_HDR>()
            //    .Property(e => e.PRIOR_PERIOD)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DOC_BATCH_HDR>()
            //    .Property(e => e.EMAIL)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DOC_BATCH_HDR>()
            //    .Property(e => e.TO_PRINTER)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DOC_BATCH_HDR>()
            //    .Property(e => e.PRINTER_URL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DOC_BATCH_HDR>()
            //    .Property(e => e.STATUS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DOC_BATCH_HDR>()
            //    .Property(e => e.ATTACHMENT_URL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DOC_BATCH_HDR>()
            //    .Property(e => e.COVERING_LETTER_CLM)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DOC_BATCH_HDR>()
            //    .Property(e => e.EMAIL_BODY_CLE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DOC_BATCH_HDR>()
            //    .Property(e => e.EMAIL_BODY)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DOC_BATCH_HDR>()
            //    .Property(e => e.FILTERSQL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DOC_BATCH_HDR>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DOC_BATCH_LINES>()
            //    .Property(e => e.PRIOR_PERIOD)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DOC_BATCH_LINES>()
            //    .Property(e => e.EMAIL)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DOC_BATCH_LINES>()
            //    .Property(e => e.TO_PRINTER)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DOC_BATCH_LINES>()
            //    .Property(e => e.PRINTER_URL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DOC_BATCH_LINES>()
            //    .Property(e => e.EMAIL_PRIMARY)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DOC_BATCH_LINES>()
            //    .Property(e => e.EMAIL_CC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DOC_BATCH_LINES>()
            //    .Property(e => e.STATUS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DOC_BATCH_LINES>()
            //    .Property(e => e.ATTACHMENT_URL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DOC_BATCH_LINES>()
            //    .Property(e => e.COVERING_LETTER_CLM)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DOC_BATCH_LINES>()
            //    .Property(e => e.EMAIL_BODY_CLE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DOC_BATCH_LINES>()
            //    .Property(e => e.COMPANYID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DOC_BATCH_LINES>()
            //    .Property(e => e.OPTOUT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DOCUMENTS>()
            //    .Property(e => e.MODULE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DOCUMENTS>()
            //    .Property(e => e.REFCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DOCUMENTS>()
            //    .Property(e => e.DOCCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DOCUMENTS>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DOCUMENTS>()
            //    .Property(e => e.FILENAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DOCUMENTS>()
            //    .Property(e => e.FILEPATH)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DOCUMENTS>()
            //    .Property(e => e.DOCLINK)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DOCUMENTS>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCGROUP2S>()
            //    .Property(e => e.GROUPNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCGROUP2S>()
            //    .Property(e => e.REPORTCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCGROUP2S>()
            //    .HasMany(e => e.CRM_BUDGET_LINE)
            //    .WithOptional(e => e.DR_ACCGROUP2S)
            //    .HasForeignKey(e => e.ACCGROUP2);

            //modelBuilder.Entity<DR_ACCGROUPS>()
            //    .Property(e => e.GROUPNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCGROUPS>()
            //    .Property(e => e.REPORTCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.ADDRESS1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.ADDRESS2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.ADDRESS3)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.ADDRESS4)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.ADDRESS5)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.DELADDR1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.DELADDR2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.DELADDR3)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.DELADDR4)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.DELADDR5)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.DELADDR6)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.PHONE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.FAX)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.EMAIL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.OPENITEM)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.NOTES)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.SORTCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.BANK)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.BANK_ACCOUNT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.BANK_ACC_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.BSBNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.D_DEBIT_FAX)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.D_DEBIT_PRINT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.D_DEBIT_EMAIL)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.BRANCH)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.DRAWER)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.AUTOBILLCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.ALPHACODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.PASS_WORD)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.ALERT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.STATEMENT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.ISACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.BAD_CHEQUE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.TAXREG)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.STOPCREDIT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.POST_CODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.FREIGHT_FREE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.KEEPTRANSACTIONS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.NEED_ORDERNO)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.ALLOW_RESTRICTED_STOCK)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.PRIVATE_ACC)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.ISTEMPLATE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.WEBSITE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.INVOICE_TYPE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.LINKEDIN)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.TWITTER)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.FACEBOOK)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.X_PHONE2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ACCS>()
            //    .Property(e => e.X_INDUSTRY)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ADDRESSES>()
            //    .Property(e => e.DELADDR1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ADDRESSES>()
            //    .Property(e => e.DELADDR2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ADDRESSES>()
            //    .Property(e => e.DELADDR3)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ADDRESSES>()
            //    .Property(e => e.DELADDR4)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ADDRESSES>()
            //    .Property(e => e.DELADDR5)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ADDRESSES>()
            //    .Property(e => e.DELADDR6)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_ALLOCATIONS>()
            //    .Property(e => e.TAKENUP)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONT_HIST>()
            //    .Property(e => e.NOTE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONT_HIST>()
            //    .Property(e => e.SUBJECT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONT_HIST>()
            //    .Property(e => e.OUTLOOK_LINK)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.DEFCONTACT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.SALUTATION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.FIRSTNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.LASTNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.TITLE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.MOBILE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.DIRECTPHONE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.DIRECTFAX)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.EMAIL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.HOMEPHONE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.NOTES)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.ADDRESS1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.ADDRESS2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.ADDRESS3)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.DELADDR1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.DELADDR2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.DELADDR3)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.DELADDR4)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.ISACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.SUB1)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.SUB2)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.SUB3)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.SUB4)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.POST_CODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.SUB5)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.SUB6)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.SUB7)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.SUB8)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.SUB9)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.SUB10)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.SUB11)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.SUB12)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.SUB13)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.SUB14)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.SUB15)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.SUB16)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.SUB17)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.SUB18)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.SUB19)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.SUB20)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.ADDRESS4)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.DELADDR5)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.DELADDR6)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.SUB21)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.SUB22)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.SUB23)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.SUB24)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.SUB25)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.SUB26)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.DEFACCOUNT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_CONTACTS>()
            //    .Property(e => e.ADDRESS5)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_INVLINES>()
            //    .Property(e => e.INVNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_INVLINES>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_INVLINES>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_INVLINES>()
            //    .Property(e => e.CUSTOMFIELD)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_INVLINES>()
            //    .Property(e => e.UPDATE_STOCK)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_INVLINES>()
            //    .Property(e => e.JOBCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_INVLINES>()
            //    .Property(e => e.BATCHCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_INVLINES>()
            //    .Property(e => e.HIDDEN)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_INVLINES>()
            //    .Property(e => e.LINETAX_OVERRIDDEN)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_INVLINES>()
            //    .Property(e => e.SERIALNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_INVLINES>()
            //    .Property(e => e.FREIGHT_FREE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_INVLINES>()
            //    .Property(e => e.PRICE_OVERRIDDEN)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_INVLINES>()
            //    .Property(e => e.KITCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_INVLINES>()
            //    .Property(e => e.LINKED_STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_INVLINES>()
            //    .Property(e => e.BOMTYPE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_INVLINES>()
            //    .Property(e => e.SHOWLINE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_INVLINES>()
            //    .Property(e => e.LINKEDSTATUS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_INVLINES>()
            //    .Property(e => e.BOMPRICING)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_INVLINES_SERIALS>()
            //    .Property(e => e.SERIALNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_PRICE_POLICY>()
            //    .Property(e => e.CUSTOMER_REF)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_PRICE_POLICY>()
            //    .Property(e => e.POLICY_REF)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_PRICE_POLICY>()
            //    .Property(e => e.PRICE_MODE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_PRICE_POLICY>()
            //    .Property(e => e.IS_ACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_PRICE_POLICY>()
            //    .Property(e => e.NOTES)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_PRICE_POLICY>()
            //    .Property(e => e.FREIGHT_FREE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_PRICE_POLICY>()
            //    .Property(e => e.FIXED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_PRICEGROUPS>()
            //    .Property(e => e.GROUPNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_PRICEGROUPS>()
            //    .Property(e => e.REPORTCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_PRICES>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_PRICES>()
            //    .Property(e => e.FREIGHT_FREE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_TRANS>()
            //    .Property(e => e.INVNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_TRANS>()
            //    .Property(e => e.REF1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_TRANS>()
            //    .Property(e => e.REF2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_TRANS>()
            //    .Property(e => e.REF3)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_TRANS>()
            //    .Property(e => e.NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_TRANS>()
            //    .Property(e => e.TAXINC)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_TRANS>()
            //    .Property(e => e.ANALYSIS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_TRANS>()
            //    .Property(e => e.ALLOCATED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_TRANS>()
            //    .Property(e => e.GLPOSTED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_TRANS>()
            //    .Property(e => e.DELIVADDR1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_TRANS>()
            //    .Property(e => e.DELIVADDR2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_TRANS>()
            //    .Property(e => e.DELIVADDR3)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_TRANS>()
            //    .Property(e => e.DELIVADDR4)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_TRANS>()
            //    .Property(e => e.ORD_REF)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_TRANS>()
            //    .Property(e => e.DISPATCH_INFO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_TRANS>()
            //    .Property(e => e.DELIVADDR5)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_TRANS>()
            //    .Property(e => e.DELIVADDR6)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_TRANS>()
            //    .Property(e => e.TERMINAL_ID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_TRANS>()
            //    .Property(e => e.TAXRETCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_TRANS>()
            //    .Property(e => e.FREIGHT_FREE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_TRANS>()
            //    .Property(e => e.BANKACCNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_TRANS>()
            //    .Property(e => e.BANKACCNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_TRANS>()
            //    .Property(e => e.EFTCAID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_TRANS>()
            //    .Property(e => e.EFTAUTH)
            //    .IsUnicode(false);

            //modelBuilder.Entity<DR_TRANS>()
            //    .Property(e => e.CUSTORDERNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<dtproperties>()
            //    .Property(e => e.property)
            //    .IsUnicode(false);

            //modelBuilder.Entity<dtproperties>()
            //    .Property(e => e.value)
            //    .IsUnicode(false);

            //modelBuilder.Entity<EFT_AUDIT>()
            //    .Property(e => e.PRINTOK)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<EFT_AUDIT>()
            //    .Property(e => e.GLPOSTED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<EFT_AUDIT>()
            //    .Property(e => e.MANUAL_CHQ)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<EFT_AUDIT>()
            //    .Property(e => e.DETAILS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<EXONET_CACHE_TABLES>()
            //    .Property(e => e.TABLENAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<EXTRA_FIELDS>()
            //    .Property(e => e.TABLE_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<EXTRA_FIELDS>()
            //    .Property(e => e.FIELD_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<EXTRA_FIELDS>()
            //    .Property(e => e.DISPLAY_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<EXTRA_FIELDS>()
            //    .Property(e => e.LOOKUP_TABLE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<EXTRA_FIELDS>()
            //    .Property(e => e.LOOKUP_FIELD)
            //    .IsUnicode(false);

            //modelBuilder.Entity<EXTRA_FIELDS>()
            //    .Property(e => e.DATA_FIELD)
            //    .IsUnicode(false);

            //modelBuilder.Entity<EXTRA_FIELDS>()
            //    .Property(e => e.DATA_TYPE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<EXTRA_FIELDS>()
            //    .Property(e => e.MIXEDCASE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<EXTRA_FIELDS>()
            //    .Property(e => e.EDITABLE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<EXTRA_FIELDS>()
            //    .Property(e => e.TOOLTIP)
            //    .IsUnicode(false);

            //modelBuilder.Entity<EXTRA_FIELDS>()
            //    .Property(e => e.EVENT_FIELD_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<EXTRA_FIELDS>()
            //    .Property(e => e.EVENT_SQL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<FACT_GLOSSARY>()
            //    .Property(e => e.FIELDNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<FACT_GLOSSARY>()
            //    .Property(e => e.NOTES)
            //    .IsUnicode(false);

            //modelBuilder.Entity<FORMULAHDR>()
            //    .Property(e => e.NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<FORMULAHDR>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<FREIGHT_COST_TYPES>()
            //    .Property(e => e.COSTCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<FREIGHT_COST_TYPES>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<FREIGHT_COST_TYPES>()
            //    .Property(e => e.CAN_SPREAD)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<FREIGHTCOST_SPREAD_TYPE>()
            //    .Property(e => e.DISPLAY_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.USERNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.ADDRESS1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.ADDRESS2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.ADDRESS3)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.PHONE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.FAX)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.EMAIL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.TAXREGNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.GL_PASSWORD)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.SUB1_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.SUB1_DEF)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.SUB2_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.SUB2_DEF)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.SUB3_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.SUB3_DEF)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.SUB4_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.SUB4_DEF)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.DELIVADDR1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.DELIVADDR2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.DELIVADDR3)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.DELIVADDR4)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.JOBCODE_ON_TRANS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.STOCKDISCTABLE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.STOCK_WARNING)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.USE_WORKSORDS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.CUSTOMCONFIG_CACHE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.BANK_ACC_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.BANK_ACC_NO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.COUNTRY)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.VALIDATE_ANALYSISCODES)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.SQL_COMPAT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.OPENSYS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.CURR_YEAR_LABEL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.LAST_YEAR_LABEL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.NEXT_YEAR_LABEL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.CHQ_FILE_FORMAT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.RECAL_STOCK_TRANS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.EXOCFG_VERIFY)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.DBUPDATE_GLMOVEMENTS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.MERCHANTPWD)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.ENCRYPTKEY)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.DECRYPTKEY)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.WEBSITE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.SYSADMIN_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.SYSADMIN_PHONE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.SYSADMIN_EMAIL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.PARTNER_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.PARTNER_COMPANYNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.PARTNER_PHONE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.PARTNER_EMAIL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.MPOWERED_PAYEE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.PARENTUSERNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.POSTAL_AS_DELIVARY)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.MPOWERED_SERVICES)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.MPOWERED_SIGNUP_PROMPT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.MSC_DATABASE_ID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.MSC_DATABASE_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.MPOWERED_PAYMENTS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.FACEBOOK_APP_KEY)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.FACEBOOK_APP_SECRET)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.LINKEDIN_APP_KEY)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.LINKEDIN_APP_SECRET)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.TWITTER_APP_KEY)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.TWITTER_APP_SECRET)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.FACEBOOK_ACCESS_TOKEN)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.LINKEDIN_TOKEN_KEY)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.LINKEDIN_TOKEN_SECRET)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.TWITTER_TOKEN_KEY)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.TWITTER_TOKEN_SECRET)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.FACEBOOK_USER_ID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.FACEBOOK_PAGE_ID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.LINKEDIN_USER_ID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.LINKEDIN_COMPANY_ID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.TWITTER_USER_ID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.DIAGNOSTICS_EMAIL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.DETAILS_UPDATE_EMAIL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GENERAL_INFO>()
            //    .Property(e => e.UPDATED_CURRENCY_RATECHANGES)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_ACCGROUPS>()
            //    .Property(e => e.GROUPNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_ACCGROUPS>()
            //    .Property(e => e.DRCR)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_ACCGROUPS>()
            //    .Property(e => e.IS_RECONCILABLE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_ACCGRP_REPCODE>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_CONT_HIST>()
            //    .Property(e => e.NOTE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_CONT_HIST>()
            //    .Property(e => e.SUBJECT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_CONT_HIST>()
            //    .Property(e => e.OUTLOOK_LINK)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_POSTBATCH>()
            //    .Property(e => e.CHQNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_POSTBATCH>()
            //    .Property(e => e.INVNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_POSTBATCH>()
            //    .Property(e => e.DETAILS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTCOLS>()
            //    .Property(e => e.COLNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTCOLS>()
            //    .Property(e => e.COLTYPE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTCOLS>()
            //    .Property(e => e.FIELDNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTCOLS>()
            //    .Property(e => e.FORMATCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTCOLS>()
            //    .Property(e => e.GROUPBYPERIOD)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTCOLS>()
            //    .Property(e => e.SORT_OLDTONEW)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTROWS>()
            //    .Property(e => e.REPORTACTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTROWS>()
            //    .Property(e => e.CAPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTROWS>()
            //    .Property(e => e.DRCR)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTROWS>()
            //    .Property(e => e.SQL_LOGIC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTROWS>()
            //    .Property(e => e.RESETSUBTOT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTROWS>()
            //    .Property(e => e.RESETTOT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTROWS>()
            //    .Property(e => e.RESETGRTOT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTS>()
            //    .Property(e => e.REPORTNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTS>()
            //    .Property(e => e.FONTNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTS>()
            //    .Property(e => e.ORIENTATION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTS>()
            //    .Property(e => e.USERHEADING)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTS>()
            //    .Property(e => e.PERIODBLURB)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTS>()
            //    .Property(e => e.PAGENOLOCATION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTS>()
            //    .Property(e => e.REPEATHEADER)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTS>()
            //    .Property(e => e.PERIODNOINHEADER)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTS>()
            //    .Property(e => e.PERIODYRINHEADER)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTS>()
            //    .Property(e => e.SHORTPERIODS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTS>()
            //    .Property(e => e.SUPPRESSZEROS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTS>()
            //    .Property(e => e.COMMAFORMAT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTS>()
            //    .Property(e => e.SHOWVARIANCEASPC)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTS>()
            //    .Property(e => e.LISTSUBACCOUNTS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTS>()
            //    .Property(e => e.REPORTTABLENAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTS>()
            //    .Property(e => e.USECLARITY)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTS>()
            //    .Property(e => e.RENDERINEXCEL)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTS>()
            //    .Property(e => e.DEFPLROLLINGCYEARONLY)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTS>()
            //    .Property(e => e.REPORTCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_REPORTS>()
            //    .Property(e => e.VALIDATEATRUNTIME)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_SJLINES>()
            //    .Property(e => e.DETAILS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_SJLINES>()
            //    .Property(e => e.CHQNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_SJLINES>()
            //    .Property(e => e.INVNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_SJLINES>()
            //    .Property(e => e.TRANSTYPE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_SJPOSTRUN>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_SJPOSTRUN>()
            //    .Property(e => e.INITIALS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_SJPOSTRUN>()
            //    .Property(e => e.STOCKLEDGER)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_SJPOSTRUN>()
            //    .Property(e => e.DEBTORSLEDGER)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_SJPOSTRUN>()
            //    .Property(e => e.CREDITORSLEDGER)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_SJPOSTRUN>()
            //    .Property(e => e.EXCLUDEDRPMTS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_SJPOSTRUN>()
            //    .Property(e => e.STARTRANGE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_SJPOSTRUN>()
            //    .Property(e => e.ENDRANGE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLACCS>()
            //    .Property(e => e.NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLACCS>()
            //    .Property(e => e.DRCR)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLACCS>()
            //    .Property(e => e.USESUBCODES)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLACCS>()
            //    .Property(e => e.REPORTCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLACCS>()
            //    .Property(e => e.ISACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLACCS>()
            //    .Property(e => e.ALLOWJOURNAL)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLACCS>()
            //    .Property(e => e.NOTES)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLACCS>()
            //    .Property(e => e.PRIVATE_ACC)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLBATCH>()
            //    .Property(e => e.SOURCE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLBATCH>()
            //    .Property(e => e.NARRATIVE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLBATCH>()
            //    .Property(e => e.INITIALS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLBATCH>()
            //    .Property(e => e.AUTO_REVERSE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLBATCH>()
            //    .Property(e => e.PRINTED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLBUDGETS>()
            //    .Property(e => e.BUDGETCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLBUDGETS_HDR>()
            //    .Property(e => e.BUDGETCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLCHART_BUSINESS>()
            //    .Property(e => e.TYPENAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLCHART_BUSINESS>()
            //    .Property(e => e.TEMPLATETEXT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLCHART_INDUSTRY>()
            //    .Property(e => e.NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORT_BATCH>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORT_BATCH_LINE>()
            //    .Property(e => e.BUDGETCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORT_BATCH_LINE>()
            //    .Property(e => e.BRANCHNOS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORT_BATCH_LINE>()
            //    .Property(e => e.SUPPRESSZEROS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORT_BATCH_LINE>()
            //    .Property(e => e.LISTSUBACCOUNTS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORT_BATCH_LINE>()
            //    .Property(e => e.USECLARITY)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORT_BATCH_LINE>()
            //    .Property(e => e.SAVETOFILE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORT_BATCH_LINE>()
            //    .Property(e => e.PRINTERNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLSUBACCS>()
            //    .Property(e => e.NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLSUBACCS>()
            //    .Property(e => e.REPORTCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLSUBACCS>()
            //    .Property(e => e.ALLOWJOURNAL)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLTRANS>()
            //    .Property(e => e.INITIALS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLTRANS>()
            //    .Property(e => e.CHQNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLTRANS>()
            //    .Property(e => e.INVNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLTRANS>()
            //    .Property(e => e.DETAILS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLTRANS>()
            //    .Property(e => e.SOURCE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLTRANS>()
            //    .Property(e => e.AUTO_REVERSE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLTRANS>()
            //    .Property(e => e.TAXRETCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLTRANS>()
            //    .Property(e => e.TRANSTYPE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLTRANS_ARCHIVE>()
            //    .Property(e => e.INITIALS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLTRANS_ARCHIVE>()
            //    .Property(e => e.CHQNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLTRANS_ARCHIVE>()
            //    .Property(e => e.INVNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLTRANS_ARCHIVE>()
            //    .Property(e => e.DETAILS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLTRANS_ARCHIVE>()
            //    .Property(e => e.SOURCE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLTRANS_ARCHIVE>()
            //    .Property(e => e.AUTO_REVERSE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLTRANS_ARCHIVE>()
            //    .Property(e => e.TAXRETCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GRAPH_SERIES>()
            //    .Property(e => e.NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GRAPH_SERIES>()
            //    .Property(e => e.BUDGETCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<IDENTIFIERS>()
            //    .Property(e => e.TABLENAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<IDENTIFIERS>()
            //    .Property(e => e.FIELDNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<INV_FIELD_NAMES>()
            //    .Property(e => e.FIELDNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<INV_NUMBERS>()
            //    .Property(e => e.USERID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<INWARDS_GOODS>()
            //    .Property(e => e.SUPPLIERNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<INWARDS_GOODS>()
            //    .Property(e => e.SUPPLIERREF)
            //    .IsUnicode(false);

            //modelBuilder.Entity<INWARDS_GOODS>()
            //    .Property(e => e.COMPLETE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<INWARDS_GOODS>()
            //    .Property(e => e.NOTES)
            //    .IsUnicode(false);

            //modelBuilder.Entity<INWARDS_GOODS>()
            //    .Property(e => e.INVOICED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<INWARDS_GOODS>()
            //    .Property(e => e.PACKSLIP)
            //    .IsUnicode(false);

            //modelBuilder.Entity<INWARDS_GOODS_COSTS>()
            //    .Property(e => e.COSTCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<INWARDS_GOODS_COSTS>()
            //    .Property(e => e.DETAILS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<INWARDS_GOODS_COSTS>()
            //    .Property(e => e.COMPLETE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<INWARDS_GOODS_COSTS>()
            //    .Property(e => e.GLPOSTED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<INWARDS_GOODS_COSTS>()
            //    .Property(e => e.CAN_SPREAD)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<INWARDS_GOODS_LINES>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<INWARDS_GOODS_LINES>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<INWARDS_GOODS_LINES>()
            //    .Property(e => e.BATCHCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<INWARDS_GOODS_LINES>()
            //    .Property(e => e.SUPPLIERCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<INWARDS_GOODS_LINES>()
            //    .Property(e => e.KITCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<INWARDS_GOODS_LINES>()
            //    .Property(e => e.LINKED_STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<INWARDS_GOODS_LINES>()
            //    .Property(e => e.BOMTYPE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<INWARDS_GOODS_LINES>()
            //    .Property(e => e.SHOWLINE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<INWARDS_GOODS_LINES>()
            //    .Property(e => e.LINKEDSTATUS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<INWARDS_GOODS_LINES>()
            //    .Property(e => e.BOMPRICING)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<INWARDS_GOODS_LINES>()
            //    .Property(e => e.COMPLETE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<INWARDS_GOODS_LINES>()
            //    .Property(e => e.INV_COMPLETE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<INWARDS_GOODS_LINES>()
            //    .Property(e => e.VAR_GLPOSTED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_CATEGORIES>()
            //    .Property(e => e.CATDESC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_CATEGORIES>()
            //    .Property(e => e.DISP_COLOUR)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_CATEGORIES>()
            //    .Property(e => e.SHORTCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_CONTRACT_BILLINGS>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_CONTRACT_BILLINGS>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_CONTRACT_BILLINGS>()
            //    .Property(e => e.PROFORMA_NO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_CONTRACT_BILLINGS>()
            //    .Property(e => e.NOTES)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_CONTRACT_BILLINGS>()
            //    .Property(e => e.INVNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_CONTRACT_BILLINGS>()
            //    .Property(e => e.RETENTION_INVNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_CONTRACT_BILLINGS>()
            //    .Property(e => e.ALLOW_ALLOCATION)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_COSTGROUPS>()
            //    .Property(e => e.COSTDESC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_COSTGROUPS>()
            //    .Property(e => e.SHORTCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_COSTGROUPS>()
            //    .Property(e => e.SHOWONQUOTE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_COSTGROUPS>()
            //    .Property(e => e.CONSOLIDATE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_COSTGROUPS>()
            //    .Property(e => e.COPY_FROM_QUOTE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_COSTTYPES>()
            //    .Property(e => e.COSTDESC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_COSTTYPES>()
            //    .Property(e => e.SHOWONQUOTE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_COSTTYPES>()
            //    .Property(e => e.SHORTCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_COSTTYPES>()
            //    .Property(e => e.CONSOLIDATE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_COSTTYPES>()
            //    .Property(e => e.COPY_FROM_QUOTE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_HIST>()
            //    .Property(e => e.SUBJECT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_HIST>()
            //    .Property(e => e.NOTE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_HIST>()
            //    .Property(e => e.OUTLOOK_LINK)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_HIST>()
            //    .Property(e => e.X_INVNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_OTHER_REPORTS>()
            //    .Property(e => e.REPORTDESC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_OTHER_REPORTS>()
            //    .Property(e => e.REPORT_PARAMS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_OUTPUT_ITEMS>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_OUTPUT_ITEMS>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_QUOTE_OPTIONS>()
            //    .Property(e => e.OPTION_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_QUOTE_OPTIONS>()
            //    .Property(e => e.OPTION_SELECTED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_RESOURCE_ALLOCATION>()
            //    .Property(e => e.SUBJECT_NOTES)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_RESOURCE_ALLOCATION>()
            //    .Property(e => e.APPOINTMENT_SCHEDULED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_STATUS>()
            //    .Property(e => e.STATUSKEY)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_STATUS>()
            //    .Property(e => e.STATUSDESC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_STATUS>()
            //    .Property(e => e.ADMIN_STAT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_STATUS>()
            //    .Property(e => e.LOCK_JOB)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_STATUS>()
            //    .Property(e => e.ISARCHIVED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_STATUS>()
            //    .Property(e => e.ISCOMPLETE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_STATUS>()
            //    .Property(e => e.ISACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_STATUS>()
            //    .Property(e => e.ISINVOICEREADY)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_STATUS>()
            //    .Property(e => e.ISLOCKQUOTE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_STATUS>()
            //    .Property(e => e.WORKFLOW_CONSTRAINED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_STATUS_CONSTRAINT>()
            //    .Property(e => e.ISACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_STATUS_CONSTRAINT>()
            //    .Property(e => e.FROM_STATUS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_STATUS_CONSTRAINT>()
            //    .Property(e => e.TO_STATUS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_STATUS_CONSTRAINT>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_STATUS_CONSTRAINT>()
            //    .Property(e => e.SHORTDESC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_STATUS_CONSTRAINT>()
            //    .Property(e => e.TRACKEVENT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TIMESHEET_ALLOWANCE>()
            //    .Property(e => e.ALLOWANCE_ID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TIMESHEET_ALLOWANCE>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TIMESHEET_ALLOWANCE_TYPES>()
            //    .Property(e => e.ALLOWANCE_ID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TIMESHEET_ALLOWANCE_TYPES>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TIMESHEET_ALLOWANCE_TYPES>()
            //    .Property(e => e.UNIT_OF_MEASURE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TIMESHEET_ALLOWANCE_TYPES>()
            //    .Property(e => e.PAYROLL_ALLOWANCE_ID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TIMESHEETS>()
            //    .Property(e => e.TITLE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TIMESHEETS>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TIMESHEETS>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TIMESHEETS>()
            //    .Property(e => e.DAY1_POSTED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TIMESHEETS>()
            //    .Property(e => e.DAY2_POSTED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TIMESHEETS>()
            //    .Property(e => e.DAY3_POSTED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TIMESHEETS>()
            //    .Property(e => e.DAY4_POSTED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TIMESHEETS>()
            //    .Property(e => e.DAY5_POSTED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TIMESHEETS>()
            //    .Property(e => e.DAY6_POSTED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TIMESHEETS>()
            //    .Property(e => e.DAY7_POSTED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TIMESHEETS>()
            //    .Property(e => e.IS_OVERTIME)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TIMESHEETS>()
            //    .Property(e => e.HAS_ALLOWANCE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TIMESHEETS>()
            //    .Property(e => e.SOURCE_REF)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TIMESHEETS>()
            //    .Property(e => e.X_DECLINE_REASON)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TIMESHEETS_RATES>()
            //    .Property(e => e.SHORTCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TIMESHEETS_RATES>()
            //    .Property(e => e.RATENAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TIMESHEETS_RATES>()
            //    .Property(e => e.PAYROLL_ALLOWANCE_ID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TRANSACTIONS>()
            //    .Property(e => e.TRANSTYPE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TRANSACTIONS>()
            //    .Property(e => e.LINE_STATUS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TRANSACTIONS>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TRANSACTIONS>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TRANSACTIONS>()
            //    .Property(e => e.LINE_SOURCE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TRANSACTIONS>()
            //    .Property(e => e.NARRATIVE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TRANSACTIONS>()
            //    .Property(e => e.STARTTIME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TRANSACTIONS>()
            //    .Property(e => e.ENDTIME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TRANSACTIONS>()
            //    .Property(e => e.BILLING_REF)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TRANSACTIONS>()
            //    .Property(e => e.PRICE_OVERRIDDEN)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TRANSACTIONS>()
            //    .Property(e => e.KITCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TRANSACTIONS>()
            //    .Property(e => e.LINKED_STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TRANSACTIONS>()
            //    .Property(e => e.BOMTYPE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TRANSACTIONS>()
            //    .Property(e => e.BOMPRICING)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TRANSACTIONS>()
            //    .Property(e => e.SHOWLINE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TRANSACTIONS>()
            //    .Property(e => e.LINKEDSTATUS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TRANSACTIONS>()
            //    .Property(e => e.SOURCE_REF)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TRANSACTIONS>()
            //    .Property(e => e.BATCHCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TRANSACTIONS>()
            //    .Property(e => e.SPREADVALUE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TRANSACTIONS>()
            //    .Property(e => e.PROGRESSINVOICE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TRANSACTIONS>()
            //    .Property(e => e.PAYROLL_STATUS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TRANSACTIONS>()
            //    .Property(e => e.ISSUPPLIED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TRANSACTIONS>()
            //    .Property(e => e.X_WAGE_TYPE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TYPES>()
            //    .Property(e => e.TYPEDESC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOB_TYPES>()
            //    .Property(e => e.SHORTCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_FLAGS>()
            //    .Property(e => e.INVOICEREADY)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_FLAGS>()
            //    .Property(e => e.ISACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_FLAGS>()
            //    .Property(e => e.ISCOMPLETE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_FLAGS>()
            //    .Property(e => e.ISARCHIVED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_FLAGS>()
            //    .Property(e => e.FLAG01)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_FLAGS>()
            //    .Property(e => e.FLAG02)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_FLAGS>()
            //    .Property(e => e.FLAG03)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_FLAGS>()
            //    .Property(e => e.FLAG04)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_FLAGS>()
            //    .Property(e => e.FLAG05)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_FLAGS>()
            //    .Property(e => e.FLAG06)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_FLAGS>()
            //    .Property(e => e.FLAG07)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_FLAGS>()
            //    .Property(e => e.FLAG08)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_FLAGS>()
            //    .Property(e => e.FLAG09)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_FLAGS>()
            //    .Property(e => e.FLAG10)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_FLAGS>()
            //    .Property(e => e.FLAG11)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_FLAGS>()
            //    .Property(e => e.FLAG12)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_FLAGS>()
            //    .Property(e => e.FLAG13)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_FLAGS>()
            //    .Property(e => e.FLAG14)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_FLAGS>()
            //    .Property(e => e.FLAG15)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_FLAGSDESC>()
            //    .Property(e => e.FLAGCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_FLAGSDESC>()
            //    .Property(e => e.FLAGDESC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_GENERAL_INFO>()
            //    .Property(e => e.QUOTE_TERMS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_GENERAL_INFO>()
            //    .Property(e => e.SUBSADDUP)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_GENERAL_INFO>()
            //    .Property(e => e.JOBSTAT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_GENERAL_INFO>()
            //    .Property(e => e.MOVE_STOCK)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_GENERAL_INFO>()
            //    .Property(e => e.COSTGLSQL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_GENERAL_INFO>()
            //    .Property(e => e.SALESGLSQL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_GENERAL_INFO>()
            //    .Property(e => e.OURREF)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_GENERAL_INFO>()
            //    .Property(e => e.PRINTDESC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_GENERAL_INFO>()
            //    .Property(e => e.OPTION_1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_GENERAL_INFO>()
            //    .Property(e => e.OPTION_2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_GENERAL_INFO>()
            //    .Property(e => e.OPTION_3)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_GENERAL_INFO>()
            //    .Property(e => e.OPTION_4)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_GENERAL_INFO>()
            //    .Property(e => e.OPTION_5)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_GENERAL_INFO>()
            //    .Property(e => e.OPTION_6)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_GENERAL_INFO>()
            //    .Property(e => e.OPTION_7)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_GENERAL_INFO>()
            //    .Property(e => e.OPTION_8)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_GENERAL_INFO>()
            //    .Property(e => e.OPTION_9)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_GENERAL_INFO>()
            //    .Property(e => e.OPTION_10)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_GENERAL_INFO>()
            //    .Property(e => e.JOBNO_PROMPT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_GENERAL_INFO>()
            //    .Property(e => e.SCHEDULER)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_GENERAL_INFO>()
            //    .Property(e => e.TIMESELECTSQL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_GENERAL_INFO>()
            //    .Property(e => e.COSTSELECTSQL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_GENERAL_INFO>()
            //    .Property(e => e.SHOWMAIN)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_GENERAL_INFO>()
            //    .Property(e => e.WIPNOTSOH)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_GENERAL_INFO>()
            //    .Property(e => e.DEFNONSTOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_GENERAL_INFO>()
            //    .Property(e => e.USE_SALES_ORD_NO)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_HDR>()
            //    .Property(e => e.JOBCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_HDR>()
            //    .Property(e => e.CUSTORDNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_HDR>()
            //    .Property(e => e.STATUS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_HDR>()
            //    .Property(e => e.TITLE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_HDR>()
            //    .Property(e => e.SERIALNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_HDR>()
            //    .Property(e => e.CONTACT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_HDR>()
            //    .Property(e => e.PRIVATE_NOTE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_HDR>()
            //    .Property(e => e.DELADDR1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_HDR>()
            //    .Property(e => e.DELADDR2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_HDR>()
            //    .Property(e => e.DELADDR3)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_HDR>()
            //    .Property(e => e.DELADDR4)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_HDR>()
            //    .Property(e => e.DELADDR5)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_HDR>()
            //    .Property(e => e.DELADDR6)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_HDR>()
            //    .Property(e => e.ISACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_HDR>()
            //    .Property(e => e.HASUNBILLED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_HDR>()
            //    .Property(e => e.INVOICEREADY)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_HDR>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_LINES>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_LINES>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_LINES>()
            //    .Property(e => e.SHOW_ON_INVOICE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_LINES>()
            //    .Property(e => e.LINE_STATUS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_LINES>()
            //    .Property(e => e.LINE_SOURCE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_LINES>()
            //    .Property(e => e.NARRATIVE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_LINES>()
            //    .Property(e => e.COPY_FROM_QUOTE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_LINES>()
            //    .Property(e => e.PRICE_OVERRIDDEN)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_LINES>()
            //    .Property(e => e.KITCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_LINES>()
            //    .Property(e => e.LINKED_STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_LINES>()
            //    .Property(e => e.BOMTYPE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_LINES>()
            //    .Property(e => e.SHOWLINE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_LINES>()
            //    .Property(e => e.BOMPRICING)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_LINES>()
            //    .Property(e => e.LINKEDSTATUS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_LINES>()
            //    .Property(e => e.BATCHCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_LINES>()
            //    .Property(e => e.SPREADVALUE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_LINES>()
            //    .Property(e => e.PREF_SERIALNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_PROJ>()
            //    .Property(e => e.PROJ_CODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_PROJ>()
            //    .Property(e => e.PROJ_TITLE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_PROJ>()
            //    .Property(e => e.ISACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_PROJ>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_RESOURCE>()
            //    .Property(e => e.RESOURCENAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_RESOURCE>()
            //    .Property(e => e.TITLE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_RESOURCE>()
            //    .Property(e => e.ISACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_RESOURCE>()
            //    .Property(e => e.DEFAULT_STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_RESOURCE>()
            //    .Property(e => e.SHORTCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_RESOURCE>()
            //    .Property(e => e.EMAIL_ADDRESS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<JOBCOST_RESOURCE>()
            //    .Property(e => e.FILTERSQL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<KIT_SERIAL_INFO>()
            //    .Property(e => e.KIT_SERIAL_NO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<KIT_SERIAL_INFO>()
            //    .Property(e => e.REFERENCE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<LEDGER_PERIODS>()
            //    .Property(e => e.LEDGER)
            //    .IsUnicode(false);

            //modelBuilder.Entity<LEGENDS>()
            //    .Property(e => e.TABLENAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<LEGENDS>()
            //    .Property(e => e.FIELDNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<LEGENDS>()
            //    .Property(e => e.FIELDVALUE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<LEGENDS>()
            //    .Property(e => e.FIELDLABEL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<LEGENDS>()
            //    .Property(e => e.FIELDNOTES)
            //    .IsUnicode(false);

            //modelBuilder.Entity<MANIFESTS>()
            //    .Property(e => e.CARRIERCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<MANIFESTS>()
            //    .Property(e => e.DETAILS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<MANREP>()
            //    .Property(e => e.PERIOD_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<MANREP>()
            //    .Property(e => e.SCHEDULED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<MANREP_BRANCH>()
            //    .Property(e => e.SCHEDULED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<MANREP_CLOSING_STOCK>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<MANREP_CLOSING_STOCK>()
            //    .Property(e => e.SCHEDULED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<MANREP_DAYPLAN>()
            //    .Property(e => e.WORKDAY)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<MANREP_PERIOD>()
            //    .Property(e => e.PERIOD_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<MANREP_PERIOD>()
            //    .Property(e => e.CALENDAR)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<MANREP_PERIOD>()
            //    .HasMany(e => e.CRM_BUDGET)
            //    .WithRequired(e => e.MANREP_PERIOD)
            //    .HasForeignKey(e => e.MANREP_PERIOD_SEQNO)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<MANREP_PERIOD>()
            //    .HasMany(e => e.CRM_BUDGET_HDR)
            //    .WithRequired(e => e.MANREP_PERIOD)
            //    .HasForeignKey(e => e.MANREP_PERIOD_SEQNO)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<MANREP_SALESPERSON>()
            //    .Property(e => e.SCHEDULED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<MANREP_SETUP>()
            //    .Property(e => e.PERIOD_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<MANREP_SETUP>()
            //    .Property(e => e.EXCLUDE_QUOTES)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<MANREP_SETUP>()
            //    .Property(e => e.USE_ACTUALDATE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<MANREP_STOCK>()
            //    .Property(e => e.SCHEDULED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<MENU_ASSIGNMENTS>()
            //    .Property(e => e.USERNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<MENU_COLLECTION>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<MENU_COLLECTION>()
            //    .Property(e => e.ISENABLED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<MENU_HDR>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<MENU_HDR>()
            //    .Property(e => e.ISENABLED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<MENU_LINES>()
            //    .Property(e => e.CAPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<MENU_LINES>()
            //    .Property(e => e.PROCPARAMS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<MENU_LINES>()
            //    .Property(e => e.LINECLASSCOORDINATES)
            //    .IsUnicode(false);

            //modelBuilder.Entity<MENU_LINES>()
            //    .Property(e => e.LINECLASSPROPERTIES)
            //    .IsUnicode(false);

            //modelBuilder.Entity<MENU_MASTER>()
            //    .Property(e => e.MENU_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<MENU_OPTIONS>()
            //    .Property(e => e.CAPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<MENU_OPTIONS>()
            //    .Property(e => e.PROC_DATA)
            //    .IsUnicode(false);

            //modelBuilder.Entity<MENU_PROCEDURES>()
            //    .Property(e => e.PROCNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<MENU_PROCEDURES>()
            //    .Property(e => e.MENU_NOTES)
            //    .IsUnicode(false);

            //modelBuilder.Entity<MENU_PROCEDURES>()
            //    .Property(e => e.DEFAULT_HELP_TEXT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<MODULE_SECURITY>()
            //    .Property(e => e.ACCESSCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<MODULE_SECURITY>()
            //    .Property(e => e.MODULE_INFO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<MONTHSTABLE>()
            //    .Property(e => e.MONTHNAMESHORT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<MONTHSTABLE>()
            //    .Property(e => e.MONTHNAMELONG)
            //    .IsUnicode(false);

            //modelBuilder.Entity<NARRATIVES>()
            //    .Property(e => e.NARRATIVE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<OBJECT_DATA>()
            //    .Property(e => e.ID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<OBJECT_DATA>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<OBJECT_DATA>()
            //    .Property(e => e.MIMECODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<OBJECT_DATA>()
            //    .Property(e => e.FILENAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<OBJECT_LOCK>()
            //    .Property(e => e.OBJECT_ID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<OBJECT_LOCK>()
            //    .Property(e => e.COMPUTERID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY>()
            //    .Property(e => e.COMPANYID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY>()
            //    .Property(e => e.DETAILS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY>()
            //    .Property(e => e.IS_CLOSE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY>()
            //    .Property(e => e.X_DOCSLINK)
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_HIST>()
            //    .Property(e => e.SUBJECT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_HIST>()
            //    .Property(e => e.NOTE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_HIST>()
            //    .Property(e => e.OUTLOOK_LINK)
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_LEAD>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_LINES>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_LINES>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_QUOTE>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_QUOTE>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_QUOTE>()
            //    .Property(e => e.SHOW_ON_INVOICE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_QUOTE>()
            //    .Property(e => e.LINE_STATUS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_QUOTE>()
            //    .Property(e => e.NARRATIVE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_QUOTE>()
            //    .Property(e => e.PRICE_OVERRIDDEN)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_QUOTE>()
            //    .Property(e => e.BOMTYPE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_QUOTE>()
            //    .Property(e => e.BOMPRICING)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_QUOTE>()
            //    .Property(e => e.SHOWLINE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_QUOTE>()
            //    .Property(e => e.LINKEDSTATUS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_QUOTE>()
            //    .Property(e => e.KITCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_QUOTE>()
            //    .Property(e => e.LINKED_STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_QUOTE>()
            //    .Property(e => e.SPREADVALUE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_QUOTE>()
            //    .Property(e => e.LINETAX_OVERRIDDEN)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_QUOTE_OPTIONS>()
            //    .Property(e => e.OPTION_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_QUOTE_OPTIONS>()
            //    .Property(e => e.OPTION_SELECTED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_STAGE>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_STAGE>()
            //    .Property(e => e.STATUSKEY)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_STAGE>()
            //    .Property(e => e.ADMIN_STAT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_STAGE>()
            //    .Property(e => e.LOCK_JOB)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_STAGE>()
            //    .Property(e => e.ISARCHIVED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_STAGE>()
            //    .Property(e => e.ISCOMPLETE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_STAGE>()
            //    .Property(e => e.ISACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_STAGE>()
            //    .Property(e => e.ISINVOICEREADY)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_STAGE>()
            //    .Property(e => e.ISLOCKQUOTE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_STAGE>()
            //    .Property(e => e.WORKFLOW_CONSTRAINED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_STAGE_CONSTRAINT>()
            //    .Property(e => e.ISACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_STAGE_CONSTRAINT>()
            //    .Property(e => e.FROM_STATUS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_STAGE_CONSTRAINT>()
            //    .Property(e => e.TO_STATUS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_STAGE_CONSTRAINT>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_STAGE_CONSTRAINT>()
            //    .Property(e => e.SHORTDESC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_STAGE_CONSTRAINT>()
            //    .Property(e => e.TRACKEVENT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<OPPORTUNITY_TYPE>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_DENOMINATIONS>()
            //    .Property(e => e.PDNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_GROUP>()
            //    .Property(e => e.PGDESC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_GROUP>()
            //    .Property(e => e.INFLOAT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_GROUP>()
            //    .Property(e => e.BANKABLE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_GROUP>()
            //    .Property(e => e.ISACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_GROUP>()
            //    .Property(e => e.CALC_BANKFEE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_TYPES>()
            //    .Property(e => e.PTDESC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_TYPES>()
            //    .Property(e => e.PTKEY)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_TYPES>()
            //    .Property(e => e.IMG_FILE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_TYPES>()
            //    .Property(e => e.WEB_SHOW)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_TYPES>()
            //    .Property(e => e.LIVE_TRANS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_TYPES>()
            //    .Property(e => e.SHORTNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_TYPES>()
            //    .Property(e => e.REFUND)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_TYPES>()
            //    .Property(e => e.OVER_TEND)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_TYPES>()
            //    .Property(e => e.ACC_MASK)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_TYPES>()
            //    .Property(e => e.ROUND_UP)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_TYPES>()
            //    .Property(e => e.FEE_STOCKITEM)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_TYPES>()
            //    .Property(e => e.ACTIVE_DR)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_TYPES>()
            //    .Property(e => e.ACTIVE_CR)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_TYPES>()
            //    .Property(e => e.ACTIVE_POS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_TYPES>()
            //    .Property(e => e.CHEQUE_TYPE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_TYPES>()
            //    .Property(e => e.DIRECT_DEBIT_TYPE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_TYPES>()
            //    .Property(e => e.DIRECT_CREDIT_TYPE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_TYPES>()
            //    .Property(e => e.BANKFEE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_TYPES>()
            //    .Property(e => e.ZERO_TEND)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_TYPES>()
            //    .Property(e => e.POS_SALE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_TYPES>()
            //    .Property(e => e.POS_RECEIPT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_TYPES>()
            //    .Property(e => e.POS_CREDIT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_TYPES>()
            //    .Property(e => e.POS_REFUND)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_TYPES>()
            //    .Property(e => e.POS_QUOTE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_TYPES>()
            //    .Property(e => e.POS_LAYBY)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_TYPES>()
            //    .Property(e => e.INCASHDRAWER)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYMENT_TYPES>()
            //    .Property(e => e.MPOWERED_TYPE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYRUN_LINES>()
            //    .Property(e => e.REFERENCE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYRUN_LINES>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PERIOD_STATUS>()
            //    .Property(e => e.LEDGER)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PERIOD_STATUS>()
            //    .Property(e => e.LOCKED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PERIOD_STATUS>()
            //    .Property(e => e.PERIODNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PERIOD_STATUS>()
            //    .Property(e => e.PERIOD_SHORTNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PERIOD_STATUS>()
            //    .Property(e => e.REPORTCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PERIOD_STATUS>()
            //    .HasMany(e => e.CRM_BUDGET)
            //    .WithOptional(e => e.PERIOD_STATUS)
            //    .HasForeignKey(e => e.ACTUAL_PERIOD_STATUS_SEQNO);

            //modelBuilder.Entity<PERIOD_STATUS>()
            //    .HasMany(e => e.CRM_BUDGET1)
            //    .WithOptional(e => e.PERIOD_STATUS1)
            //    .HasForeignKey(e => e.LYACTUAL_PERIOD_STATUS_SEQNO);

            //modelBuilder.Entity<PERIODS_DEFN>()
            //    .Property(e => e.PERIODNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PERIODS_DEFN>()
            //    .Property(e => e.CALMONTHS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PERIODS_DEFN>()
            //    .Property(e => e.PERIOD_SHORTNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PERIODS_DEFN>()
            //    .Property(e => e.REPORTCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PERIODS_DEFN_NEXTFINYR>()
            //    .Property(e => e.PERIODNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PERIODS_DEFN_NEXTFINYR>()
            //    .Property(e => e.CALMONTHS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PERIODS_DEFN_NEXTFINYR>()
            //    .Property(e => e.PERIOD_SHORTNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PERIODS_DEFN_NEXTFINYR>()
            //    .Property(e => e.REPORTCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<POS_COUNT>()
            //    .Property(e => e.PTDESC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<POS_SETTLEMENT>()
            //    .Property(e => e.CAID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<POS_SETTLEMENT>()
            //    .Property(e => e.TERMINALID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<POS_SHIFTS>()
            //    .Property(e => e.NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<POS_SHIFTS>()
            //    .Property(e => e.TERMINAL_ID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PRICE_NAMES>()
            //    .Property(e => e.NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PRICE_NAMES>()
            //    .Property(e => e.PRICE_CALC_SQL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PRICE_NAMES>()
            //    .Property(e => e.SYSTEM_PRICETAX_EXCEPTION)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PRICE_SCHEDULE>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PRICE_SCHEDULE>()
            //    .Property(e => e.NEWPRICESQL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PRINT_LOG>()
            //    .Property(e => e.REFERENCE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PRINT_LOG>()
            //    .Property(e => e.SENT_TO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE>()
            //    .Property(e => e.PROFILENAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE>()
            //    .Property(e => e.PATHREF)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_CONFLICT_SET>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_CONFLICT_SET>()
            //    .Property(e => e.NOTES)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_CONFLICT_SET_MEMBERS>()
            //    .Property(e => e.FIELDNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_CONFLICT_SET_MEMBERS>()
            //    .Property(e => e.FIELDVALUE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_CONSTRAINT_VALUES>()
            //    .Property(e => e.VALUETYPE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_CONSTRAINT_VALUES>()
            //    .Property(e => e.CONSTRAINTVALUE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_CONSTRAINT_VALUES>()
            //    .Property(e => e.DISPLAYTEXT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_CONSTRAINTS>()
            //    .Property(e => e.CONSTRAINTTYPE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_CONSTRAINTS>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_CONSTRAINTS>()
            //    .Property(e => e.CONSTRAINTSQL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_FIELDS>()
            //    .Property(e => e.FIELDNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_FIELDS>()
            //    .Property(e => e.DEFAULTVALUE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_FIELDS>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_FIELDS>()
            //    .Property(e => e.PROGNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_FIELDS>()
            //    .Property(e => e.MODULENAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_FIELDS>()
            //    .Property(e => e.FUNCTIONNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_FIELDS>()
            //    .Property(e => e.NOTES)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_FIELDS>()
            //    .Property(e => e.KEYWORDS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_FIELDS>()
            //    .Property(e => e.FIELDPROPERTIES)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_FORMS>()
            //    .Property(e => e.LAYOUTNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_FORMS>()
            //    .Property(e => e.LASTUSEDLAYOUT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_FORMS>()
            //    .Property(e => e.MODULENAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_FORMS>()
            //    .Property(e => e.WIDGETDATA)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_FORMS>()
            //    .Property(e => e.LAYOUTDATA)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_FORMS>()
            //    .Property(e => e.ZOOMSETTINGS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_FORMS>()
            //    .Property(e => e.WIDGETSETTINGS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_GRIDS>()
            //    .Property(e => e.REGKEY)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_TYPE_INFO>()
            //    .Property(e => e.TYPENAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_TYPE_INFO>()
            //    .Property(e => e.DATATYPE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_TYPE_INFO>()
            //    .Property(e => e.TYPESQL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_TYPES>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_VALUES>()
            //    .Property(e => e.FIELDNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROFILE_VALUES>()
            //    .Property(e => e.FIELDVALUE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROSPECT_CONTACTS>()
            //    .Property(e => e.DEFCONTACT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROSPECT_CONTACTS>()
            //    .Property(e => e.DEFACCOUNT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROSPECT_HIST>()
            //    .Property(e => e.SUBJECT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROSPECT_HIST>()
            //    .Property(e => e.NOTE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROSPECT_HIST>()
            //    .Property(e => e.OUTLOOK_LINK)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROSPECTS>()
            //    .Property(e => e.NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROSPECTS>()
            //    .Property(e => e.POST_CODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROSPECTS>()
            //    .Property(e => e.PHONE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROSPECTS>()
            //    .Property(e => e.FAX)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROSPECTS>()
            //    .Property(e => e.EMAIL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROSPECTS>()
            //    .Property(e => e.WEBSITE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROSPECTS>()
            //    .Property(e => e.ADDRESS1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROSPECTS>()
            //    .Property(e => e.ADDRESS2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROSPECTS>()
            //    .Property(e => e.ADDRESS3)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROSPECTS>()
            //    .Property(e => e.ADDRESS4)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROSPECTS>()
            //    .Property(e => e.DELADDR1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROSPECTS>()
            //    .Property(e => e.DELADDR2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROSPECTS>()
            //    .Property(e => e.DELADDR3)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROSPECTS>()
            //    .Property(e => e.DELADDR4)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROSPECTS>()
            //    .Property(e => e.DELADDR5)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROSPECTS>()
            //    .Property(e => e.DELADDR6)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROSPECTS>()
            //    .Property(e => e.NOTES)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROSPECTS>()
            //    .Property(e => e.ADDRESS5)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROSPECTS>()
            //    .Property(e => e.ALPHACODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROSPECTS>()
            //    .Property(e => e.LINKEDIN)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROSPECTS>()
            //    .Property(e => e.TWITTER)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PROSPECTS>()
            //    .Property(e => e.FACEBOOK)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_HDR>()
            //    .Property(e => e.REFERENCE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_HDR>()
            //    .Property(e => e.ADDRESS1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_HDR>()
            //    .Property(e => e.ADDRESS2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_HDR>()
            //    .Property(e => e.ADDRESS3)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_HDR>()
            //    .Property(e => e.ADDRESS4)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_HDR>()
            //    .Property(e => e.INSTRUCTIONS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_HDR>()
            //    .Property(e => e.TAXINC)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_HDR>()
            //    .Property(e => e.ISCONFIRMED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_HDR>()
            //    .Property(e => e.ADDRESS5)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_HDR>()
            //    .Property(e => e.ADDRESS6)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_HDR>()
            //    .Property(e => e.SO_SOURCE_REF)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_HDR>()
            //    .Property(e => e.X_CONTACT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_HDR_ARCHIVE>()
            //    .Property(e => e.REFERENCE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_HDR_ARCHIVE>()
            //    .Property(e => e.ADDRESS1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_HDR_ARCHIVE>()
            //    .Property(e => e.ADDRESS2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_HDR_ARCHIVE>()
            //    .Property(e => e.ADDRESS3)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_HDR_ARCHIVE>()
            //    .Property(e => e.ADDRESS4)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_HDR_ARCHIVE>()
            //    .Property(e => e.INSTRUCTIONS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_HDR_ARCHIVE>()
            //    .Property(e => e.TAXINC)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_HDR_ARCHIVE>()
            //    .Property(e => e.ISCONFIRMED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_HDR_ARCHIVE>()
            //    .Property(e => e.ADDRESS5)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_HDR_ARCHIVE>()
            //    .Property(e => e.ADDRESS6)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_HDR_ARCHIVE>()
            //    .Property(e => e.SO_SOURCE_REF)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_HDR_ARCHIVE>()
            //    .Property(e => e.X_CONTACT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_HIST>()
            //    .Property(e => e.EVENT_TYPE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_HIST>()
            //    .Property(e => e.FILEURL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_LINES>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_LINES>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_LINES>()
            //    .Property(e => e.JOBCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_LINES>()
            //    .Property(e => e.BATCHCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_LINES>()
            //    .Property(e => e.CODETYPE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_LINES>()
            //    .Property(e => e.LINETAX_OVERRIDDEN)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_LINES>()
            //    .Property(e => e.SUPPLIERCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_LINES>()
            //    .Property(e => e.ISCONFIRMED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_LINES>()
            //    .Property(e => e.REFERENCE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_LINES>()
            //    .Property(e => e.KITCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_LINES>()
            //    .Property(e => e.LINKED_STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_LINES>()
            //    .Property(e => e.BOMTYPE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_LINES>()
            //    .Property(e => e.SHOWLINE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_LINES>()
            //    .Property(e => e.LINKEDSTATUS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_LINES>()
            //    .Property(e => e.BOMPRICING)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_LINES>()
            //    .Property(e => e.PRICE_OVERRIDDEN)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_LINES_ARCHIVE>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_LINES_ARCHIVE>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_LINES_ARCHIVE>()
            //    .Property(e => e.JOBCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_LINES_ARCHIVE>()
            //    .Property(e => e.BATCHCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_LINES_ARCHIVE>()
            //    .Property(e => e.CODETYPE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_LINES_ARCHIVE>()
            //    .Property(e => e.LINETAX_OVERRIDDEN)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_LINES_ARCHIVE>()
            //    .Property(e => e.SUPPLIERCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_LINES_ARCHIVE>()
            //    .Property(e => e.ISCONFIRMED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_LINES_ARCHIVE>()
            //    .Property(e => e.REFERENCE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_LINES_ARCHIVE>()
            //    .Property(e => e.KITCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_LINES_ARCHIVE>()
            //    .Property(e => e.LINKED_STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_LINES_ARCHIVE>()
            //    .Property(e => e.BOMTYPE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_LINES_ARCHIVE>()
            //    .Property(e => e.SHOWLINE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_LINES_ARCHIVE>()
            //    .Property(e => e.LINKEDSTATUS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_LINES_ARCHIVE>()
            //    .Property(e => e.BOMPRICING)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PURCHORD_LINES_ARCHIVE>()
            //    .Property(e => e.PRICE_OVERRIDDEN)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<QUICK_INSERT_TABSHEET>()
            //    .Property(e => e.TABSHEETNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<QUICK_INSERT_TABSHEET>()
            //    .Property(e => e.TEMPLATENAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<QUICK_INSERT_TABSHEET>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<QUICK_INSERT_TABSHEET>()
            //    .Property(e => e.ISACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<REASON_CLASS>()
            //    .Property(e => e.CLASSNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<REASON_CLASS>()
            //    .Property(e => e.ACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<REASON_CLASS>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<REASON_CLASS>()
            //    .Property(e => e.COPYTO_HIST)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<REASON_CLASS>()
            //    .Property(e => e.REF1_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<REASON_EVENTS>()
            //    .Property(e => e.FROM_LEDGER)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<REASON_EVENTS>()
            //    .Property(e => e.MEMO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<REASON_EVENTS>()
            //    .Property(e => e.OLD_VAL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<REASON_EVENTS>()
            //    .Property(e => e.NEW_VAL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<REASON_EVENTS>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<REASON_EVENTS>()
            //    .Property(e => e.SERIALNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<REASON_EVENTS>()
            //    .Property(e => e.REF1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<REASONS>()
            //    .Property(e => e.REASONNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<REASONS>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<REASONS>()
            //    .Property(e => e.REPORTCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<RELATIONSHIPS_DEFN>()
            //    .Property(e => e.RELNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<RELATIONSHIPS_DEFN>()
            //    .Property(e => e.RELNAME_INV)
            //    .IsUnicode(false);

            //modelBuilder.Entity<RELATIONSHIPS_SET>()
            //    .Property(e => e.SETNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<RELATIONSHIPS_SET>()
            //    .Property(e => e.SETACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALES_MESSAGES>()
            //    .Property(e => e.MESSAGE1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALES_MESSAGES>()
            //    .Property(e => e.MESSAGE2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALES_MESSAGES>()
            //    .Property(e => e.MESSAGE3)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALES_MESSAGES>()
            //    .Property(e => e.MESSAGE4)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR>()
            //    .Property(e => e.CUSTORDERNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR>()
            //    .Property(e => e.REFERENCE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR>()
            //    .Property(e => e.ADDRESS1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR>()
            //    .Property(e => e.ADDRESS2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR>()
            //    .Property(e => e.ADDRESS3)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR>()
            //    .Property(e => e.ADDRESS4)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR>()
            //    .Property(e => e.INSTRUCTIONS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR>()
            //    .Property(e => e.TAXINC)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR>()
            //    .Property(e => e.BACKORDER)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR>()
            //    .Property(e => e.DISPATCH_INFO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR>()
            //    .Property(e => e.ADDRESS5)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR>()
            //    .Property(e => e.ADDRESS6)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR>()
            //    .Property(e => e.HAS_UNRELEASED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR>()
            //    .Property(e => e.HAS_BACKORDERS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR>()
            //    .Property(e => e.HAS_UNSUPPLIED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR>()
            //    .Property(e => e.HAS_UNINVOICED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR>()
            //    .Property(e => e.HAS_UNPICKED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR>()
            //    .Property(e => e.SHIP_COMPLETE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR>()
            //    .Property(e => e.ONHOLD)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR>()
            //    .Property(e => e.WAS_BACKORDERED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR_ARCHIVE>()
            //    .Property(e => e.CUSTORDERNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR_ARCHIVE>()
            //    .Property(e => e.REFERENCE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR_ARCHIVE>()
            //    .Property(e => e.ADDRESS1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR_ARCHIVE>()
            //    .Property(e => e.ADDRESS2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR_ARCHIVE>()
            //    .Property(e => e.ADDRESS3)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR_ARCHIVE>()
            //    .Property(e => e.ADDRESS4)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR_ARCHIVE>()
            //    .Property(e => e.INSTRUCTIONS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR_ARCHIVE>()
            //    .Property(e => e.TAXINC)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR_ARCHIVE>()
            //    .Property(e => e.BACKORDER)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR_ARCHIVE>()
            //    .Property(e => e.DISPATCH_INFO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR_ARCHIVE>()
            //    .Property(e => e.ADDRESS5)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR_ARCHIVE>()
            //    .Property(e => e.ADDRESS6)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR_ARCHIVE>()
            //    .Property(e => e.HAS_UNRELEASED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR_ARCHIVE>()
            //    .Property(e => e.HAS_BACKORDERS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR_ARCHIVE>()
            //    .Property(e => e.HAS_UNSUPPLIED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR_ARCHIVE>()
            //    .Property(e => e.HAS_UNINVOICED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR_ARCHIVE>()
            //    .Property(e => e.HAS_UNPICKED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR_ARCHIVE>()
            //    .Property(e => e.ONHOLD)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_HDR_ARCHIVE>()
            //    .Property(e => e.SHIP_COMPLETE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_LINES>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_LINES>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_LINES>()
            //    .Property(e => e.JOBCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_LINES>()
            //    .Property(e => e.BATCHCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_LINES>()
            //    .Property(e => e.LINETAX_OVERRIDDEN)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_LINES>()
            //    .Property(e => e.SERIALNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_LINES>()
            //    .Property(e => e.BINCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_LINES>()
            //    .Property(e => e.LINKED_STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_LINES>()
            //    .Property(e => e.PRICE_OVERRIDDEN)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_LINES>()
            //    .Property(e => e.KITCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_LINES>()
            //    .Property(e => e.BOMTYPE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_LINES>()
            //    .Property(e => e.SHOWLINE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_LINES>()
            //    .Property(e => e.LINKEDSTATUS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_LINES>()
            //    .Property(e => e.BOMPRICING)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_LINES>()
            //    .Property(e => e.CUSTORDERNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_LINES_ARCHIVE>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_LINES_ARCHIVE>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_LINES_ARCHIVE>()
            //    .Property(e => e.JOBCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_LINES_ARCHIVE>()
            //    .Property(e => e.BATCHCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_LINES_ARCHIVE>()
            //    .Property(e => e.LINETAX_OVERRIDDEN)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_LINES_ARCHIVE>()
            //    .Property(e => e.SERIALNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_LINES_ARCHIVE>()
            //    .Property(e => e.BINCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_LINES_ARCHIVE>()
            //    .Property(e => e.LINKED_STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_LINES_ARCHIVE>()
            //    .Property(e => e.PRICE_OVERRIDDEN)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_LINES_ARCHIVE>()
            //    .Property(e => e.KITCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_LINES_ARCHIVE>()
            //    .Property(e => e.BOMTYPE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_LINES_ARCHIVE>()
            //    .Property(e => e.SHOWLINE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_LINES_ARCHIVE>()
            //    .Property(e => e.LINKEDSTATUS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_LINES_ARCHIVE>()
            //    .Property(e => e.BOMPRICING)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORD_STATUS>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORDHIST>()
            //    .Property(e => e.EVENT_TYPE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SALESORDHIST>()
            //    .Property(e => e.FILEURL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SHIPMENT_COST_DETAILS>()
            //    .Property(e => e.COSTCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SHIPMENT_COST_DETAILS>()
            //    .Property(e => e.DETAILS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SHIPMENT_HDR>()
            //    .Property(e => e.INT_SHIPREF)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SHIPMENT_HDR>()
            //    .Property(e => e.EXT_SHIPREF)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SHIPMENT_HDR>()
            //    .Property(e => e.WEIGHT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SHIPMENT_HDR>()
            //    .Property(e => e.VESSEL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SHIPMENT_HDR>()
            //    .Property(e => e.CARRIER)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SHIPMENT_HDR>()
            //    .Property(e => e.NOTES)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SHIPMENT_METHODS>()
            //    .Property(e => e.DISPLAY_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SHIPMENT_STATUS>()
            //    .Property(e => e.DISPLAY_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SMTP_ACCOUNTS>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SMTP_ACCOUNTS>()
            //    .Property(e => e.REPORTCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SMTP_ACCOUNTS>()
            //    .Property(e => e.SMTP_SERVER)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SMTP_ACCOUNTS>()
            //    .Property(e => e.SMTP_USER)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SMTP_ACCOUNTS>()
            //    .Property(e => e.AUTHENTICATE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SMTP_ACCOUNTS>()
            //    .Property(e => e.PASS_WORD)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SMTP_ACCOUNTS>()
            //    .Property(e => e.FROM_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SMTP_ACCOUNTS>()
            //    .Property(e => e.FROM_EMAIL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SMTP_ACCOUNTS>()
            //    .Property(e => e.REPLY_EMAIL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SMTP_ACCOUNTS>()
            //    .Property(e => e.BCC_EMAIL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SMTP_ACCOUNTS>()
            //    .Property(e => e.FILE_DIR)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SMTP_ACCOUNTS>()
            //    .Property(e => e.USETLS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SMTP_ACCOUNTS>()
            //    .Property(e => e.USESSL)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STAFF>()
            //    .Property(e => e.NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STAFF>()
            //    .Property(e => e.JOBTITLE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STAFF>()
            //    .Property(e => e.EXTENSION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STAFF>()
            //    .Property(e => e.PHONE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STAFF>()
            //    .Property(e => e.HOMEPHONE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STAFF>()
            //    .Property(e => e.ISACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STAFF>()
            //    .Property(e => e.APP_PASSWORD)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STAFF>()
            //    .Property(e => e.LOGINID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STAFF>()
            //    .Property(e => e.EMAIL_ADDRESS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STAFF>()
            //    .Property(e => e.PAYROLL_ID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STAFF>()
            //    .Property(e => e.IS_SUPERVISOR)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STAFF>()
            //    .Property(e => e.NICKNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STAFF>()
            //    .Property(e => e.ABSENT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STAFF>()
            //    .Property(e => e.HAS_BUDGETS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STAFF>()
            //    .Property(e => e.FACEBOOK_ACCESS_TOKEN)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STAFF>()
            //    .Property(e => e.LINKEDIN_TOKEN_KEY)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STAFF>()
            //    .Property(e => e.LINKEDIN_TOKEN_SECRET)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STAFF>()
            //    .Property(e => e.TWITTER_TOKEN_KEY)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STAFF>()
            //    .Property(e => e.TWITTER_TOKEN_SECRET)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STAFF>()
            //    .HasMany(e => e.CRM_BUDGET_LINE)
            //    .WithOptional(e => e.STAFF)
            //    .HasForeignKey(e => e.SALESNO);

            //modelBuilder.Entity<STOCK_CLASSIFICATIONS>()
            //    .Property(e => e.CLASSNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_COLOUR>()
            //    .Property(e => e.COLOURCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_COLOUR>()
            //    .Property(e => e.COLOURNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_COLOUR>()
            //    .Property(e => e.ISACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_CONT_HIST>()
            //    .Property(e => e.NOTE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_CONT_HIST>()
            //    .Property(e => e.SUBJECT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_CONT_HIST>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_CONT_HIST>()
            //    .Property(e => e.OUTLOOK_LINK)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_GROUP2S>()
            //    .Property(e => e.GROUPNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_GROUP2S>()
            //    .Property(e => e.STATUS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_GROUP2S>()
            //    .Property(e => e.FILENAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_GROUP2S>()
            //    .Property(e => e.DATAX_EXCHANGE_FLAG)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_GROUP2S>()
            //    .Property(e => e.AUTOCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_GROUP2S>()
            //    .Property(e => e.REPORTCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_GROUP2S>()
            //    .HasMany(e => e.CRM_BUDGET_LINE)
            //    .WithOptional(e => e.STOCK_GROUP2S)
            //    .HasForeignKey(e => e.STOCK_GROUPNO2);

            //modelBuilder.Entity<STOCK_GROUPS>()
            //    .Property(e => e.GROUPNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_GROUPS>()
            //    .Property(e => e.AUTOCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_GROUPS>()
            //    .Property(e => e.STATUS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_GROUPS>()
            //    .Property(e => e.FILENAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_GROUPS>()
            //    .Property(e => e.REPORTCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_GROUPS>()
            //    .Property(e => e.ISACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_GROUPS>()
            //    .Property(e => e.X_ISSTYLE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_GROUPS>()
            //    .Property(e => e.X_SIZEIDS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_GROUPS>()
            //    .Property(e => e.X_COLOURIDS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_GROUPS>()
            //    .Property(e => e.X_PRICEGROUP)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_GROUPS>()
            //    .Property(e => e.X_STOCKCODE_FORMULA)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_GROUPS>()
            //    .Property(e => e.X_DESCRIPTION_FORMULA)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_GROUPS>()
            //    .Property(e => e.X_BINCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_GROUPS>()
            //    .HasMany(e => e.CRM_BUDGET_LINE)
            //    .WithOptional(e => e.STOCK_GROUPS)
            //    .HasForeignKey(e => e.STOCK_GROUPNO);

            //modelBuilder.Entity<STOCK_ITEMS>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_ITEMS>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_ITEMS>()
            //    .Property(e => e.STATUS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_ITEMS>()
            //    .Property(e => e.BINCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_ITEMS>()
            //    .Property(e => e.BARCODE1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_ITEMS>()
            //    .Property(e => e.BARCODE2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_ITEMS>()
            //    .Property(e => e.BARCODE3)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_ITEMS>()
            //    .Property(e => e.WEB_SHOW)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_ITEMS>()
            //    .Property(e => e.ISACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_ITEMS>()
            //    .Property(e => e.ALERT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_ITEMS>()
            //    .Property(e => e.NOTES)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_ITEMS>()
            //    .Property(e => e.PACK)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_ITEMS>()
            //    .Property(e => e.HAS_SN)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_ITEMS>()
            //    .Property(e => e.UPDATEITEM_CODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_ITEMS>()
            //    .Property(e => e.LINKED_BILLCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_ITEMS>()
            //    .Property(e => e.HAS_BN)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_ITEMS>()
            //    .Property(e => e.HAS_EXPIRY)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_ITEMS>()
            //    .Property(e => e.IS_DISCOUNTABLE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_ITEMS>()
            //    .Property(e => e.RESTRICTED_ITEM)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_ITEMS>()
            //    .Property(e => e.VARIABLECOST)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_LOC_INFO>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_LOC_INFO>()
            //    .Property(e => e.BINCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_LOCATIONS>()
            //    .Property(e => e.LCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_LOCATIONS>()
            //    .Property(e => e.LNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_LOCATIONS>()
            //    .Property(e => e.ISACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_LOCATIONS>()
            //    .Property(e => e.EXCLUDE_FROMVALUATION)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_LOCATIONS>()
            //    .Property(e => e.EXCLUDE_FROMFREE_STOCK)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_LOCATIONS>()
            //    .Property(e => e.EXCLUDE_FROMSALES)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_LOCATIONS>()
            //    .Property(e => e.DELADDR1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_LOCATIONS>()
            //    .Property(e => e.DELADDR2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_LOCATIONS>()
            //    .Property(e => e.DELADDR3)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_LOCATIONS>()
            //    .Property(e => e.DELADDR4)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_LOCATIONS>()
            //    .Property(e => e.DELADDR5)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_LOCATIONS>()
            //    .Property(e => e.DELADDR6)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_PRICEGROUPS>()
            //    .Property(e => e.GROUPNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_REQUESTLINES>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_REQUESTLINES>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_REQUESTLINES>()
            //    .Property(e => e.COMMENT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_REQUESTLINES>()
            //    .Property(e => e.BATCHCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_REQUESTLINES>()
            //    .Property(e => e.LINKED_STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_REQUESTLINES>()
            //    .Property(e => e.BOMTYPE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_REQUESTLINES>()
            //    .Property(e => e.SHOWLINE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_REQUESTLINES>()
            //    .Property(e => e.LINKEDSTATUS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_REQUESTLINES>()
            //    .Property(e => e.BOMPRICING)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_REQUESTS>()
            //    .Property(e => e.CUSTORDERNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_REQUESTTYPES>()
            //    .Property(e => e.DISPLAY_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_SERIALNOS>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_SERIALNOS>()
            //    .Property(e => e.SERIALNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_SERIALNOS>()
            //    .Property(e => e.REFERENCE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_SERIALNOS>()
            //    .Property(e => e.INSTOCK)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_SERIALNOS>()
            //    .Property(e => e.KITCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_SERIALNOS>()
            //    .Property(e => e.KITID_SERIAL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_SERIALNOS>()
            //    .Property(e => e.ISASSIGNED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_SIZE>()
            //    .Property(e => e.SIZECODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_SIZE>()
            //    .Property(e => e.SIZENAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_SIZE>()
            //    .Property(e => e.ISACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_TRANS>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_TRANS>()
            //    .Property(e => e.REF1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_TRANS>()
            //    .Property(e => e.REF2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_TRANS>()
            //    .Property(e => e.FROM_LEDGER)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_TRANS>()
            //    .Property(e => e.BATCHCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_TRANS>()
            //    .Property(e => e.GLPOSTED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_TRANS>()
            //    .Property(e => e.UPDATEITEM_FLAG)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_TRANS>()
            //    .Property(e => e.INC_FIFO_STOCKTAKE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_TRANS>()
            //    .Property(e => e.REVERSED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_TRANS>()
            //    .Property(e => e.POST_LOOKUP_TO_GL)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_TRANS>()
            //    .Property(e => e.KITCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_TRANS>()
            //    .Property(e => e.PLU)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_TRANS>()
            //    .Property(e => e.POST_TO_GL)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_TRANS>()
            //    .Property(e => e.PERIOD_TRADED_IN_SEQ)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_TRANS_ARCHIVE>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_TRANS_ARCHIVE>()
            //    .Property(e => e.REF1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_TRANS_ARCHIVE>()
            //    .Property(e => e.REF2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_TRANS_ARCHIVE>()
            //    .Property(e => e.FROM_LEDGER)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_TRANS_ARCHIVE>()
            //    .Property(e => e.BATCHCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_TRANS_ARCHIVE>()
            //    .Property(e => e.GLPOSTED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_TRANS_ARCHIVE>()
            //    .Property(e => e.UPDATEITEM_FLAG)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_TRANS_ARCHIVE>()
            //    .Property(e => e.INC_FIFO_STOCKTAKE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_TRANS_ARCHIVE>()
            //    .Property(e => e.REVERSED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_TRANS_ARCHIVE>()
            //    .Property(e => e.POST_LOOKUP_TO_GL)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_TRANS_ARCHIVE>()
            //    .Property(e => e.KITCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_TRANS_ARCHIVE>()
            //    .Property(e => e.PLU)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_TRANS_ARCHIVE>()
            //    .Property(e => e.POST_TO_GL)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_TRANS_HDR>()
            //    .Property(e => e.REFERENCE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_TRANS_SERIALS>()
            //    .Property(e => e.SERIALNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_UNITDEFINITION>()
            //    .Property(e => e.UNITCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_UNITDEFINITION>()
            //    .Property(e => e.UNITDESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_WEB>()
            //    .Property(e => e.STOCKCODE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_WEB>()
            //    .Property(e => e.SALES_HTML)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_WEB>()
            //    .Property(e => e.PICTURE_URL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCKTAKE_CTRL>()
            //    .Property(e => e.LOCNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCKTAKE_CTRL>()
            //    .Property(e => e.ISACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCKTAKE_CTRL>()
            //    .Property(e => e.CUSTOM_FILTER)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCKTAKE_EVENTS>()
            //    .Property(e => e.FROM_LEDGER)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCKTAKE_EVENTS>()
            //    .Property(e => e.MEMO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCKTAKE_EVENTS>()
            //    .Property(e => e.OLD_VAL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCKTAKE_EVENTS>()
            //    .Property(e => e.NEW_VAL)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCKTAKE_EVENTS>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCKTAKE_EVENTS>()
            //    .Property(e => e.SERIALNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCKTAKE_EVENTS>()
            //    .Property(e => e.REF1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCKTAKE_EVENTS>()
            //    .Property(e => e.NEEDREVISIT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCKTAKE_TOTALS>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCKTAKE_TOTALS>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCKTAKE_TOTALS>()
            //    .Property(e => e.BINCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCKTAKE_TOTALS>()
            //    .Property(e => e.BATCHCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCKTAKE_TOTALS>()
            //    .Property(e => e.HAS_BN)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCKTAKE_TOTALS>()
            //    .Property(e => e.HAS_EXPIRY)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCKTAKE_TOTALS>()
            //    .Property(e => e.STOCKGROUP_REPC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCKTAKE_TOTALS>()
            //    .Property(e => e.STOCKGROUP2_REPC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCKTAKE_TOTALS>()
            //    .Property(e => e.BARCODE1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SU_WARRANTY>()
            //    .Property(e => e.WARRANTY_REF)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SU_WARRANTY>()
            //    .Property(e => e.SERIALNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SU_WARRANTY>()
            //    .Property(e => e.ISACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SU_WARRANTYTYPE>()
            //    .Property(e => e.WARRANTYDESC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SU_WARRANTYTYPE>()
            //    .Property(e => e.SUPPLIERLIABILITY)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SU_WARRANTYTYPE>()
            //    .Property(e => e.FWDREPLACEMENT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SU_WARRANTYTYPE>()
            //    .Property(e => e.CHARGE_STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SU_WARRANTYTYPE>()
            //    .Property(e => e.ISACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<SUPPLIER_STOCK_ITEMS>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SUPPLIER_STOCK_ITEMS>()
            //    .Property(e => e.SUPPLIERCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SUPPLIER_STOCK_ITEMS>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SUPPLIER_STOCK_ITEMS>()
            //    .Property(e => e.PACKREFERENCE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<SUPPLIER_STOCK_ITEMS>()
            //    .Property(e => e.IS_DEFAULT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<TASK_STATUSES>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<TASK_TYPES>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<TASKS>()
            //    .Property(e => e.SUBJECT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<TASKS>()
            //    .Property(e => e.COMPANYID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<TASKS>()
            //    .Property(e => e.COMPLETED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<TASKS>()
            //    .Property(e => e.PRIORITY)
            //    .IsUnicode(false);

            //modelBuilder.Entity<TASKS>()
            //    .Property(e => e.OUTLOOKENTRYID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<TASKS>()
            //    .Property(e => e.DELETED_FLAG)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<TASKS>()
            //    .Property(e => e.SYNC_ACTIVITY)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<TASKS>()
            //    .Property(e => e.DETAILS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<TAX_KEY_POINT>()
            //    .Property(e => e.COUNTRY)
            //    .IsUnicode(false);

            //modelBuilder.Entity<TAX_KEY_POINT>()
            //    .Property(e => e.KEY_POINT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<TAX_KEY_POINT>()
            //    .Property(e => e.DR_LEDGER)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<TAX_KEY_POINT>()
            //    .Property(e => e.CR_LEDGER)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<TAX_KEY_POINT>()
            //    .Property(e => e.KP_DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<TAX_RATES>()
            //    .Property(e => e.NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<TAX_RATES>()
            //    .Property(e => e.SHORTNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<TAX_RATES>()
            //    .Property(e => e.KEY_POINT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<TAX_RETURN>()
            //    .Property(e => e.TAXRETURNCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<TAX_RETURN>()
            //    .Property(e => e.TAXREGNO)
            //    .IsUnicode(false);

            //modelBuilder.Entity<TAX_RETURN>()
            //    .Property(e => e.INCLUDE_PREVIOUS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<TAX_RETURN>()
            //    .Property(e => e.USED_PERIODS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<TAX_RETURN>()
            //    .Property(e => e.GROSS_FROM_TAX)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<TAX_RETURN_POINT>()
            //    .Property(e => e.TAXRETURNCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<TAX_RETURN_POINT>()
            //    .Property(e => e.KEY_POINT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<TAX_RETURN_POINT>()
            //    .Property(e => e.MANUALLY_CHANGED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<TERRITORYHDR>()
            //    .Property(e => e.REPORTCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<TERRITORYHDR>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<TERRITORYHDR>()
            //    .Property(e => e.NOTES)
            //    .IsUnicode(false);

            //modelBuilder.Entity<TIME_BILLING>()
            //    .Property(e => e.CODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<TIME_BILLING>()
            //    .Property(e => e.DETAILS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<TIME_BILLING>()
            //    .Property(e => e.LINKED_STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<USER_DEF_BANK_COLS>()
            //    .Property(e => e.COLNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<USER_DEF_BANK_COLS>()
            //    .Property(e => e.PADCHAR)
            //    .IsUnicode(false);

            //modelBuilder.Entity<USER_DEF_BANK_COLS>()
            //    .Property(e => e.TEXT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<VERIFICATION_CLASS>()
            //    .Property(e => e.V_CLASS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<VERIFICATION_CLASS>()
            //    .Property(e => e.DESCRIPT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<VERIFICATION_CLASS>()
            //    .Property(e => e.HELPTEXT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<VERIFICATION_CLASS>()
            //    .Property(e => e.PROCNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<VERIFICATION_CLASS>()
            //    .Property(e => e.SOURCE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<VERIFICATION_HDR>()
            //    .Property(e => e.CONFIG_FLAG)
            //    .IsUnicode(false);

            //modelBuilder.Entity<VERIFICATION_LINES>()
            //    .Property(e => e.MESSAGE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<VERIFICATION_LINES>()
            //    .Property(e => e.V_CLASS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<VERIFICATION_LINES>()
            //    .Property(e => e.SOURCE_SEQ)
            //    .IsUnicode(false);

            //modelBuilder.Entity<VERIFICATION_LINES>()
            //    .Property(e => e.LINETYPE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<WEEK_DATES>()
            //    .Property(e => e.CLOSED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<WORKSORD_HDR>()
            //    .Property(e => e.BILLCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<WORKSORD_HDR>()
            //    .Property(e => e.PRODCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<WORKSORD_HDR>()
            //    .Property(e => e.BATCHCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<WORKSORD_HDR>()
            //    .Property(e => e.NOTES)
            //    .IsUnicode(false);

            //modelBuilder.Entity<WORKSORD_HDR>()
            //    .Property(e => e.REFERENCE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<WORKSORD_LINES>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<WORKSORD_LINES>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<WORKSORD_LINES>()
            //    .Property(e => e.BATCHCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<WORKSORD_STATUS>()
            //    .Property(e => e.STATUSDESC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_ER_REPORT>()
            //    .Property(e => e.LOGINNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_ER_REPORT>()
            //    .Property(e => e.GROUPNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_ER_REPORT>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_ER_REPORT>()
            //    .Property(e => e.CODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_ER_SETUP>()
            //    .Property(e => e.LOGINNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_ER_SETUP>()
            //    .Property(e => e.REPORTNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_ER_SETUP>()
            //    .Property(e => e.REPORTDESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_ER_SETUP>()
            //    .Property(e => e.HAS_SUBTOTAL_BELOW)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_ER_SETUP>()
            //    .Property(e => e.SHOW_CENTS)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_ER_SETUP>()
            //    .Property(e => e.SALES_FROM_BRANCHES)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_ER_SETUP>()
            //    .Property(e => e.SHOWQTYONLY)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_ER_SETUP>()
            //    .Property(e => e.SHOWDOLLARSONLY)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_ER_SETUP>()
            //    .Property(e => e.MONTH1DESC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_ER_SETUP>()
            //    .Property(e => e.MONTH2DESC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_ER_SETUP>()
            //    .Property(e => e.MONTH3DESC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_ER_SETUP>()
            //    .Property(e => e.MONTH4DESC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_ER_SETUP>()
            //    .Property(e => e.MONTH5DESC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_ER_SETUP>()
            //    .Property(e => e.MONTH6DESC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_ER_SETUP>()
            //    .Property(e => e.MONTH7DESC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_ER_SETUP>()
            //    .Property(e => e.MONTH8DESC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_ER_SETUP>()
            //    .Property(e => e.MONTH9DESC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_ER_SETUP>()
            //    .Property(e => e.MONTH10DESC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_ER_SETUP>()
            //    .Property(e => e.MONTH11DESC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_ER_SETUP>()
            //    .Property(e => e.MONTH12DESC)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_HBIZ_NOTIFICATIONS>()
            //    .Property(e => e.LINK)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_HBIZ_NOTIFICATIONS>()
            //    .Property(e => e.SUBJECT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_HBIZ_NOTIFICATIONS>()
            //    .Property(e => e.MESSAGE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_HBIZ_PURCH_REQ_COMMENTS>()
            //    .Property(e => e.COMMENT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_HBIZ_PURCH_REQ_HDR>()
            //    .Property(e => e.TITLE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_HBIZ_PURCH_REQ_HDR>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_HBIZ_PURCH_REQ_HDR>()
            //    .Property(e => e.DELADDR_1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_HBIZ_PURCH_REQ_HDR>()
            //    .Property(e => e.DELADDR_2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_HBIZ_PURCH_REQ_HDR>()
            //    .Property(e => e.DELADDR_3)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_HBIZ_PURCH_REQ_HDR>()
            //    .Property(e => e.DELADDR_4)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_HBIZ_PURCH_REQ_HDR>()
            //    .Property(e => e.DELADDR_5)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_HBIZ_PURCH_REQ_HDR>()
            //    .Property(e => e.DELADDR_6)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_HBIZ_PURCH_REQ_HDR_HISTORY>()
            //    .Property(e => e.REASON)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_HBIZ_PURCH_REQ_LINES>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_HBIZ_PURCH_REQ_LINES>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_HBIZ_PURCH_REQ_LINES>()
            //    .Property(e => e.UNITPRICE)
            //    .HasPrecision(19, 4);

            //modelBuilder.Entity<X_HBIZ_PURCH_REQ_LINES>()
            //    .Property(e => e.QUANTITY)
            //    .HasPrecision(18, 3);

            //modelBuilder.Entity<X_HBIZ_PURCH_REQ_LINES>()
            //    .Property(e => e.CR_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_HBIZ_PURCH_REQ_LINES>()
            //    .Property(e => e.ANALYSIS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_HBIZ_PURCH_REQ_LINES>()
            //    .Property(e => e.SUPPLIERCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_HBIZ_PURCH_REQ_LINES>()
            //    .Property(e => e.NARRATIVE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_HBIZ_PURCH_REQ_STATUS_VALUES>()
            //    .Property(e => e.STATE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_PURCH_REQ_COMMENTS_FILES>()
            //    .Property(e => e.FILESIZE)
            //    .HasPrecision(18, 0);

            //modelBuilder.Entity<X_PURCH_REQ_COMMENTS_FILES>()
            //    .Property(e => e.FILENAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<X_PURCH_REQ_COMMENTS_FILES>()
            //    .Property(e => e.FILETYPE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<ACCS_BALANCE>()
            //    .Property(e => e.ALPHACODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<ACCS_BALANCE>()
            //    .Property(e => e.NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<ACCS_BALANCE>()
            //    .Property(e => e.ISHEADOFFICE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<ACCS_BALANCE>()
            //    .Property(e => e.POST_CODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CAMPAIGN_TYPE>()
            //    .Property(e => e.DESCRIPT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CAMPAIGN_WAVE_AUDIT>()
            //    .Property(e => e.EXECUTE_TYPE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CHEQUE_GEN_INFO>()
            //    .Property(e => e.SUMDC)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CHEQUE_GEN_INFO>()
            //    .Property(e => e.PRINTREMIT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CHEQUE_GEN_INFO>()
            //    .Property(e => e.POPEMAIL)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CHEQUE_GEN_INFO>()
            //    .Property(e => e.FCWARNING)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CHEQUE_GEN_INFO>()
            //    .Property(e => e.EMAILREMIT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CHEQUE_GEN_INFO>()
            //    .Property(e => e.MULTIPLE_BANK_DC)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CHEQUE_GEN_INFO>()
            //    .Property(e => e.USEMAILSHOT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CHEQUE_PRINT>()
            //    .Property(e => e.InvNo)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CHEQUE_PRINT>()
            //    .Property(e => e.AmountText)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CHEQUE_PRINT>()
            //    .Property(e => e.Address)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CHEQUE_PRINT>()
            //    .Property(e => e.Details)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CHEQUE_PRINT>()
            //    .Property(e => e.Ref1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CHEQUE_PRINT>()
            //    .Property(e => e.Ref2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CHEQUE_PRINT_HDR>()
            //    .Property(e => e.ACCNAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CHEQUE_PRINT_HDR>()
            //    .Property(e => e.HASWHTAX)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<CHEQUE_PRINT_HDR>()
            //    .Property(e => e.TAXOVERRIDDEN)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<COMMUNICATION_PROCESSES>()
            //    .Property(e => e.DESCRIPT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<CONTACT_LIST_TYPE>()
            //    .Property(e => e.DESCRIPT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_SJHDR>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_SJHDR>()
            //    .Property(e => e.TEMPONLY)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_SJHDR>()
            //    .Property(e => e.INITIALS)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GL_SJHDR>()
            //    .Property(e => e.AUTO_REVERSE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLBATCH_NUMBERS>()
            //    .Property(e => e.USERID)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADERTYPE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER1)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER2)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER3)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER4)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER5)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER6)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER7)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER8)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER9)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER10)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER11)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER12)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER13)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER14)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER15)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER16)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER17)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER18)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER19)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER20)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER21)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER22)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER23)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER24)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER25)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER26)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER27)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER28)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER29)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER30)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER31)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER32)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER33)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER34)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER35)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER36)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER37)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER38)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER39)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER40)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER41)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER42)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER43)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER44)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER45)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER46)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER47)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER48)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER49)
            //    .IsUnicode(false);

            //modelBuilder.Entity<GLREPORTHEADER>()
            //    .Property(e => e.HEADER50)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYROLL_COSTCENTRE>()
            //    .Property(e => e.NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYROLL_EMPLOYEES>()
            //    .Property(e => e.LAST_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYROLL_EMPLOYEES>()
            //    .Property(e => e.FIRST_NAME)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYROLL_EMPLOYEES>()
            //    .Property(e => e.ISACTIVE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYROLL_INFO>()
            //    .Property(e => e.COMACCPATH)
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYROLL_INFO>()
            //    .Property(e => e.ENABLEMULTIBRANCH)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYROLL_INFO>()
            //    .Property(e => e.ENABLEAPPSUBACCOUNT)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYROLL_INFO>()
            //    .Property(e => e.ENFORCEPEROIDNO)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYROLL_INFO>()
            //    .Property(e => e.RENAMEFILE)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<PAYROLL_PAYRATES>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<POSTCODES>()
            //    .Property(e => e.PLACE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<POSTCODES>()
            //    .Property(e => e.DISTRICT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<POSTCODES>()
            //    .Property(e => e.PLACE_POSTCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<POSTCODES>()
            //    .Property(e => e.BOX_POSTCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<POSTCODES>()
            //    .Property(e => e.BAG_POSTCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<POSTCODES>()
            //    .Property(e => e.RD_POSTCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<POSTCODES>()
            //    .Property(e => e.STATE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_RECEIPTS>()
            //    .Property(e => e.PACKINGSLIP)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCK_RECEIPTS>()
            //    .Property(e => e.INVOICED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCKREQUIREMENT>()
            //    .Property(e => e.STOCKCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<STOCKREQUIREMENT>()
            //    .Property(e => e.DESCRIPTION)
            //    .IsUnicode(false);

            //modelBuilder.Entity<TAX_RETURN_ALLOC>()
            //    .Property(e => e.TAXRETURNCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<TAX_RETURN_ALLOC>()
            //    .Property(e => e.DRCR)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<TAX_RETURN_CALC>()
            //    .Property(e => e.DRCR)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<TAX_RETURN_CALC>()
            //    .Property(e => e.ISALLOCATED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<TAX_RETURN_CALC>()
            //    .Property(e => e.KEY_POINT)
            //    .IsUnicode(false);

            //modelBuilder.Entity<TAX_RETURN_CALC>()
            //    .Property(e => e.ALLOCATED)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<TAX_RETURN_TRANS>()
            //    .Property(e => e.TAXRETURNCODE)
            //    .IsUnicode(false);

            //modelBuilder.Entity<TAX_RETURN_TRANS>()
            //    .Property(e => e.DRCR)
            //    .IsFixedLength()
            //    .IsUnicode(false);

            //modelBuilder.Entity<TAX_RETURN_TRANS>()
            //    .Property(e => e.ALLOCATED)
            //    .IsFixedLength()
            //    .IsUnicode(false);
        }
    }
}