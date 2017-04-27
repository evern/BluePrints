using BluePrints.Common.DataModel;
using System;

namespace BluePrints.PrimeroData.PrimeroEntitiesDataModel
{
    /// <summary>
    /// IPrimeroEntitiesUnitOfWork extends the IUnitOfWork interface with repositories representing specific entities.
    /// </summary>
    public interface IPrimeroEntitiesUnitOfWork : IUnitOfWork
    {
        /// <summary>
        /// The ACCS_BALANCE entities repository.
        /// </summary>
        IRepository<ACCS_BALANCE, int> ACCS_BALANCE { get; }

        /// <summary>
        /// The ADJUSTMENT_TYPES entities repository.
        /// </summary>
        IRepository<ADJUSTMENT_TYPES, int> ADJUSTMENT_TYPES { get; }

        /// <summary>
        /// The ADVERT_TYPES entities repository.
        /// </summary>
        IRepository<ADVERT_TYPES, int> ADVERT_TYPES { get; }

        /// <summary>
        /// The ANALYSIS entities repository.
        /// </summary>
        IRepository<ANALYSIS, int> ANALYSIS { get; }

        /// <summary>
        /// The ANALYSIS_CODES entities repository.
        /// </summary>
        IRepository<ANALYSIS_CODES, int> ANALYSIS_CODES { get; }

        /// <summary>
        /// The ANALYSIS_MATRIX entities repository.
        /// </summary>
        IRepository<ANALYSIS_MATRIX, Tuple<int, int, string>> ANALYSIS_MATRIX { get; }

        /// <summary>
        /// The ANALYSIS_TYPES entities repository.
        /// </summary>
        IRepository<ANALYSIS_TYPES, string> ANALYSIS_TYPES { get; }

        /// <summary>
        /// The AUDIT_TRAIL entities repository.
        /// </summary>
        IRepository<AUDIT_TRAIL, int> AUDIT_TRAIL { get; }

        /// <summary>
        /// The AUDITOR_EVENTS entities repository.
        /// </summary>
        IRepository<AUDITOR_EVENTS, int> AUDITOR_EVENTS { get; }

        /// <summary>
        /// The AUDITOR_SAVEPROCS entities repository.
        /// </summary>
        IRepository<AUDITOR_SAVEPROCS, Tuple<int, int>> AUDITOR_SAVEPROCS { get; }

        /// <summary>
        /// The AUDITOR_SETTINGS entities repository.
        /// </summary>
        IRepository<AUDITOR_SETTINGS, Tuple<int, string, int, int, int, int>> AUDITOR_SETTINGS { get; }

        /// <summary>
        /// The AUDITOR_TERMINALS entities repository.
        /// </summary>
        IRepository<AUDITOR_TERMINALS, Tuple<int, string>> AUDITOR_TERMINALS { get; }

        /// <summary>
        /// The AUTO_PURCH entities repository.
        /// </summary>
        IRepository<AUTO_PURCH, Tuple<string, int, int>> AUTO_PURCH { get; }

        /// <summary>
        /// The BANK_BATCH_PRINT entities repository.
        /// </summary>
        IRepository<BANK_BATCH_PRINT, int> BANK_BATCH_PRINT { get; }

        /// <summary>
        /// The BANK_DATA_FORMAT entities repository.
        /// </summary>
        IRepository<BANK_DATA_FORMAT, int> BANK_DATA_FORMAT { get; }

        /// <summary>
        /// The BANK_DATA_TYPE entities repository.
        /// </summary>
        IRepository<BANK_DATA_TYPE, int> BANK_DATA_TYPE { get; }

        /// <summary>
        /// The BANK_REC_AUTO_MATCH entities repository.
        /// </summary>
        IRepository<BANK_REC_AUTO_MATCH, int> BANK_REC_AUTO_MATCH { get; }

        /// <summary>
        /// The BANK_REC_LOG entities repository.
        /// </summary>
        IRepository<BANK_REC_LOG, int> BANK_REC_LOG { get; }

        /// <summary>
        /// The BANK_REC_SETUP entities repository.
        /// </summary>
        IRepository<BANK_REC_SETUP, int> BANK_REC_SETUP { get; }

        /// <summary>
        /// The BANK_REC_UPLOADS entities repository.
        /// </summary>
        IRepository<BANK_REC_UPLOADS, int> BANK_REC_UPLOADS { get; }

        /// <summary>
        /// The BANK_TEMPLATES entities repository.
        /// </summary>
        IRepository<BANK_TEMPLATES, int> BANK_TEMPLATES { get; }

        /// <summary>
        /// The BANKBATCH_HDR entities repository.
        /// </summary>
        IRepository<BANKBATCH_HDR, int> BANKBATCH_HDR { get; }

        /// <summary>
        /// The BANKS entities repository.
        /// </summary>
        IRepository<BANKS, int> BANKS { get; }

        /// <summary>
        /// The BATCH_QUANTITIES entities repository.
        /// </summary>
        IRepository<BATCH_QUANTITIES, int> BATCH_QUANTITIES { get; }

        /// <summary>
        /// The BATCH_STOCKTAKE entities repository.
        /// </summary>
        IRepository<BATCH_STOCKTAKE, int> BATCH_STOCKTAKE { get; }

        /// <summary>
        /// The BILLOMAT_HDR entities repository.
        /// </summary>
        IRepository<BILLOMAT_HDR, string> BILLOMAT_HDR { get; }

        /// <summary>
        /// The BILLOMAT_LINES entities repository.
        /// </summary>
        IRepository<BILLOMAT_LINES, int> BILLOMAT_LINES { get; }

        /// <summary>
        /// The BILLOMAT_TEMP entities repository.
        /// </summary>
        IRepository<BILLOMAT_TEMP, int> BILLOMAT_TEMP { get; }

        /// <summary>
        /// The BPAY_TYPES entities repository.
        /// </summary>
        IRepository<BPAY_TYPES, int> BPAY_TYPES { get; }

        /// <summary>
        /// The BRANCHES entities repository.
        /// </summary>
        IRepository<BRANCHES, int> BRANCHES { get; }

        /// <summary>
        /// The CAMPAIGN entities repository.
        /// </summary>
        IRepository<CAMPAIGN, int> CAMPAIGN { get; }

        /// <summary>
        /// The CAMPAIGN_CONTACT_LISTS entities repository.
        /// </summary>
        IReadOnlyRepository<CAMPAIGN_CONTACT_LISTS> CAMPAIGN_CONTACT_LISTS { get; }

        /// <summary>
        /// The CAMPAIGN_HIST entities repository.
        /// </summary>
        IRepository<CAMPAIGN_HIST, int> CAMPAIGN_HIST { get; }

        /// <summary>
        /// The CAMPAIGN_STAGE entities repository.
        /// </summary>
        IRepository<CAMPAIGN_STAGE, int> CAMPAIGN_STAGE { get; }

        /// <summary>
        /// The CAMPAIGN_STAGE_CONSTRAINT entities repository.
        /// </summary>
        IRepository<CAMPAIGN_STAGE_CONSTRAINT, int> CAMPAIGN_STAGE_CONSTRAINT { get; }

        /// <summary>
        /// The CAMPAIGN_TYPE entities repository.
        /// </summary>
        IRepository<CAMPAIGN_TYPE, int> CAMPAIGN_TYPE { get; }

        /// <summary>
        /// The CAMPAIGN_WAVE entities repository.
        /// </summary>
        IRepository<CAMPAIGN_WAVE, int> CAMPAIGN_WAVE { get; }

        /// <summary>
        /// The CAMPAIGN_WAVE_AUDIT entities repository.
        /// </summary>
        IRepository<CAMPAIGN_WAVE_AUDIT, Tuple<int, int, int, DateTime>> CAMPAIGN_WAVE_AUDIT { get; }

        /// <summary>
        /// The CAMPAIGN_WAVE_CONTACT_LISTS entities repository.
        /// </summary>
        IReadOnlyRepository<CAMPAIGN_WAVE_CONTACT_LISTS> CAMPAIGN_WAVE_CONTACT_LISTS { get; }

