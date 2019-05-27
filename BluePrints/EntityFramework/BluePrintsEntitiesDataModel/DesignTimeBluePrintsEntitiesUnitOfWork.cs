using BaseModel.DataModel;
using BaseModel.DataModel.DesignTime;
using BluePrints.Data;
using System;

namespace BluePrints.BluePrintsEntitiesDataModel
{
    /// <summary>
    /// A BluePrintsEntitiesDesignTimeUnitOfWork instance that represents the design-time implementation of the IBluePrintsEntitiesUnitOfWork interface.
    /// </summary>
    public class BluePrintsEntitiesDesignTimeUnitOfWork : DesignTimeUnitOfWork, IBluePrintsEntitiesUnitOfWork
    {
        /// <summary>
        /// Initializes a new instance of the BluePrintsEntitiesDesignTimeUnitOfWork class.
        /// </summary>
        public BluePrintsEntitiesDesignTimeUnitOfWork()
        {
        }

        IRepository<AREA, Guid> IBluePrintsEntitiesUnitOfWork.AREAS
        {
            get { return GetRepository((AREA x) => x.GUID); }
        }

        IRepository<BASELINE_ITEM, Guid> IBluePrintsEntitiesUnitOfWork.BASELINE_ITEMS
        {
            get { return GetRepository((BASELINE_ITEM x) => x.GUID); }
        }

        IRepository<BASELINE_ITEM_WORK, Guid> IBluePrintsEntitiesUnitOfWork.BASELINE_ITEM_WORKS
        {
            get { return GetRepository((BASELINE_ITEM_WORK x) => x.GUID); }
        }

        IRepository<BASELINE, Guid> IBluePrintsEntitiesUnitOfWork.BASELINES
        {
            get { return GetRepository((BASELINE x) => x.GUID); }
        }

        IRepository<CONSTRUCTION_CONFIG, Guid> IBluePrintsEntitiesUnitOfWork.CONSTRUCTION_CONFIGS
        {
            get { return GetRepository((CONSTRUCTION_CONFIG x) => x.GUID); }
        }

        IRepository<PO_CUSTOMDATE, Guid> IBluePrintsEntitiesUnitOfWork.PO_CUSTOMDATES
        {
            get { return GetRepository((PO_CUSTOMDATE x) => x.GUID); }
        }

        IRepository<STOCK_GROUP, Guid> IBluePrintsEntitiesUnitOfWork.STOCK_GROUPS
        {
            get { return GetRepository((STOCK_GROUP x) => x.GUID); }
        }

        IRepository<COMMODITY_CODE, Guid> IBluePrintsEntitiesUnitOfWork.COMMODITY_CODES
        {
            get { return GetRepository((COMMODITY_CODE x) => x.GUID); }
        }

        IRepository<DEPARTMENT, Guid> IBluePrintsEntitiesUnitOfWork.DEPARTMENTS
        {
            get { return GetRepository((DEPARTMENT x) => x.GUID); }
        }

        IRepository<DELIVERABLES_STATUS, Guid> IBluePrintsEntitiesUnitOfWork.DELIVERABLES_STATUSES
        {
            get { return GetRepository((DELIVERABLES_STATUS x) => x.GUID); }
        }

        IRepository<DSTATUS_DOCTYPE, Guid> IBluePrintsEntitiesUnitOfWork.DSTATUS_DOCTYPES
        {
            get { return GetRepository((DSTATUS_DOCTYPE x) => x.GUID); }
        }

        IRepository<DAYWORK, Guid> IBluePrintsEntitiesUnitOfWork.DAYWORKS
        {
            get { return GetRepository((DAYWORK x) => x.GUID); }
        }

        IRepository<DAYWORK_EQUIPMENT, Guid> IBluePrintsEntitiesUnitOfWork.DAYWORK_EQUIPMENTS
        {
            get { return GetRepository((DAYWORK_EQUIPMENT x) => x.GUID); }
        }

        IRepository<DAYWORK_MATERIAL, Guid> IBluePrintsEntitiesUnitOfWork.DAYWORK_MATERIALS
        {
            get { return GetRepository((DAYWORK_MATERIAL x) => x.GUID); }
        }

        IRepository<DAYWORK_LABOUR, Guid> IBluePrintsEntitiesUnitOfWork.DAYWORK_LABOURS
        {
            get { return GetRepository((DAYWORK_LABOUR x) => x.GUID); }
        }

        IRepository<DAYWORK_STAFF_ROLE, Guid> IBluePrintsEntitiesUnitOfWork.DAYWORK_STAFF_ROLES
        {
            get { return GetRepository((DAYWORK_STAFF_ROLE x) => x.GUID); }
        }

