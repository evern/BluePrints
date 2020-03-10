using BaseModel.DataModel;
using BluePrints.Data;
using System;

namespace BluePrints.BluePrintsEntitiesDataModel
{
    /// <summary>
    /// IBluePrintsEntitiesUnitOfWork extends the IUnitOfWork interface with repositories representing specific entities.
    /// </summary>
    public interface IBluePrintsEntitiesUnitOfWork : IUnitOfWork
    {
        /// <summary>
        /// The AREA entities repository.
        /// </summary>
        IRepository<AREA, Guid> AREAS { get; }

        /// <summary>
        /// The BASELINE_ITEMS entities repository.
        /// </summary>
        IRepository<BASELINE_ITEM, Guid> BASELINE_ITEMS { get; }
        
        /// <summary>
        /// The BASELINE_ITEM_WORK entities repository.
        /// </summary>
        IRepository<BASELINE_ITEM_WORK, Guid> BASELINE_ITEM_WORKS { get; }

        /// <summary>
        /// The BASELINE entities repository.
        /// </summary>
        IRepository<BASELINE, Guid> BASELINES { get; }

        /// <summary>
        /// The CONSTRUCTION_CONFIG entities repository.
        /// </summary>
        IRepository<CONSTRUCTION_CONFIG, Guid> CONSTRUCTION_CONFIGS { get; }

        /// <summary>
        /// The STOCK_GROUP entities repository.
        /// </summary>
        IRepository<STOCK_GROUP, Guid> STOCK_GROUPS { get; }

        /// <summary>
        /// The DEPARTMENT entities repository.
        /// </summary>
        IRepository<DEPARTMENT, Guid> DEPARTMENTS { get; }

        /// <summary>
        /// The DAYWORK entities repository.
        /// </summary>
        IRepository<DAYWORK, Guid> DAYWORKS { get; }

        /// <summary>
        /// The DAYWORK_EQUIPMENT entities repository.
        /// </summary>
        IRepository<DAYWORK_EQUIPMENT, Guid> DAYWORK_EQUIPMENTS { get; }

        /// <summary>
        /// The DAYWORK_MATERIAL entities repository.
        /// </summary>
        IRepository<DAYWORK_MATERIAL, Guid> DAYWORK_MATERIALS { get; }

        /// <summary>
        /// The DAYWORK_LABOUR entities repository.
        /// </summary>
        IRepository<DAYWORK_LABOUR, Guid> DAYWORK_LABOURS { get; }

        /// <summary>
        /// The DAYWORK_STAFF_ROLE entities repository.
        /// </summary>
        IRepository<DAYWORK_STAFF_ROLE, Guid> DAYWORK_STAFF_ROLES { get; }

        /// <summary>
        /// The DataPoint entities repository.
        /// </summary>
        IRepository<DataPoint, Guid> DataPoints { get; }

        /// <summary>
        /// The DELIVERABLES_STATUS entities repository.
        /// </summary>
        IRepository<DELIVERABLES_STATUS, Guid> DELIVERABLES_STATUSES { get; }

        /// <summary>
        /// The DSTATUS_DOCTYPE entities repository.
        /// </summary>
        IRepository<DSTATUS_DOCTYPE, Guid> DSTATUS_DOCTYPES { get; }

        /// <summary>
        /// The DISCIPLINE entities repository.
        /// </summary>
        IRepository<DISCIPLINE, Guid> DISCIPLINES { get; }

        /// <summary>
        /// The DOCTYPE entities repository.
        /// </summary>
        IRepository<DOCTYPE, Guid> DOCTYPES { get; }

        /// <summary>
        /// The ESTIMATE entities repository.
        /// </summary>
        IRepository<ESTIMATE, Guid> ESTIMATES { get; }

        /// <summary>
        /// The FORECAST entities repository.
        /// </summary>
        IRepository<FORECAST, Guid> FORECASTS { get; }