        /// <summary>
        /// The CASHBOOKHEADER entities repository.
        /// </summary>
        IRepository<CASHBOOKHEADER, int> CASHBOOKHEADER { get; }

        /// <summary>
        /// The CASHBOOKLINE entities repository.
        /// </summary>
        IRepository<CASHBOOKLINE, int> CASHBOOKLINE { get; }

        /// <summary>
        /// The CASHBOOKLINEALLOC entities repository.
        /// </summary>
        IRepository<CASHBOOKLINEALLOC, int> CASHBOOKLINEALLOC { get; }

        /// <summary>
        /// The CHECKLIST_WIDGET_NODES entities repository.
        /// </summary>
        IRepository<CHECKLIST_WIDGET_NODES, int> CHECKLIST_WIDGET_NODES { get; }

        /// <summary>
        /// The CHEQUE_AUDIT entities repository.
        /// </summary>
        IRepository<CHEQUE_AUDIT, int> CHEQUE_AUDIT { get; }

        /// <summary>
        /// The CHEQUE_GEN_INFO entities repository.
        /// </summary>
        IRepository<CHEQUE_GEN_INFO, Tuple<int, int, int, string>> CHEQUE_GEN_INFO { get; }

        /// <summary>
        /// The CHEQUE_PRINT entities repository.
        /// </summary>
        IRepository<CHEQUE_PRINT, Tuple<int, string, double, double>> CHEQUE_PRINT { get; }

        /// <summary>
        /// The CHEQUE_PRINT_HDR entities repository.
        /// </summary>
        IRepository<CHEQUE_PRINT_HDR, int> CHEQUE_PRINT_HDR { get; }

        /// <summary>
        /// The CL_END_OF_PERIOD entities repository.
        /// </summary>
        IRepository<CL_END_OF_PERIOD, int> CL_END_OF_PERIOD { get; }

        /// <summary>
        /// The CL_END_OF_YEAR entities repository.
        /// </summary>
        IRepository<CL_END_OF_YEAR, int> CL_END_OF_YEAR { get; }

        /// <summary>
        /// The CL_FINANCIAL_REVIEW entities repository.
        /// </summary>
        IRepository<CL_FINANCIAL_REVIEW, int> CL_FINANCIAL_REVIEW { get; }

        /// <summary>
        /// The CL_SYSTEMS_INTEGRITY entities repository.
        /// </summary>
        IRepository<CL_SYSTEMS_INTEGRITY, int> CL_SYSTEMS_INTEGRITY { get; }

        /// <summary>
        /// The CL_TAX_RETURN entities repository.
        /// </summary>
        IRepository<CL_TAX_RETURN, int> CL_TAX_RETURN { get; }

        /// <summary>
        /// The ClarityReport entities repository.
        /// </summary>
        IRepository<ClarityReport, int> ClarityReport { get; }

        /// <summary>
        /// The COMMON_PHRASES entities repository.
        /// </summary>
        IRepository<COMMON_PHRASES, int> COMMON_PHRASES { get; }

        /// <summary>
        /// The COMMUNICATION_PROCESSES entities repository.
        /// </summary>
        IRepository<COMMUNICATION_PROCESSES, int> COMMUNICATION_PROCESSES { get; }

        /// <summary>
        /// The COMPANY_TYPES entities repository.
        /// </summary>
        IRepository<COMPANY_TYPES, int> COMPANY_TYPES { get; }

        /// <summary>
        /// The COMPUTER entities repository.
        /// </summary>
        IRepository<COMPUTER, int> COMPUTER { get; }

        /// <summary>
        /// The CONTACT_HIST entities repository.
        /// </summary>
        IRepository<CONTACT_HIST, int> CONTACT_HIST { get; }

        /// <summary>
        /// The CONTACT_LIST entities repository.
        /// </summary>
        IRepository<CONTACT_LIST, int> CONTACT_LIST { get; }

        /// <summary>
        /// The CONTACT_LIST_ITEM entities repository.
        /// </summary>
        IRepository<CONTACT_LIST_ITEM, int> CONTACT_LIST_ITEM { get; }

        /// <summary>
        /// The CONTACT_LIST_TYPE entities repository.
        /// </summary>
        IRepository<CONTACT_LIST_TYPE, int> CONTACT_LIST_TYPE { get; }

        /// <summary>
        /// The CONTACT_MARKETING_CLASSES entities repository.
        /// </summary>
        IRepository<CONTACT_MARKETING_CLASSES, int> CONTACT_MARKETING_CLASSES { get; }

        /// <summary>
        /// The CONTACTS entities repository.
        /// </summary>
        IRepository<CONTACTS, int> CONTACTS { get; }

        /// <summary>
        /// The COUNTRY entities repository.
        /// </summary>
        IRepository<COUNTRY, string> COUNTRY { get; }

        /// <summary>
        /// The Courier_Location_Depot entities repository.
        /// </summary>
        IRepository<Courier_Location_Depot, int> Courier_Location_Depot { get; }

        /// <summary>
        /// The COURIER_MANIFEST entities repository.
        /// </summary>
        IRepository<COURIER_MANIFEST, int> COURIER_MANIFEST { get; }

        /// <summary>
        /// The COURIERS entities repository.
        /// </summary>
        IRepository<COURIERS, int> COURIERS { get; }

        /// <summary>
        /// The CR_ACCGROUP2S entities repository.
        /// </summary>
        IRepository<CR_ACCGROUP2S, int> CR_ACCGROUP2S { get; }

        /// <summary>
        /// The CR_ACCGROUPS entities repository.
        /// </summary>
        IRepository<CR_ACCGROUPS, int> CR_ACCGROUPS { get; }

        /// <summary>
        /// The CR_ACCS entities repository.
        /// </summary>
        IRepository<CR_ACCS, int> CR_ACCS { get; }

        /// <summary>
        /// The CR_ALLOCATIONS entities repository.
        /// </summary>
        IRepository<CR_ALLOCATIONS, Tuple<int, int>> CR_ALLOCATIONS { get; }

        /// <summary>
        /// The CR_CONT_HIST entities repository.
        /// </summary>
        IRepository<CR_CONT_HIST, int> CR_CONT_HIST { get; }

        /// <summary>
        /// The CR_CONTACTS entities repository.
        /// </summary>
        IRepository<CR_CONTACTS, int> CR_CONTACTS { get; }

        /// <summary>
        /// The CR_INVLINES entities repository.
        /// </summary>
        IRepository<CR_INVLINES, int> CR_INVLINES { get; }

        /// <summary>
        /// The CR_INVLINES_SERIALS entities repository.
        /// </summary>
        IRepository<CR_INVLINES_SERIALS, int> CR_INVLINES_SERIALS { get; }

        /// <summary>
        /// The CR_LIST_NAME entities repository.
        /// </summary>
        IRepository<CR_LIST_NAME, int> CR_LIST_NAME { get; }

        /// <summary>
        /// The CR_LISTS entities repository.
        /// </summary>
        IRepository<CR_LISTS, Tuple<int, int>> CR_LISTS { get; }

        /// <summary>
        /// The CR_PRICES entities repository.
        /// </summary>
        IRepository<CR_PRICES, int> CR_PRICES { get; }

        /// <summary>
        /// The CR_TRANS entities repository.
        /// </summary>
        IRepository<CR_TRANS, int> CR_TRANS { get; }

        /// <summary>
        /// The CREDIT_STATUSES entities repository.
        /// </summary>
        IRepository<CREDIT_STATUSES, int> CREDIT_STATUSES { get; }

        /// <summary>
        /// The CRM_BUDGET entities repository.
        /// </summary>
        IRepository<CRM_BUDGET, int> CRM_BUDGET { get; }

        /// <summary>
        /// The CRM_BUDGET_HDR entities repository.
        /// </summary>
        IRepository<CRM_BUDGET_HDR, int> CRM_BUDGET_HDR { get; }

        /// <summary>
        /// The CRM_BUDGET_LINE entities repository.
        /// </summary>
        IRepository<CRM_BUDGET_LINE, int> CRM_BUDGET_LINE { get; }

        /// <summary>
        /// The CURRENCIES entities repository.
        /// </summary>
        IRepository<CURRENCIES, int> CURRENCIES { get; }