        IRepository<DISCIPLINE, Guid> IBluePrintsEntitiesUnitOfWork.DISCIPLINES
        {
            get { return GetRepository((DISCIPLINE x) => x.GUID); }
        }

        IRepository<DOCTYPE, Guid> IBluePrintsEntitiesUnitOfWork.DOCTYPES
        {
            get { return GetRepository((DOCTYPE x) => x.GUID); }
        }

        IRepository<DataPoint, Guid> IBluePrintsEntitiesUnitOfWork.DataPoints
        {
            get { return GetRepository((DataPoint x) => x.Guid_DataPoint); }
        }

        IRepository<ESTIMATE, Guid> IBluePrintsEntitiesUnitOfWork.ESTIMATES
        {
            get { return GetRepository((ESTIMATE x) => x.GUID); }
        }

        IRepository<FORECAST, Guid> IBluePrintsEntitiesUnitOfWork.FORECASTS
        {
            get { return GetRepository((FORECAST x) => x.GUID); }
        }

        IRepository<FORECAST_PO, Guid> IBluePrintsEntitiesUnitOfWork.FORECAST_POS
        {
            get { return GetRepository((FORECAST_PO x) => x.GUID); }
        }

        IRepository<FORECAST_PO_RESULT, Guid> IBluePrintsEntitiesUnitOfWork.FORECAST_PO_RESULTS
        {
            get { return GetRepository((FORECAST_PO_RESULT x) => x.GUID); }
        }

        IRepository<ESTIMATE_ITEM, Guid> IBluePrintsEntitiesUnitOfWork.ESTIMATE_ITEMS
        {
            get { return GetRepository((ESTIMATE_ITEM x) => x.GUID); }
        }

        IRepository<HOLIDAY, Guid> IBluePrintsEntitiesUnitOfWork.HOLIDAYS
        {
            get { return GetRepository((HOLIDAY x) => x.GUID); }
        }

        IRepository<HSE, Guid> IBluePrintsEntitiesUnitOfWork.HSES
        {
            get { return GetRepository((HSE x) => x.GUID); }
        }

        IRepository<HSE_INCIDENT, Guid> IBluePrintsEntitiesUnitOfWork.HSE_INCIDENTS
        {
            get { return GetRepository((HSE_INCIDENT x) => x.GUID); }
        }

        IRepository<HSE_INJURY, Guid> IBluePrintsEntitiesUnitOfWork.HSE_INJURIES
        {
            get { return GetRepository((HSE_INJURY x) => x.GUID); }
        }

        IRepository<OFFICE, Guid> IBluePrintsEntitiesUnitOfWork.OFFICES
        {
            get { return GetRepository((OFFICE x) => x.GUID); }
        }

        IRepository<PHASE, Guid> IBluePrintsEntitiesUnitOfWork.PHASES
        {
            get { return GetRepository((PHASE x) => x.GUID); }
        }

        IRepository<PROGRESS_ITEM, Guid> IBluePrintsEntitiesUnitOfWork.PROGRESS_ITEMS
        {
            get { return GetRepository((PROGRESS_ITEM x) => x.GUID); }
        }

        IRepository<PROGRESS, Guid> IBluePrintsEntitiesUnitOfWork.PROGRESSES
        {
            get { return GetRepository((PROGRESS x) => x.GUID); }
        }

        IRepository<PROJECT_REPORT, Guid> IBluePrintsEntitiesUnitOfWork.PROJECT_REPORTS
        {
            get { return GetRepository((PROJECT_REPORT x) => x.GUID); }
        }

        IRepository<PROJECT, Guid> IBluePrintsEntitiesUnitOfWork.PROJECTS
        {
            get { return GetRepository((PROJECT x) => x.GUID); }
        }

        IRepository<PROJECT_SUMMARY, Guid> IBluePrintsEntitiesUnitOfWork.PROJECT_SUMMARIES
        {
            get { return GetRepository((PROJECT_SUMMARY x) => x.GUID); }
        }

        IRepository<PROJECT_SUMMARY_SETTING, Guid> IBluePrintsEntitiesUnitOfWork.PROJECT_SUMMARY_SETTINGS
        {
            get { return GetRepository((PROJECT_SUMMARY_SETTING x) => x.GUID); }
        }

        IRepository<PROJECT_DISCIPLINE, Guid> IBluePrintsEntitiesUnitOfWork.PROJECT_DISCIPLINES
        {
            get { return GetRepository((PROJECT_DISCIPLINE x) => x.GUID); }
        }

        IRepository<RATE, Guid> IBluePrintsEntitiesUnitOfWork.RATES
        {
            get { return GetRepository((RATE x) => x.GUID); }
        }