        /// <summary>
        /// The FORECAST_EAC entities repository.
        /// </summary>
        IRepository<FORECAST_EAC, Guid> FORECAST_EACS { get; }

        /// <summary>
        /// The FORECAST_JOB entities repository.
        /// </summary>
        IRepository<FORECAST_JOB, Guid> FORECAST_JOBS { get; }

        /// <summary>
        /// The FORECAST_JOB_SETTING entities repository.
        /// </summary>
        IRepository<FORECAST_JOB_SETTING, Guid> FORECAST_JOB_SETTINGS { get; }

        /// <summary>
        /// The FORECAST_JOB_HOUR entities repository.
        /// </summary>
        IRepository<FORECAST_JOB_HOUR, Guid> FORECAST_JOB_HOURS { get; }

        /// <summary>
        /// The FORECAST_PO entities repository.
        /// </summary>
        IRepository<FORECAST_PO, Guid> FORECAST_POS { get; }

        /// <summary>
        /// The FORECAST_PO_SETTING entities repository.
        /// </summary>
        IRepository<FORECAST_PO_SETTING, Guid> FORECAST_PO_SETTINGS { get; }

        /// <summary>
        /// The ESTIMATE_ITEM entities repository.
        /// </summary>
        IRepository<ESTIMATE_ITEM, Guid> ESTIMATE_ITEMS { get; }

        /// <summary>
        /// The HOLIDAY entities repository.
        /// </summary>
        IRepository<HOLIDAY, Guid> HOLIDAYS { get; }

        /// <summary>
        /// The HSE entities repository.
        /// </summary>
        IRepository<HSE, Guid> HSES { get; }

        /// <summary>
        /// The HSE_INCIDENT entities repository.
        /// </summary>
        IRepository<HSE_INCIDENT, Guid> HSE_INCIDENTS { get; }

        /// <summary>
        /// The HSE_INURY entities repository.
        /// </summary>
        IRepository<HSE_INJURY, Guid> HSE_INJURIES { get; }

        /// <summary>
        /// The OFFICE entities repository.
        /// </summary>
        IRepository<OFFICE, Guid> OFFICES { get; }

        /// <summary>
        /// The PHASE entities repository.
        /// </summary>
        IRepository<PHASE, Guid> PHASES { get; }

        /// <summary>
        /// The PROGRESS_ITEMS entities repository.
        /// </summary>
        IRepository<PROGRESS_ITEM, Guid> PROGRESS_ITEMS { get; }

        /// <summary>
        /// The PROGRESS entities repository.
        /// </summary>
        IRepository<PROGRESS, Guid> PROGRESSES { get; }

        /// <summary>
        /// The PROJECT_PERMISSION entities repository.
        /// </summary>
        IRepository<PROJECT_PERMISSION, Guid> PROJECT_PERMISSIONS { get; }

        /// <summary>
        /// The PROJECT_REPORTS entities repository.
        /// </summary>
        IRepository<PROJECT_REPORT, Guid> PROJECT_REPORTS { get; }

        /// <summary>
        /// The PROJECT_REVENUE entities repository.
        /// </summary>
        IRepository<PROJECT_REVENUE, Guid> PROJECT_REVENUES { get; }

        /// <summary>
        /// The PROJECT entities repository.
        /// </summary>
        IRepository<PROJECT, Guid> PROJECTS { get; }

        /// <summary>
        /// The PROJECT_SUMMARY entities repository.
        /// </summary>
        IRepository<PROJECT_SUMMARY, Guid> PROJECT_SUMMARIES { get; }

        /// <summary>
        /// The PROJECT_SUMMARY_SETTING entities repository.
        /// </summary>
        IRepository<PROJECT_SUMMARY_SETTING, Guid> PROJECT_SUMMARY_SETTINGS { get; }

        /// <summary>
        /// The PROJECT_DISCIPLINE entities repository.
        /// </summary>
        IRepository<PROJECT_DISCIPLINE, Guid> PROJECT_DISCIPLINES { get; }