        /// <summary>
        /// The CURRENCY_CLOSING_RATES entities repository.
        /// </summary>
        IRepository<CURRENCY_CLOSING_RATES, Tuple<int, int>> CURRENCY_CLOSING_RATES { get; }

        /// <summary>
        /// The CURRENCY_RATECHANGES entities repository.
        /// </summary>
        IRepository<CURRENCY_RATECHANGES, int> CURRENCY_RATECHANGES { get; }

        /// <summary>
        /// The CUSTCLARITYREPORT entities repository.
        /// </summary>
        IRepository<CUSTCLARITYREPORT, int> CUSTCLARITYREPORT { get; }

        /// <summary>
        /// The CUSTOM_REPORTS entities repository.
        /// </summary>
        IRepository<CUSTOM_REPORTS, string> CUSTOM_REPORTS { get; }

        /// <summary>
        /// The CUSTOM_VIEWS entities repository.
        /// </summary>
        IRepository<CUSTOM_VIEWS, int> CUSTOM_VIEWS { get; }

        /// <summary>
        /// The CUSTOMIZATION entities repository.
        /// </summary>
        IRepository<CUSTOMIZATION, int> CUSTOMIZATION { get; }

        /// <summary>
        /// The D_DEBIT_HDR entities repository.
        /// </summary>
        IRepository<D_DEBIT_HDR, int> D_DEBIT_HDR { get; }

        /// <summary>
        /// The D_DEBIT_LINES entities repository.
        /// </summary>
        IRepository<D_DEBIT_LINES, int> D_DEBIT_LINES { get; }

        /// <summary>
        /// The DASHBOARDS entities repository.
        /// </summary>
        IRepository<DASHBOARDS, int> DASHBOARDS { get; }

        /// <summary>
        /// The DASHBOARDS_STAFF entities repository.
        /// </summary>
        IRepository<DASHBOARDS_STAFF, int> DASHBOARDS_STAFF { get; }

        /// <summary>
        /// The DATAEXPORT_CTRL entities repository.
        /// </summary>
        IRepository<DATAEXPORT_CTRL, int> DATAEXPORT_CTRL { get; }

        /// <summary>
        /// The DATE_RANGES entities repository.
        /// </summary>
        IRepository<DATE_RANGES, int> DATE_RANGES { get; }

        /// <summary>
        /// The DISCOUNTS entities repository.
        /// </summary>
        IRepository<DISCOUNTS, Tuple<int, int>> DISCOUNTS { get; }

        /// <summary>
        /// The DISPLAY_NAMES entities repository.
        /// </summary>
        IRepository<DISPLAY_NAMES, string> DISPLAY_NAMES { get; }

        /// <summary>
        /// The DISPMETHOD entities repository.
        /// </summary>
        IRepository<DISPMETHOD, int> DISPMETHOD { get; }

        /// <summary>
        /// The DOC_BATCH_HDR entities repository.
        /// </summary>
        IRepository<DOC_BATCH_HDR, int> DOC_BATCH_HDR { get; }

        /// <summary>
        /// The DOC_BATCH_LINES entities repository.
        /// </summary>
        IRepository<DOC_BATCH_LINES, int> DOC_BATCH_LINES { get; }

        /// <summary>
        /// The DOCUMENTS entities repository.
        /// </summary>
        IRepository<DOCUMENTS, int> DOCUMENTS { get; }

        /// <summary>
        /// The DR_ACCGROUP2S entities repository.
        /// </summary>
        IRepository<DR_ACCGROUP2S, int> DR_ACCGROUP2S { get; }

        /// <summary>
        /// The DR_ACCGROUPS entities repository.
        /// </summary>
        IRepository<DR_ACCGROUPS, int> DR_ACCGROUPS { get; }

        /// <summary>
        /// The DR_ACCS entities repository.
        /// </summary>
        IRepository<DR_ACCS, int> DR_ACCS { get; }

        /// <summary>
        /// The DR_ADDRESSES entities repository.
        /// </summary>
        IRepository<DR_ADDRESSES, int> DR_ADDRESSES { get; }

        /// <summary>
        /// The DR_ALLOCATIONS entities repository.
        /// </summary>
        IRepository<DR_ALLOCATIONS, Tuple<int, int>> DR_ALLOCATIONS { get; }

        /// <summary>
        /// The DR_CONT_HIST entities repository.
        /// </summary>
        IRepository<DR_CONT_HIST, int> DR_CONT_HIST { get; }

        /// <summary>
        /// The DR_CONTACTS entities repository.
        /// </summary>
        IRepository<DR_CONTACTS, int> DR_CONTACTS { get; }

        /// <summary>
        /// The DR_INVLINES entities repository.
        /// </summary>
        IRepository<DR_INVLINES, int> DR_INVLINES { get; }

        /// <summary>
        /// The DR_INVLINES_SERIALS entities repository.
        /// </summary>
        IRepository<DR_INVLINES_SERIALS, int> DR_INVLINES_SERIALS { get; }

        /// <summary>
        /// The DR_PRICE_POLICY entities repository.
        /// </summary>
        IRepository<DR_PRICE_POLICY, int> DR_PRICE_POLICY { get; }

        /// <summary>
        /// The DR_PRICE_POLICY_ACC entities repository.
        /// </summary>
        IRepository<DR_PRICE_POLICY_ACC, int> DR_PRICE_POLICY_ACC { get; }

        /// <summary>
        /// The DR_PRICEGROUPS entities repository.
        /// </summary>
        IRepository<DR_PRICEGROUPS, int> DR_PRICEGROUPS { get; }

        /// <summary>
        /// The DR_PRICES entities repository.
        /// </summary>
        IRepository<DR_PRICES, int> DR_PRICES { get; }

        /// <summary>
        /// The DR_TRANS entities repository.
        /// </summary>
        IRepository<DR_TRANS, int> DR_TRANS { get; }

        /// <summary>
        /// The dtproperties entities repository.
        /// </summary>
        IRepository<dtproperties, Tuple<int, string>> dtproperties { get; }

        /// <summary>
        /// The EFT_AUDIT entities repository.
        /// </summary>
        IRepository<EFT_AUDIT, int> EFT_AUDIT { get; }

        /// <summary>
        /// The EXONET_CACHE_TABLES entities repository.
        /// </summary>
        IRepository<EXONET_CACHE_TABLES, Tuple<int, string>> EXONET_CACHE_TABLES { get; }

        /// <summary>
        /// The EXTRA_FIELDS entities repository.
        /// </summary>
        IRepository<EXTRA_FIELDS, int> EXTRA_FIELDS { get; }

        /// <summary>
        /// The FACT_GLOSSARY entities repository.
        /// </summary>
        IRepository<FACT_GLOSSARY, string> FACT_GLOSSARY { get; }

        /// <summary>
        /// The FORMULAHDR entities repository.
        /// </summary>
        IRepository<FORMULAHDR, int> FORMULAHDR { get; }

        /// <summary>
        /// The FORMULALINES entities repository.
        /// </summary>
        IRepository<FORMULALINES, int> FORMULALINES { get; }

        /// <summary>
        /// The FREIGHT_COST_TYPES entities repository.
        /// </summary>
        IRepository<FREIGHT_COST_TYPES, string> FREIGHT_COST_TYPES { get; }

        /// <summary>
        /// The FREIGHTCOST_SPREAD_TYPE entities repository.
        /// </summary>
        IRepository<FREIGHTCOST_SPREAD_TYPE, int> FREIGHTCOST_SPREAD_TYPE { get; }

        /// <summary>
        /// The GENERAL_INFO entities repository.
        /// </summary>
        IRepository<GENERAL_INFO, int> GENERAL_INFO { get; }

        /// <summary>
        /// The GL_ACCGROUPS entities repository.
        /// </summary>
        IRepository<GL_ACCGROUPS, int> GL_ACCGROUPS { get; }

        /// <summary>
        /// The GL_ACCGRP_REPCODE entities repository.
        /// </summary>
        IRepository<GL_ACCGRP_REPCODE, int> GL_ACCGRP_REPCODE { get; }