        IRepository<RA_STUDY, Guid> IBluePrintsEntitiesUnitOfWork.RA_STUDIES
        {
            get { return GetRepository((RA_STUDY x) => x.GUID); }
        }

        IRepository<RA_STUDY_DATA, Guid> IBluePrintsEntitiesUnitOfWork.RA_STUDY_DATAS
        {
            get { return GetRepository((RA_STUDY_DATA x) => x.GUID); }
        }
        
        IRepository<RA_STUDY_DRAWING, Guid> IBluePrintsEntitiesUnitOfWork.RA_STUDY_DRAWINGS
        {
            get { return GetRepository((RA_STUDY_DRAWING x) => x.GUID); }
        }

        IRepository<RA_STUDY_NODE, Guid> IBluePrintsEntitiesUnitOfWork.RA_STUDY_NODES
        {
            get { return GetRepository((RA_STUDY_NODE x) => x.GUID); }
        }

        IRepository<RA_STUDY_TEAM, Guid> IBluePrintsEntitiesUnitOfWork.RA_STUDY_TEAMS
        {
            get { return GetRepository((RA_STUDY_TEAM x) => x.GUID); }
        }

        IRepository<RA_STUDY_TYPE, Guid> IBluePrintsEntitiesUnitOfWork.RA_STUDY_TYPES
        {
            get { return GetRepository((RA_STUDY_TYPE x) => x.GUID); }
        }

        IRepository<RA_GUIDE_PROMPT, Guid> IBluePrintsEntitiesUnitOfWork.RA_GUIDE_PROMPTS
        {
            get { return GetRepository((RA_GUIDE_PROMPT x) => x.GUID); }
        }

        IRepository<RA_GUIDE_SUBPROMPT, Guid> IBluePrintsEntitiesUnitOfWork.RA_GUIDE_SUBPROMPTS
        {
            get { return GetRepository((RA_GUIDE_SUBPROMPT x) => x.GUID); }
        }

        IRepository<ROLE_COMMODITY, Guid> IBluePrintsEntitiesUnitOfWork.ROLE_COMMODITIES
        {
            get { return GetRepository((ROLE_COMMODITY x) => x.GUID); }
        }

        IRepository<REGISTER_CHANGE, Guid> IBluePrintsEntitiesUnitOfWork.REGISTER_CHANGE
        {
            get { return GetRepository((REGISTER_CHANGE x) => x.GUID); }
        }

        IRepository<REGISTER_HOLD, Guid> IBluePrintsEntitiesUnitOfWork.REGISTER_HOLD
        {
            get { return GetRepository((REGISTER_HOLD x) => x.GUID); }
        }

        IRepository<REGISTER_HOLD_REF, Guid> IBluePrintsEntitiesUnitOfWork.REGISTER_HOLD_REF
        {
            get { return GetRepository((REGISTER_HOLD_REF x) => x.GUID); }
        }

        IRepository<REGISTER_ISSUE, Guid> IBluePrintsEntitiesUnitOfWork.REGISTER_ISSUE
        {
            get { return GetRepository((REGISTER_ISSUE x) => x.GUID); }
        }

        IRepository<REGISTER_RISK, Guid> IBluePrintsEntitiesUnitOfWork.REGISTER_RISK
        {
            get { return GetRepository((REGISTER_RISK x) => x.GUID); }
        }

        IRepository<REGISTER_LL, Guid> IBluePrintsEntitiesUnitOfWork.REGISTER_LL
        {
            get { return GetRepository((REGISTER_LL x) => x.GUID); }
        }

        IRepository<REGISTER_NC, Guid> IBluePrintsEntitiesUnitOfWork.REGISTER_NC
        {
            get { return GetRepository((REGISTER_NC x) => x.GUID); }
        }

        IRepository<REGISTER, Guid> IBluePrintsEntitiesUnitOfWork.REGISTERS
        {
            get { return GetRepository((REGISTER x) => x.GUID); }
        }

        IRepository<ROLE_PERMISSION, Guid> IBluePrintsEntitiesUnitOfWork.ROLE_PERMISSIONS
        {
            get { return GetRepository((ROLE_PERMISSION x) => x.GUID); }
        }

        IRepository<ROSTER_STAFF, Guid> IBluePrintsEntitiesUnitOfWork.ROSTER_STAFFS
        {
            get { return GetRepository((ROSTER_STAFF x) => x.GUID); }
        }

        IRepository<ROSTER_STAFF_STATUS, Guid> IBluePrintsEntitiesUnitOfWork.ROSTER_STAFF_STATUSES
        {
            get { return GetRepository((ROSTER_STAFF_STATUS x) => x.GUID); }
        }