        /// <summary>
        /// The RA_STUDY_TYPE entities repository.
        /// </summary>
        IRepository<RA_STUDY_TYPE, Guid> RA_STUDY_TYPES { get; }

        /// <summary>
        /// The RA_GUIDE_PROMPT entities repository.
        /// </summary>
        IRepository<RA_GUIDE_PROMPT, Guid> RA_GUIDE_PROMPTS { get; }

        /// <summary>
        /// The RA_GUIDE_SUBPROMPT entities repository.
        /// </summary>
        IRepository<RA_GUIDE_SUBPROMPT, Guid> RA_GUIDE_SUBPROMPTS { get; }

        /// <summary>
        /// The RA_STUDY entities repository.
        /// </summary>
        IRepository<RA_STUDY, Guid> RA_STUDIES { get; }

        /// <summary>
        /// The RA_STUDY_DATA entities repository.
        /// </summary>
        IRepository<RA_STUDY_DATA, Guid> RA_STUDY_DATAS { get; }

        /// <summary>
        /// The RA_STUDY_DRAWING entities repository.
        /// </summary>
        IRepository<RA_STUDY_DRAWING, Guid> RA_STUDY_DRAWINGS { get; }

        /// <summary>
        /// The RA_STUDY_TEAM entities repository.
        /// </summary>
        IRepository<RA_STUDY_TEAM, Guid> RA_STUDY_TEAMS { get; }

        /// <summary>
        /// The RA_STUDY_NODE entities repository.
        /// </summary>
        IRepository<RA_STUDY_NODE, Guid> RA_STUDY_NODES { get; }

        /// <summary>
        /// The RATE entities repository.
        /// </summary>
        IRepository<RATE, Guid> RATES { get; }

        /// <summary>
        /// The ROLE_COMMODITIES entities repository.
        /// </summary>
        IRepository<ROLE_COMMODITY, Guid> ROLE_COMMODITIES { get; }

        /// <summary>
        /// The REGISTER_CHANGE entities repository.
        /// </summary>
        IRepository<REGISTER_CHANGE, Guid> REGISTER_CHANGE { get; }

        /// <summary>
        /// The REGISTER_HOLD entities repository.
        /// </summary>
        IRepository<REGISTER_HOLD, Guid> REGISTER_HOLD { get; }

        /// <summary>
        /// The REGISTER_HOLD_REF entities repository.
        /// </summary>
        IRepository<REGISTER_HOLD_REF, Guid> REGISTER_HOLD_REF { get; }

        /// <summary>
        /// The REGISTER_ISSUE entities repository.
        /// </summary>
        IRepository<REGISTER_ISSUE, Guid> REGISTER_ISSUE { get; }

        /// <summary>
        /// The REGISTER_RISK entities repository.
        /// </summary>
        IRepository<REGISTER_RISK, Guid> REGISTER_RISK { get; }

        /// <summary>
        /// The REGISTER_LL entities repository.
        /// </summary>
        IRepository<REGISTER_LL, Guid> REGISTER_LL { get; }

        /// <summary>
        /// The REGISTER_NC entities repository.
        /// </summary>
        IRepository<REGISTER_NC, Guid> REGISTER_NC { get; }

        /// <summary>
        /// The REGISTER entities repository.
        /// </summary>
        IRepository<REGISTER, Guid> REGISTERS { get; }

        /// <summary>
        /// The ROLE_PERMISSIONS entities repository.
        /// </summary>
        IRepository<ROLE_PERMISSION, Guid> ROLE_PERMISSIONS { get; }

        /// <summary>
        /// The ROLE entities repository.
        /// </summary>
        IRepository<ROLE, Guid> ROLES { get; }

        /// <summary>
        /// The ROSTER_STAFF entities repository.
        /// </summary>
        IRepository<ROSTER_STAFF, Guid> ROSTER_STAFFS { get; }

        /// <summary>
        /// The ROSTER_STAFF_STATUS entities repository.
        /// </summary>
        IRepository<ROSTER_STAFF_STATUS, Guid> ROSTER_STAFF_STATUSES { get; }