        /// <summary>
        /// The GL_CLOSING_STOCK entities repository.
        /// </summary>
        IRepository<GL_CLOSING_STOCK, Tuple<int, int, int, int>> GL_CLOSING_STOCK { get; }

        /// <summary>
        /// The GL_CONT_HIST entities repository.
        /// </summary>
        IRepository<GL_CONT_HIST, int> GL_CONT_HIST { get; }

        /// <summary>
        /// The GL_CONTROL entities repository.
        /// </summary>
        IRepository<GL_CONTROL, int> GL_CONTROL { get; }

        /// <summary>
        /// The GL_POSTBATCH entities repository.
        /// </summary>
        IRepository<GL_POSTBATCH, int> GL_POSTBATCH { get; }

        /// <summary>
        /// The GL_REPORTCOLS entities repository.
        /// </summary>
        IRepository<GL_REPORTCOLS, Tuple<int, int>> GL_REPORTCOLS { get; }

        /// <summary>
        /// The GL_REPORTROWS entities repository.
        /// </summary>
        IRepository<GL_REPORTROWS, Tuple<int, int>> GL_REPORTROWS { get; }

        /// <summary>
        /// The GL_REPORTS entities repository.
        /// </summary>
        IRepository<GL_REPORTS, int> GL_REPORTS { get; }

        /// <summary>
        /// The GL_SJHDR entities repository.
        /// </summary>
        IRepository<GL_SJHDR, Tuple<int, int>> GL_SJHDR { get; }

        /// <summary>
        /// The GL_SJLINES entities repository.
        /// </summary>
        IRepository<GL_SJLINES, Tuple<int, int>> GL_SJLINES { get; }

        /// <summary>
        /// The GL_SJPOSTRUN entities repository.
        /// </summary>
        IRepository<GL_SJPOSTRUN, int> GL_SJPOSTRUN { get; }

        /// <summary>
        /// The GLACCS entities repository.
        /// </summary>
        IRepository<GLACCS, int> GLACCS { get; }

        /// <summary>
        /// The GLBATCH entities repository.
        /// </summary>
        IRepository<GLBATCH, int> GLBATCH { get; }

        /// <summary>
        /// The GLBATCH_NUMBERS entities repository.
        /// </summary>
        IReadOnlyRepository<GLBATCH_NUMBERS> GLBATCH_NUMBERS { get; }

        /// <summary>
        /// The GLBUDGETS entities repository.
        /// </summary>
        IRepository<GLBUDGETS, int> GLBUDGETS { get; }

        /// <summary>
        /// The GLBUDGETS_HDR entities repository.
        /// </summary>
        IRepository<GLBUDGETS_HDR, int> GLBUDGETS_HDR { get; }

        /// <summary>
        /// The GLCHART_BUSINESS entities repository.
        /// </summary>
        IRepository<GLCHART_BUSINESS, int> GLCHART_BUSINESS { get; }

        /// <summary>
        /// The GLCHART_INDUSTRY entities repository.
        /// </summary>
        IRepository<GLCHART_INDUSTRY, int> GLCHART_INDUSTRY { get; }

        /// <summary>
        /// The GLMOVEMENTS entities repository.
        /// </summary>
        IRepository<GLMOVEMENTS, int> GLMOVEMENTS { get; }

        /// <summary>
        /// The GLREPORT_BATCH entities repository.
        /// </summary>
        IRepository<GLREPORT_BATCH, int> GLREPORT_BATCH { get; }

        /// <summary>
        /// The GLREPORT_BATCH_LINE entities repository.
        /// </summary>
        IRepository<GLREPORT_BATCH_LINE, int> GLREPORT_BATCH_LINE { get; }

        /// <summary>
        /// The GLREPORTHEADER entities repository.
        /// </summary>
        IRepository<GLREPORTHEADER, int> GLREPORTHEADER { get; }

        /// <summary>
        /// The GLSUBACCS entities repository.
        /// </summary>
        IRepository<GLSUBACCS, Tuple<int, int>> GLSUBACCS { get; }

        /// <summary>
        /// The GLTRANS entities repository.
        /// </summary>
        IRepository<GLTRANS, int> GLTRANS { get; }

        /// <summary>
        /// The GLTRANS_ARCHIVE entities repository.
        /// </summary>
        IRepository<GLTRANS_ARCHIVE, int> GLTRANS_ARCHIVE { get; }

        /// <summary>
        /// The GRAPH_SERIES entities repository.
        /// </summary>
        IRepository<GRAPH_SERIES, int> GRAPH_SERIES { get; }

        /// <summary>
        /// The IDENTIFIERS entities repository.
        /// </summary>
        IRepository<IDENTIFIERS, Tuple<string, string>> IDENTIFIERS { get; }

        /// <summary>
        /// The INV_FIELD_NAMES entities repository.
        /// </summary>
        IRepository<INV_FIELD_NAMES, int> INV_FIELD_NAMES { get; }

        /// <summary>
        /// The INV_NUMBERS entities repository.
        /// </summary>
        IRepository<INV_NUMBERS, int> INV_NUMBERS { get; }

        /// <summary>
        /// The INWARDS_GOODS entities repository.
        /// </summary>
        IRepository<INWARDS_GOODS, int> INWARDS_GOODS { get; }

        /// <summary>
        /// The INWARDS_GOODS_COSTS entities repository.
        /// </summary>
        IRepository<INWARDS_GOODS_COSTS, int> INWARDS_GOODS_COSTS { get; }

        /// <summary>
        /// The INWARDS_GOODS_LINES entities repository.
        /// </summary>
        IRepository<INWARDS_GOODS_LINES, int> INWARDS_GOODS_LINES { get; }

        /// <summary>
        /// The JOB_CATEGORIES entities repository.
        /// </summary>
        IRepository<JOB_CATEGORIES, int> JOB_CATEGORIES { get; }

        /// <summary>
        /// The JOB_CONTRACT_BILLINGS entities repository.
        /// </summary>
        IRepository<JOB_CONTRACT_BILLINGS, int> JOB_CONTRACT_BILLINGS { get; }

        /// <summary>
        /// The JOB_COSTGROUPS entities repository.
        /// </summary>
        IRepository<JOB_COSTGROUPS, int> JOB_COSTGROUPS { get; }

        /// <summary>
        /// The JOB_COSTTYPES entities repository.
        /// </summary>
        IRepository<JOB_COSTTYPES, int> JOB_COSTTYPES { get; }

        /// <summary>
        /// The JOB_HIST entities repository.
        /// </summary>
        IRepository<JOB_HIST, int> JOB_HIST { get; }

        /// <summary>
        /// The JOB_OTHER_REPORTS entities repository.
        /// </summary>
        IRepository<JOB_OTHER_REPORTS, int> JOB_OTHER_REPORTS { get; }

        /// <summary>
        /// The JOB_OUTPUT_ITEMS entities repository.
        /// </summary>
        IRepository<JOB_OUTPUT_ITEMS, int> JOB_OUTPUT_ITEMS { get; }

        /// <summary>
        /// The JOB_QUOTE_OPTIONS entities repository.
        /// </summary>
        IRepository<JOB_QUOTE_OPTIONS, int> JOB_QUOTE_OPTIONS { get; }

        /// <summary>
        /// The JOB_RESOURCE_ALLOCATION entities repository.
        /// </summary>
        IRepository<JOB_RESOURCE_ALLOCATION, int> JOB_RESOURCE_ALLOCATION { get; }

        /// <summary>
        /// The JOB_RETENTION_LEVELS entities repository.
        /// </summary>
        IRepository<JOB_RETENTION_LEVELS, int> JOB_RETENTION_LEVELS { get; }

        /// <summary>
        /// The JOB_STATUS entities repository.
        /// </summary>
        IRepository<JOB_STATUS, string> JOB_STATUS { get; }

        /// <summary>
        /// The JOB_STATUS_CONSTRAINT entities repository.
        /// </summary>
        IRepository<JOB_STATUS_CONSTRAINT, int> JOB_STATUS_CONSTRAINT { get; }

        /// <summary>
        /// The JOB_TIMESHEET_ALLOWANCE entities repository.
        /// </summary>
        IRepository<JOB_TIMESHEET_ALLOWANCE, int> JOB_TIMESHEET_ALLOWANCE { get; }