        IRepository<ROLE, Guid> IBluePrintsEntitiesUnitOfWork.ROLES
        {
            get { return GetRepository((ROLE x) => x.GUID); }
        }

        IRepository<SETTINGS_GLOBAL, Guid> IBluePrintsEntitiesUnitOfWork.SETTINGS_GLOBALS
        {
            get { return GetRepository((SETTINGS_GLOBAL x) => x.GUID); }
        }

        IRepository<STOCK_CODE, Guid> IBluePrintsEntitiesUnitOfWork.STOCK_CODES
        {
            get { return GetRepository((STOCK_CODE x) => x.GUID); }
        }

        IRepository<TENDER_PROFILE, Guid> IBluePrintsEntitiesUnitOfWork.TENDER_PROFILES
        {
            get { return GetRepository((TENDER_PROFILE x) => x.GUID); }
        }

        IRepository<TENDER_PROFILE_ITEM, Guid> IBluePrintsEntitiesUnitOfWork.TENDER_PROFILE_ITEMS
        {
            get { return GetRepository((TENDER_PROFILE_ITEM x) => x.GUID); }
        }

        IRepository<UOM, Guid> IBluePrintsEntitiesUnitOfWork.UOMS
        {
            get { return GetRepository((UOM x) => x.GUID); }
        }

        IRepository<USER, Guid> IBluePrintsEntitiesUnitOfWork.USERS
        {
            get { return GetRepository((USER x) => x.GUID); }
        }

        IRepository<VARIATION_ITEM, Guid> IBluePrintsEntitiesUnitOfWork.VARIATION_ITEMS
        {
            get { return GetRepository((VARIATION_ITEM x) => x.GUID); }
        }

        IRepository<VARIATION, Guid> IBluePrintsEntitiesUnitOfWork.VARIATIONS
        {
            get { return GetRepository((VARIATION x) => x.GUID); }
        }

        IRepository<SUBJOB_ASSIGNMENT, Guid> IBluePrintsEntitiesUnitOfWork.SUBJOB_ASSIGNMENTS
        {
            get { return GetRepository((SUBJOB_ASSIGNMENT x) => x.GUID); }
        }

        IRepository<P6_ASSIGNMENT, Guid> IBluePrintsEntitiesUnitOfWork.P6_ASSIGNMENTS
        {
            get { return GetRepository((P6_ASSIGNMENT x) => x.GUID); }
        }

        IRepository<SUBJOB, Guid> IBluePrintsEntitiesUnitOfWork.SUBJOBS
        {
            get { return GetRepository((SUBJOB x) => x.GUID); }
        }

        IRepository<CLIENT, Guid> IBluePrintsEntitiesUnitOfWork.CLIENTS
        {
            get { return GetRepository((CLIENT x) => x.GUID); }
        }

        IRepository<MEETING, Guid> IBluePrintsEntitiesUnitOfWork.MEETINGS
        {
            get { return GetRepository((MEETING x) => x.GUID); }
        }

        IRepository<MEETING_ACTION, Guid> IBluePrintsEntitiesUnitOfWork.MEETING_ACTIONS
        {
            get { return GetRepository((MEETING_ACTION x) => x.GUID); }
        }

        IRepository<MEETING_TYPE, Guid> IBluePrintsEntitiesUnitOfWork.MEETING_TYPES
        {
            get { return GetRepository((MEETING_TYPE x) => x.GUID); }
        }

        IRepository<MEETING_USER, Guid> IBluePrintsEntitiesUnitOfWork.MEETING_USERS
        {
            get { return GetRepository((MEETING_USER x) => x.GUID); }
        }

        IRepository<MINUTE_AGENDA, Guid> IBluePrintsEntitiesUnitOfWork.MINUTE_AGENDAS
        {
            get { return GetRepository((MINUTE_AGENDA x) => x.GUID); }
        }

        IRepository<MINUTE_TITLE, Guid> IBluePrintsEntitiesUnitOfWork.MINUTE_TITLES
        {
            get { return GetRepository((MINUTE_TITLE x) => x.GUID); }
        }

        IRepository<CLIENT_PROJECT, Guid> IBluePrintsEntitiesUnitOfWork.CLIENT_PROJECTS
        {
            get { return GetRepository((CLIENT_PROJECT x) => x.GUID); }
        }

        IRepository<WORKPACK, Guid> IBluePrintsEntitiesUnitOfWork.WORKPACKS
        {
            get { return GetRepository((WORKPACK x) => x.GUID); }
        }

        IRepository<VARIATION_REGISTER, Guid> IBluePrintsEntitiesUnitOfWork.VARIATION_REGISTERS
        {
            get { return GetRepository((VARIATION_REGISTER x) => x.GUID); }
        }
    }
}