        /// <summary>
        /// The SETTINGS_GLOBALS entities repository.
        /// </summary>
        IRepository<SETTINGS_GLOBAL, Guid> SETTINGS_GLOBALS { get; }

        /// <summary>
        /// The STOCK_CODES entities repository.
        /// </summary>
        IRepository<STOCK_CODE, Guid> STOCK_CODES { get; }

        /// <summary>
        /// The TENDER_PROFILE entities repository.
        /// </summary>
        IRepository<TENDER_PROFILE, Guid> TENDER_PROFILES { get; }

        /// <summary>
        /// The TENDER_PROFILE_ITEM entities repository.
        /// </summary>
        IRepository<TENDER_PROFILE_ITEM, Guid> TENDER_PROFILE_ITEMS { get; }

        /// <summary>
        /// The COMMODITY_CODES entities repository.
        /// </summary>
        IRepository<COMMODITY_CODE, Guid> COMMODITY_CODES { get; }

        /// <summary>
        /// The UOM entities repository.
        /// </summary>
        IRepository<UOM, Guid> UOMS { get; }

        /// <summary>
        /// The USER entities repository.
        /// </summary>
        IRepository<USER, Guid> USERS { get; }

        /// <summary>
        /// The VARIATION_ITEMS entities repository.
        /// </summary>
        IRepository<VARIATION_ITEM, Guid> VARIATION_ITEMS { get; }

        /// <summary>
        /// The VARIATION_REGISTER entities repository.
        /// </summary>
        IRepository<VARIATION_REGISTER, Guid> VARIATION_REGISTERS { get; }

        /// <summary>
        /// The VARIATION entities repository.
        /// </summary>
        IRepository<VARIATION, Guid> VARIATIONS { get; }

        /// <summary>
        /// The SUBJOB_ASSIGNMENTS entities repository.
        /// </summary>
        IRepository<SUBJOB_ASSIGNMENT, Guid> SUBJOB_ASSIGNMENTS { get; }

        /// <summary>
        /// The BASELINE_ITEM_ASSIGNMENTS entities repository.
        /// </summary>
        IRepository<P6_ASSIGNMENT, Guid> P6_ASSIGNMENTS { get; }

        /// <summary>
        /// The SUBJOB entities repository.
        /// </summary>
        IRepository<SUBJOB, Guid> SUBJOBS { get; }

        /// <summary>
        /// The CLIENT entities repository.
        /// </summary>
        IRepository<CLIENT, Guid> CLIENTS { get; }

        /// <summary>
        /// The MEETINGS entities repository.
        /// </summary>
        IRepository<MEETING, Guid> MEETINGS { get; }

        /// <summary>
        /// The MEETING_TYPES entities repository.
        /// </summary>
        IRepository<MEETING_TYPE, Guid> MEETING_TYPES { get; }

        /// <summary>
        /// MEETING_ACTIONS entities repository.
        /// </summary>
        IRepository<MEETING_ACTION, Guid> MEETING_ACTIONS { get; }

        /// <summary>
        /// The MEETING_USERS entities repository.
        /// </summary>
        IRepository<MEETING_USER, Guid> MEETING_USERS { get; }

        /// <summary>
        /// The MINUTE_AGENDAS entities repository.
        /// </summary>
        IRepository<MINUTE_AGENDA, Guid> MINUTE_AGENDAS { get; }

        /// <summary>
        /// MINUTE_TITLES entities repository.
        /// </summary>
        IRepository<MINUTE_TITLE, Guid> MINUTE_TITLES { get; }

        /// <summary>
        /// CLIENT_PROJECT entities repository.
        /// </summary>
        IRepository<CLIENT_PROJECT, Guid> CLIENT_PROJECTS { get; }

        /// <summary>
        /// The WORKPACK entities repository.
        /// </summary>
        IRepository<WORKPACK, Guid> WORKPACKS { get; }
    }
}