        /// <summary>
        /// The JOB_TIMESHEET_ALLOWANCE_TYPES entities repository.
        /// </summary>
        IRepository<JOB_TIMESHEET_ALLOWANCE_TYPES, int> JOB_TIMESHEET_ALLOWANCE_TYPES { get; }

        /// <summary>
        /// The JOB_TIMESHEETS entities repository.
        /// </summary>
        IRepository<JOB_TIMESHEETS, int> JOB_TIMESHEETS { get; }

        /// <summary>
        /// The JOB_TIMESHEETS_RATES entities repository.
        /// </summary>
        IRepository<JOB_TIMESHEETS_RATES, int> JOB_TIMESHEETS_RATES { get; }

        /// <summary>
        /// The JOB_TRANSACTIONS entities repository.
        /// </summary>
        IRepository<JOB_TRANSACTIONS, int> JOB_TRANSACTIONS { get; }

        /// <summary>
        /// The JOB_TYPES entities repository.
        /// </summary>
        IRepository<JOB_TYPES, int> JOB_TYPES { get; }

        /// <summary>
        /// The JOBCOST_FLAGS entities repository.
        /// </summary>
        IRepository<JOBCOST_FLAGS, int> JOBCOST_FLAGS { get; }

        /// <summary>
        /// The JOBCOST_FLAGSDESC entities repository.
        /// </summary>
        IRepository<JOBCOST_FLAGSDESC, string> JOBCOST_FLAGSDESC { get; }

        /// <summary>
        /// The JOBCOST_GENERAL_INFO entities repository.
        /// </summary>
        IRepository<JOBCOST_GENERAL_INFO, int> JOBCOST_GENERAL_INFO { get; }

        /// <summary>
        /// The JOBCOST_HDR entities repository.
        /// </summary>
        IRepository<JOBCOST_HDR, int> JOBCOST_HDR { get; }

        /// <summary>
        /// The JOBCOST_LINES entities repository.
        /// </summary>
        IRepository<JOBCOST_LINES, int> JOBCOST_LINES { get; }

        /// <summary>
        /// The JOBCOST_PROJ entities repository.
        /// </summary>
        IRepository<JOBCOST_PROJ, int> JOBCOST_PROJ { get; }

        /// <summary>
        /// The JOBCOST_RESOURCE entities repository.
        /// </summary>
        IRepository<JOBCOST_RESOURCE, int> JOBCOST_RESOURCE { get; }

        /// <summary>
        /// The KIT_SERIAL_INFO entities repository.
        /// </summary>
        IRepository<KIT_SERIAL_INFO, int> KIT_SERIAL_INFO { get; }

        /// <summary>
        /// The LEDGER_PERIODS entities repository.
        /// </summary>
        IRepository<LEDGER_PERIODS, int> LEDGER_PERIODS { get; }

        /// <summary>
        /// The LEGENDS entities repository.
        /// </summary>
        IRepository<LEGENDS, int> LEGENDS { get; }

        /// <summary>
        /// The MANIFESTS entities repository.
        /// </summary>
        IRepository<MANIFESTS, int> MANIFESTS { get; }

        /// <summary>
        /// The MANREP entities repository.
        /// </summary>
        IRepository<MANREP, int> MANREP { get; }

        /// <summary>
        /// The MANREP_BRANCH entities repository.
        /// </summary>
        IRepository<MANREP_BRANCH, int> MANREP_BRANCH { get; }

        /// <summary>
        /// The MANREP_BRANCH_BUDGET entities repository.
        /// </summary>
        IRepository<MANREP_BRANCH_BUDGET, int> MANREP_BRANCH_BUDGET { get; }

        /// <summary>
        /// The MANREP_BUDGET entities repository.
        /// </summary>
        IRepository<MANREP_BUDGET, int> MANREP_BUDGET { get; }

        /// <summary>
        /// The MANREP_CLOSING_STOCK entities repository.
        /// </summary>
        IRepository<MANREP_CLOSING_STOCK, Tuple<int, string>> MANREP_CLOSING_STOCK { get; }

        /// <summary>
        /// The MANREP_DAYPLAN entities repository.
        /// </summary>
        IRepository<MANREP_DAYPLAN, Tuple<DateTime, int>> MANREP_DAYPLAN { get; }

        /// <summary>
        /// The MANREP_PERIOD entities repository.
        /// </summary>
        IRepository<MANREP_PERIOD, int> MANREP_PERIOD { get; }

        /// <summary>
        /// The MANREP_SALESPERSON entities repository.
        /// </summary>
        IRepository<MANREP_SALESPERSON, int> MANREP_SALESPERSON { get; }

        /// <summary>
        /// The MANREP_SETUP entities repository.
        /// </summary>
        IRepository<MANREP_SETUP, int> MANREP_SETUP { get; }

        /// <summary>
        /// The MANREP_STOCK entities repository.
        /// </summary>
        IRepository<MANREP_STOCK, Tuple<int, int>> MANREP_STOCK { get; }

        /// <summary>
        /// The MENU_ASSIGNMENTS entities repository.
        /// </summary>
        IRepository<MENU_ASSIGNMENTS, string> MENU_ASSIGNMENTS { get; }

        /// <summary>
        /// The MENU_COLLECTION entities repository.
        /// </summary>
        IRepository<MENU_COLLECTION, int> MENU_COLLECTION { get; }

        /// <summary>
        /// The MENU_HDR entities repository.
        /// </summary>
        IRepository<MENU_HDR, int> MENU_HDR { get; }

        /// <summary>
        /// The MENU_LINES entities repository.
        /// </summary>
        IRepository<MENU_LINES, int> MENU_LINES { get; }

        /// <summary>
        /// The MENU_MASTER entities repository.
        /// </summary>
        IRepository<MENU_MASTER, int> MENU_MASTER { get; }

        /// <summary>
        /// The MENU_OPTIONS entities repository.
        /// </summary>
        IRepository<MENU_OPTIONS, int> MENU_OPTIONS { get; }

        /// <summary>
        /// The MENU_PROCEDURES entities repository.
        /// </summary>
        IRepository<MENU_PROCEDURES, int> MENU_PROCEDURES { get; }

        /// <summary>
        /// The MODULE_SECURITY entities repository.
        /// </summary>
        IRepository<MODULE_SECURITY, int> MODULE_SECURITY { get; }

        /// <summary>
        /// The MONTHSTABLE entities repository.
        /// </summary>
        IRepository<MONTHSTABLE, int> MONTHSTABLE { get; }

        /// <summary>
        /// The NARRATIVES entities repository.
        /// </summary>
        IRepository<NARRATIVES, int> NARRATIVES { get; }

        /// <summary>
        /// The OBJECT_DATA entities repository.
        /// </summary>
        IRepository<OBJECT_DATA, string> OBJECT_DATA { get; }

        /// <summary>
        /// The OBJECT_LOCK entities repository.
        /// </summary>
        IRepository<OBJECT_LOCK, int> OBJECT_LOCK { get; }

        /// <summary>
        /// The ONLINE_USERS entities repository.
        /// </summary>
        IRepository<ONLINE_USERS, Tuple<int, int>> ONLINE_USERS { get; }

        /// <summary>
        /// The OPPORTUNITY entities repository.
        /// </summary>
        IRepository<OPPORTUNITY, int> OPPORTUNITY { get; }

        /// <summary>
        /// The OPPORTUNITY_HIST entities repository.
        /// </summary>
        IRepository<OPPORTUNITY_HIST, int> OPPORTUNITY_HIST { get; }

        /// <summary>
        /// The OPPORTUNITY_LEAD entities repository.
        /// </summary>
        IRepository<OPPORTUNITY_LEAD, int> OPPORTUNITY_LEAD { get; }

        /// <summary>
        /// The OPPORTUNITY_LINES entities repository.
        /// </summary>
        IRepository<OPPORTUNITY_LINES, int> OPPORTUNITY_LINES { get; }

        /// <summary>
        /// The OPPORTUNITY_QUOTE entities repository.
        /// </summary>
        IRepository<OPPORTUNITY_QUOTE, int> OPPORTUNITY_QUOTE { get; }

        /// <summary>
        /// The OPPORTUNITY_QUOTE_OPTIONS entities repository.
        /// </summary>
        IRepository<OPPORTUNITY_QUOTE_OPTIONS, int> OPPORTUNITY_QUOTE_OPTIONS { get; }

        /// <summary>
        /// The OPPORTUNITY_STAGE entities repository.
        /// </summary>
        IRepository<OPPORTUNITY_STAGE, int> OPPORTUNITY_STAGE { get; }

        /// <summary>
        /// The OPPORTUNITY_STAGE_CONSTRAINT entities repository.
        /// </summary>
        IRepository<OPPORTUNITY_STAGE_CONSTRAINT, int> OPPORTUNITY_STAGE_CONSTRAINT { get; }

        /// <summary>
        /// The OPPORTUNITY_TYPE entities repository.
        /// </summary>
        IRepository<OPPORTUNITY_TYPE, int> OPPORTUNITY_TYPE { get; }

        /// <summary>
        /// The PAYMENT_DENOMINATIONS entities repository.
        /// </summary>
        IRepository<PAYMENT_DENOMINATIONS, int> PAYMENT_DENOMINATIONS { get; }

        /// <summary>
        /// The PAYMENT_GROUP entities repository.
        /// </summary>
        IRepository<PAYMENT_GROUP, int> PAYMENT_GROUP { get; }

        /// <summary>
        /// The PAYMENT_TYPES entities repository.
        /// </summary>
        IRepository<PAYMENT_TYPES, int> PAYMENT_TYPES { get; }

        /// <summary>
        /// The PAYROLL_COSTCENTRE entities repository.
        /// </summary>
        IRepository<PAYROLL_COSTCENTRE, Tuple<int, long>> PAYROLL_COSTCENTRE { get; }

        /// <summary>
        /// The PAYROLL_EMPLOYEES entities repository.
        /// </summary>
        IRepository<PAYROLL_EMPLOYEES, Tuple<int, string>> PAYROLL_EMPLOYEES { get; }

        /// <summary>
        /// The PAYROLL_INFO entities repository.
        /// </summary>
        IReadOnlyRepository<PAYROLL_INFO> PAYROLL_INFO { get; }

        /// <summary>
        /// The PAYROLL_PAYRATES entities repository.
        /// </summary>
        IRepository<PAYROLL_PAYRATES, Tuple<int, double>> PAYROLL_PAYRATES { get; }

        /// <summary>
        /// The PAYRUN_HDR entities repository.
        /// </summary>
        IRepository<PAYRUN_HDR, int> PAYRUN_HDR { get; }

        /// <summary>
        /// The PAYRUN_LINES entities repository.
        /// </summary>
        IRepository<PAYRUN_LINES, int> PAYRUN_LINES { get; }

        /// <summary>
        /// The PERIOD_STATUS entities repository.
        /// </summary>
        IRepository<PERIOD_STATUS, int> PERIOD_STATUS { get; }

        /// <summary>
        /// The PERIODS_DEFN entities repository.
        /// </summary>
        IRepository<PERIODS_DEFN, int> PERIODS_DEFN { get; }

        /// <summary>
        /// The PERIODS_DEFN_NEXTFINYR entities repository.
        /// </summary>
        IRepository<PERIODS_DEFN_NEXTFINYR, int> PERIODS_DEFN_NEXTFINYR { get; }

        /// <summary>
        /// The POS_COUNT entities repository.
        /// </summary>
        IRepository<POS_COUNT, Tuple<int, int>> POS_COUNT { get; }

        /// <summary>
        /// The POS_COUNT_DENOM entities repository.
        /// </summary>
        IRepository<POS_COUNT_DENOM, int> POS_COUNT_DENOM { get; }

        /// <summary>
        /// The POS_SETTLEMENT entities repository.
        /// </summary>
        IRepository<POS_SETTLEMENT, int> POS_SETTLEMENT { get; }

        /// <summary>
        /// The POS_SHIFTS entities repository.
        /// </summary>
        IRepository<POS_SHIFTS, int> POS_SHIFTS { get; }

        /// <summary>
        /// The POSTCODES entities repository.
        /// </summary>
        IRepository<POSTCODES, int> POSTCODES { get; }

        /// <summary>
        /// The PREVIOUS_ITEMS entities repository.
        /// </summary>
        IReadOnlyRepository<PREVIOUS_ITEMS> PREVIOUS_ITEMS { get; }

        /// <summary>
        /// The PRICE_NAMES entities repository.
        /// </summary>
        IRepository<PRICE_NAMES, int> PRICE_NAMES { get; }

        /// <summary>
        /// The PRICE_SCHEDULE entities repository.
        /// </summary>
        IRepository<PRICE_SCHEDULE, int> PRICE_SCHEDULE { get; }

        /// <summary>
        /// The PRINT_LOG entities repository.
        /// </summary>
        IRepository<PRINT_LOG, int> PRINT_LOG { get; }

        /// <summary>
        /// The PROFILE entities repository.
        /// </summary>
        IRepository<PROFILE, int> PROFILE { get; }

        /// <summary>
        /// The PROFILE_CONFLICT_SET entities repository.
        /// </summary>
        IRepository<PROFILE_CONFLICT_SET, int> PROFILE_CONFLICT_SET { get; }

        /// <summary>
        /// The PROFILE_CONFLICT_SET_MEMBERS entities repository.
        /// </summary>
        IRepository<PROFILE_CONFLICT_SET_MEMBERS, Tuple<int, string>> PROFILE_CONFLICT_SET_MEMBERS { get; }

        /// <summary>
        /// The PROFILE_CONSTRAINT_VALUES entities repository.
        /// </summary>
        IRepository<PROFILE_CONSTRAINT_VALUES, Tuple<int, string, string>> PROFILE_CONSTRAINT_VALUES { get; }

        /// <summary>
        /// The PROFILE_CONSTRAINTS entities repository.
        /// </summary>
        IRepository<PROFILE_CONSTRAINTS, int> PROFILE_CONSTRAINTS { get; }

        /// <summary>
        /// The PROFILE_FIELDS entities repository.
        /// </summary>
        IRepository<PROFILE_FIELDS, string> PROFILE_FIELDS { get; }

        /// <summary>
        /// The PROFILE_FORMS entities repository.
        /// </summary>
        IRepository<PROFILE_FORMS, int> PROFILE_FORMS { get; }

        /// <summary>
        /// The PROFILE_GRIDS entities repository.
        /// </summary>
        IRepository<PROFILE_GRIDS, Tuple<int, int, string>> PROFILE_GRIDS { get; }

        /// <summary>
        /// The PROFILE_TYPE_INFO entities repository.
        /// </summary>
        IRepository<PROFILE_TYPE_INFO, int> PROFILE_TYPE_INFO { get; }

        /// <summary>
        /// The PROFILE_TYPES entities repository.
        /// </summary>
        IRepository<PROFILE_TYPES, int> PROFILE_TYPES { get; }

        /// <summary>
        /// The PROFILE_VALUES entities repository.
        /// </summary>
        IRepository<PROFILE_VALUES, Tuple<int, string>> PROFILE_VALUES { get; }

        /// <summary>
        /// The PROSPECT_CONTACTS entities repository.
        /// </summary>
        IRepository<PROSPECT_CONTACTS, int> PROSPECT_CONTACTS { get; }

        /// <summary>
        /// The PROSPECT_HIST entities repository.
        /// </summary>
        IRepository<PROSPECT_HIST, int> PROSPECT_HIST { get; }

        /// <summary>
        /// The PROSPECTS entities repository.
        /// </summary>
        IRepository<PROSPECTS, int> PROSPECTS { get; }

        /// <summary>
        /// The PURCHORD_HDR entities repository.
        /// </summary>
        IRepository<PURCHORD_HDR, int> PURCHORD_HDR { get; }

        /// <summary>
        /// The PURCHORD_HDR_ARCHIVE entities repository.
        /// </summary>
        IRepository<PURCHORD_HDR_ARCHIVE, int> PURCHORD_HDR_ARCHIVE { get; }

        /// <summary>
        /// The PURCHORD_HIST entities repository.
        /// </summary>
        IRepository<PURCHORD_HIST, int> PURCHORD_HIST { get; }

        /// <summary>
        /// The PURCHORD_LINES entities repository.
        /// </summary>
        IRepository<PURCHORD_LINES, int> PURCHORD_LINES { get; }

        /// <summary>
        /// The PURCHORD_LINES_ARCHIVE entities repository.
        /// </summary>
        IRepository<PURCHORD_LINES_ARCHIVE, int> PURCHORD_LINES_ARCHIVE { get; }

        /// <summary>
        /// The QUICK_INSERT_TABSHEET entities repository.
        /// </summary>
        IRepository<QUICK_INSERT_TABSHEET, int> QUICK_INSERT_TABSHEET { get; }

        /// <summary>
        /// The REASON_CLASS entities repository.
        /// </summary>
        IRepository<REASON_CLASS, int> REASON_CLASS { get; }

        /// <summary>
        /// The REASON_EVENTS entities repository.
        /// </summary>
        IRepository<REASON_EVENTS, int> REASON_EVENTS { get; }

        /// <summary>
        /// The REASONS entities repository.
        /// </summary>
        IRepository<REASONS, int> REASONS { get; }

        /// <summary>
        /// The RELATIONSHIPS entities repository.
        /// </summary>
        IRepository<RELATIONSHIPS, int> RELATIONSHIPS { get; }

        /// <summary>
        /// The RELATIONSHIPS_DEFN entities repository.
        /// </summary>
        IRepository<RELATIONSHIPS_DEFN, int> RELATIONSHIPS_DEFN { get; }

        /// <summary>
        /// The RELATIONSHIPS_SET entities repository.
        /// </summary>
        IRepository<RELATIONSHIPS_SET, int> RELATIONSHIPS_SET { get; }

        /// <summary>
        /// The SALES_MESSAGES entities repository.
        /// </summary>
        IRepository<SALES_MESSAGES, int> SALES_MESSAGES { get; }

        /// <summary>
        /// The SALESORD_HDR entities repository.
        /// </summary>
        IRepository<SALESORD_HDR, int> SALESORD_HDR { get; }

        /// <summary>
        /// The SALESORD_HDR_ARCHIVE entities repository.
        /// </summary>
        IRepository<SALESORD_HDR_ARCHIVE, int> SALESORD_HDR_ARCHIVE { get; }

        /// <summary>
        /// The SALESORD_LINES entities repository.
        /// </summary>
        IRepository<SALESORD_LINES, int> SALESORD_LINES { get; }

        /// <summary>
        /// The SALESORD_LINES_ARCHIVE entities repository.
        /// </summary>
        IRepository<SALESORD_LINES_ARCHIVE, int> SALESORD_LINES_ARCHIVE { get; }

        /// <summary>
        /// The SALESORD_STATUS entities repository.
        /// </summary>
        IRepository<SALESORD_STATUS, int> SALESORD_STATUS { get; }

        /// <summary>
        /// The SALESORDHIST entities repository.
        /// </summary>
        IRepository<SALESORDHIST, int> SALESORDHIST { get; }

        /// <summary>
        /// The SHIPMENT_COST_DETAILS entities repository.
        /// </summary>
        IRepository<SHIPMENT_COST_DETAILS, int> SHIPMENT_COST_DETAILS { get; }

        /// <summary>
        /// The SHIPMENT_HDR entities repository.
        /// </summary>
        IRepository<SHIPMENT_HDR, int> SHIPMENT_HDR { get; }

        /// <summary>
        /// The SHIPMENT_METHODS entities repository.
        /// </summary>
        IRepository<SHIPMENT_METHODS, int> SHIPMENT_METHODS { get; }

        /// <summary>
        /// The SHIPMENT_STATUS entities repository.
        /// </summary>
        IRepository<SHIPMENT_STATUS, int> SHIPMENT_STATUS { get; }

        /// <summary>
        /// The SMTP_ACCOUNTS entities repository.
        /// </summary>
        IRepository<SMTP_ACCOUNTS, int> SMTP_ACCOUNTS { get; }

        /// <summary>
        /// The STAFF entities repository.
        /// </summary>
        IRepository<STAFF, int> STAFF { get; }

        /// <summary>
        /// The STOCK_CLASSIFICATIONS entities repository.
        /// </summary>
        IRepository<STOCK_CLASSIFICATIONS, int> STOCK_CLASSIFICATIONS { get; }

        /// <summary>
        /// The STOCK_COLOUR entities repository.
        /// </summary>
        IRepository<STOCK_COLOUR, int> STOCK_COLOUR { get; }

        /// <summary>
        /// The STOCK_CONT_HIST entities repository.
        /// </summary>
        IRepository<STOCK_CONT_HIST, int> STOCK_CONT_HIST { get; }

        /// <summary>
        /// The STOCK_GROUP2S entities repository.
        /// </summary>
        IRepository<STOCK_GROUP2S, int> STOCK_GROUP2S { get; }

        /// <summary>
        /// The STOCK_GROUPS entities repository.
        /// </summary>
        IRepository<STOCK_GROUPS, int> STOCK_GROUPS { get; }

        /// <summary>
        /// The STOCK_ITEMS entities repository.
        /// </summary>
        IRepository<STOCK_ITEMS, string> STOCK_ITEMS { get; }

        /// <summary>
        /// The STOCK_LOC_INFO entities repository.
        /// </summary>
        IRepository<STOCK_LOC_INFO, Tuple<string, int>> STOCK_LOC_INFO { get; }

        /// <summary>
        /// The STOCK_LOCATIONS entities repository.
        /// </summary>
        IRepository<STOCK_LOCATIONS, int> STOCK_LOCATIONS { get; }

        /// <summary>
        /// The STOCK_PRICEGROUPS entities repository.
        /// </summary>
        IRepository<STOCK_PRICEGROUPS, int> STOCK_PRICEGROUPS { get; }

        /// <summary>
        /// The STOCK_RECEIPTS entities repository.
        /// </summary>
        IRepository<STOCK_RECEIPTS, int> STOCK_RECEIPTS { get; }

        /// <summary>
        /// The STOCK_REQUESTLINES entities repository.
        /// </summary>
        IRepository<STOCK_REQUESTLINES, int> STOCK_REQUESTLINES { get; }

        /// <summary>
        /// The STOCK_REQUESTS entities repository.
        /// </summary>
        IRepository<STOCK_REQUESTS, int> STOCK_REQUESTS { get; }

        /// <summary>
        /// The STOCK_REQUESTTYPES entities repository.
        /// </summary>
        IRepository<STOCK_REQUESTTYPES, int> STOCK_REQUESTTYPES { get; }

        /// <summary>
        /// The STOCK_SERIALNOS entities repository.
        /// </summary>
        IRepository<STOCK_SERIALNOS, int> STOCK_SERIALNOS { get; }

        /// <summary>
        /// The STOCK_SIZE entities repository.
        /// </summary>
        IRepository<STOCK_SIZE, int> STOCK_SIZE { get; }

        /// <summary>
        /// The STOCK_TRANS entities repository.
        /// </summary>
        IRepository<STOCK_TRANS, int> STOCK_TRANS { get; }

        /// <summary>
        /// The STOCK_TRANS_ARCHIVE entities repository.
        /// </summary>
        IRepository<STOCK_TRANS_ARCHIVE, int> STOCK_TRANS_ARCHIVE { get; }

        /// <summary>
        /// The STOCK_TRANS_HDR entities repository.
        /// </summary>
        IRepository<STOCK_TRANS_HDR, int> STOCK_TRANS_HDR { get; }

        /// <summary>
        /// The STOCK_TRANS_SERIALS entities repository.
        /// </summary>
        IRepository<STOCK_TRANS_SERIALS, int> STOCK_TRANS_SERIALS { get; }

        /// <summary>
        /// The STOCK_UNITDEFINITION entities repository.
        /// </summary>
        IRepository<STOCK_UNITDEFINITION, string> STOCK_UNITDEFINITION { get; }

        /// <summary>
        /// The STOCK_WEB entities repository.
        /// </summary>
        IRepository<STOCK_WEB, string> STOCK_WEB { get; }

        /// <summary>
        /// The STOCKREQUIREMENT entities repository.
        /// </summary>
        IRepository<STOCKREQUIREMENT, string> STOCKREQUIREMENT { get; }

        /// <summary>
        /// The STOCKTAKE_CTRL entities repository.
        /// </summary>
        IRepository<STOCKTAKE_CTRL, int> STOCKTAKE_CTRL { get; }

        /// <summary>
        /// The STOCKTAKE_EVENTS entities repository.
        /// </summary>
        IRepository<STOCKTAKE_EVENTS, int> STOCKTAKE_EVENTS { get; }

        /// <summary>
        /// The STOCKTAKE_TOTALS entities repository.
        /// </summary>
        IRepository<STOCKTAKE_TOTALS, int> STOCKTAKE_TOTALS { get; }

        /// <summary>
        /// The SU_WARRANTY entities repository.
        /// </summary>
        IRepository<SU_WARRANTY, int> SU_WARRANTY { get; }

        /// <summary>
        /// The SU_WARRANTYTYPE entities repository.
        /// </summary>
        IRepository<SU_WARRANTYTYPE, int> SU_WARRANTYTYPE { get; }

        /// <summary>
        /// The SUPPLIER_STOCK_ITEMS entities repository.
        /// </summary>
        IRepository<SUPPLIER_STOCK_ITEMS, Tuple<string, string, int>> SUPPLIER_STOCK_ITEMS { get; }

        /// <summary>
        /// The TASK_STATUSES entities repository.
        /// </summary>
        IRepository<TASK_STATUSES, int> TASK_STATUSES { get; }

        /// <summary>
        /// The TASK_TYPES entities repository.
        /// </summary>
        IRepository<TASK_TYPES, int> TASK_TYPES { get; }

        /// <summary>
        /// The TASKS entities repository.
        /// </summary>
        IRepository<TASKS, int> TASKS { get; }

        /// <summary>
        /// The TAX_KEY_POINT entities repository.
        /// </summary>
        IRepository<TAX_KEY_POINT, int> TAX_KEY_POINT { get; }

        /// <summary>
        /// The TAX_RATES entities repository.
        /// </summary>
        IRepository<TAX_RATES, int> TAX_RATES { get; }

        /// <summary>
        /// The TAX_RETURN entities repository.
        /// </summary>
        IRepository<TAX_RETURN, string> TAX_RETURN { get; }

        /// <summary>
        /// The TAX_RETURN_ALLOC entities repository.
        /// </summary>
        IReadOnlyRepository<TAX_RETURN_ALLOC> TAX_RETURN_ALLOC { get; }

        /// <summary>
        /// The TAX_RETURN_CALC entities repository.
        /// </summary>
        IRepository<TAX_RETURN_CALC, Tuple<double, int>> TAX_RETURN_CALC { get; }

        /// <summary>
        /// The TAX_RETURN_POINT entities repository.
        /// </summary>
        IRepository<TAX_RETURN_POINT, Tuple<string, string>> TAX_RETURN_POINT { get; }

        /// <summary>
        /// The TAX_RETURN_TRANS entities repository.
        /// </summary>
        IRepository<TAX_RETURN_TRANS, Tuple<string, string, int, string>> TAX_RETURN_TRANS { get; }

        /// <summary>
        /// The TERRITORYBUDGET entities repository.
        /// </summary>
        IRepository<TERRITORYBUDGET, int> TERRITORYBUDGET { get; }

        /// <summary>
        /// The TERRITORYELEMENTS entities repository.
        /// </summary>
        IRepository<TERRITORYELEMENTS, int> TERRITORYELEMENTS { get; }

        /// <summary>
        /// The TERRITORYHDR entities repository.
        /// </summary>
        IRepository<TERRITORYHDR, int> TERRITORYHDR { get; }

        /// <summary>
        /// The TIME_BILLING entities repository.
        /// </summary>
        IRepository<TIME_BILLING, int> TIME_BILLING { get; }

        /// <summary>
        /// The USER_DEF_BANK_COLS entities repository.
        /// </summary>
        IRepository<USER_DEF_BANK_COLS, int> USER_DEF_BANK_COLS { get; }

        /// <summary>
        /// The VERIFICATION_CLASS entities repository.
        /// </summary>
        IRepository<VERIFICATION_CLASS, string> VERIFICATION_CLASS { get; }

        /// <summary>
        /// The VERIFICATION_HDR entities repository.
        /// </summary>
        IRepository<VERIFICATION_HDR, int> VERIFICATION_HDR { get; }

        /// <summary>
        /// The VERIFICATION_LINES entities repository.
        /// </summary>
        IRepository<VERIFICATION_LINES, int> VERIFICATION_LINES { get; }

        /// <summary>
        /// The WEEK_DATES entities repository.
        /// </summary>
        IRepository<WEEK_DATES, int> WEEK_DATES { get; }

        /// <summary>
        /// The WORKSORD_HDR entities repository.
        /// </summary>
        IRepository<WORKSORD_HDR, int> WORKSORD_HDR { get; }

        /// <summary>
        /// The WORKSORD_LINES entities repository.
        /// </summary>
        IRepository<WORKSORD_LINES, int> WORKSORD_LINES { get; }

        /// <summary>
        /// The WORKSORD_STATUS entities repository.
        /// </summary>
        IRepository<WORKSORD_STATUS, int> WORKSORD_STATUS { get; }

        /// <summary>
        /// The X_ER_REPORT entities repository.
        /// </summary>
        IRepository<X_ER_REPORT, int> X_ER_REPORT { get; }

        /// <summary>
        /// The X_ER_SETUP entities repository.
        /// </summary>
        IRepository<X_ER_SETUP, int> X_ER_SETUP { get; }

        /// <summary>
        /// The X_HBIZ_NOTIFICATIONS entities repository.
        /// </summary>
        IRepository<X_HBIZ_NOTIFICATIONS, int> X_HBIZ_NOTIFICATIONS { get; }

        /// <summary>
        /// The X_HBIZ_PURCH_REQ_COMMENTS entities repository.
        /// </summary>
        IRepository<X_HBIZ_PURCH_REQ_COMMENTS, int> X_HBIZ_PURCH_REQ_COMMENTS { get; }

        /// <summary>
        /// The X_HBIZ_PURCH_REQ_HDR entities repository.
        /// </summary>
        IRepository<X_HBIZ_PURCH_REQ_HDR, int> X_HBIZ_PURCH_REQ_HDR { get; }

        /// <summary>
        /// The X_HBIZ_PURCH_REQ_HDR_HISTORY entities repository.
        /// </summary>
        IRepository<X_HBIZ_PURCH_REQ_HDR_HISTORY, int> X_HBIZ_PURCH_REQ_HDR_HISTORY { get; }

        /// <summary>
        /// The X_HBIZ_PURCH_REQ_LINES entities repository.
        /// </summary>
        IRepository<X_HBIZ_PURCH_REQ_LINES, int> X_HBIZ_PURCH_REQ_LINES { get; }

        /// <summary>
        /// The X_HBIZ_PURCH_REQ_STATUS_VALUES entities repository.
        /// </summary>
        IRepository<X_HBIZ_PURCH_REQ_STATUS_VALUES, int> X_HBIZ_PURCH_REQ_STATUS_VALUES { get; }

        /// <summary>
        /// The X_HBIZ_PURCH_REQ_SUBSCRIPTIONS entities repository.
        /// </summary>
        IRepository<X_HBIZ_PURCH_REQ_SUBSCRIPTIONS, int> X_HBIZ_PURCH_REQ_SUBSCRIPTIONS { get; }

        /// <summary>
        /// The X_HBIZ_REPORTS_TO_STAFF entities repository.
        /// </summary>
        IRepository<X_HBIZ_REPORTS_TO_STAFF, int> X_HBIZ_REPORTS_TO_STAFF { get; }

        /// <summary>
        /// The X_PURCH_REQ_COMMENTS_FILES entities repository.
        /// </summary>
        IRepository<X_PURCH_REQ_COMMENTS_FILES, int> X_PURCH_REQ_COMMENTS_FILES { get; }
    }
}