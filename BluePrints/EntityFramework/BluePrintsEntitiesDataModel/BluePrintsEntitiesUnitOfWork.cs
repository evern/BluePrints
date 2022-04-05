using BaseModel.DataModel;
using BaseModel.DataModel.EntityFramework;
using BluePrints.Data;
using System;
using System.Data.Entity;

namespace BluePrints.BluePrintsEntitiesDataModel
{
    /// <summary>
    /// A BluePrintsEntitiesUnitOfWork instance that represents the run-time implementation of the IBluePrintsEntitiesUnitOfWork interface.
    /// </summary>
    public class BluePrintsEntitiesUnitOfWork : DbUnitOfWork<BluePrintsEntities>, IBluePrintsEntitiesUnitOfWork
    {
        public BluePrintsEntitiesUnitOfWork(Func<BluePrintsEntities> contextFactory)
            : base(contextFactory)
        {
        }

        IRepository<AREA, Guid> IBluePrintsEntitiesUnitOfWork.AREAS
        {
            get { return GetRepository(x => x.Set<AREA>(), (AREA x) => x.GUID); }
        }

        IRepository<BASELINE_ITEM, Guid> IBluePrintsEntitiesUnitOfWork.BASELINE_ITEMS
        {
            get { return GetRepository(x => x.Set<BASELINE_ITEM>(), (BASELINE_ITEM x) => x.GUID); }
        }

        IRepository<BASELINE_ITEM_WORK, Guid> IBluePrintsEntitiesUnitOfWork.BASELINE_ITEM_WORKS
        {
            get { return GetRepository(x => x.Set<BASELINE_ITEM_WORK>(), (BASELINE_ITEM_WORK x) => x.GUID); }
        }

        IRepository<BASELINE, Guid> IBluePrintsEntitiesUnitOfWork.BASELINES
        {
            get { return GetRepository(x => x.Set<BASELINE>(), (BASELINE x) => x.GUID); }
        }

        IRepository<CONSTRUCTION_CONFIG, Guid> IBluePrintsEntitiesUnitOfWork.CONSTRUCTION_CONFIGS
        {
            get { return GetRepository(x => x.Set<CONSTRUCTION_CONFIG>(), (CONSTRUCTION_CONFIG x) => x.GUID); }
        }

        IRepository<CONSTRUCTION_STAGE, Guid> IBluePrintsEntitiesUnitOfWork.CONSTRUCTION_STAGES
        {
            get { return GetRepository(x => x.Set<CONSTRUCTION_STAGE>(), (CONSTRUCTION_STAGE x) => x.GUID); }
        }

        IRepository<COMMODITY_CODE, Guid> IBluePrintsEntitiesUnitOfWork.COMMODITY_CODES
        {
            get { return GetRepository(x => x.Set<COMMODITY_CODE>(), (COMMODITY_CODE x) => x.GUID); }
        }

        IRepository<DEPARTMENT, Guid> IBluePrintsEntitiesUnitOfWork.DEPARTMENTS
        {
            get { return GetRepository(x => x.Set<DEPARTMENT>(), (DEPARTMENT x) => x.GUID); }
        }

        IRepository<DAYWORK, Guid> IBluePrintsEntitiesUnitOfWork.DAYWORKS
        {
            get { return GetRepository(x => x.Set<DAYWORK>(), (DAYWORK x) => x.GUID); }
        }

        IRepository<DAYWORK_EQUIPMENT, Guid> IBluePrintsEntitiesUnitOfWork.DAYWORK_EQUIPMENTS
        {
            get { return GetRepository(x => x.Set<DAYWORK_EQUIPMENT>(), (DAYWORK_EQUIPMENT x) => x.GUID); }
        }

        IRepository<DAYWORK_MATERIAL, Guid> IBluePrintsEntitiesUnitOfWork.DAYWORK_MATERIALS
        {
            get { return GetRepository(x => x.Set<DAYWORK_MATERIAL>(), (DAYWORK_MATERIAL x) => x.GUID); }
        }

        IRepository<DAYWORK_LABOUR, Guid> IBluePrintsEntitiesUnitOfWork.DAYWORK_LABOURS
        {
            get { return GetRepository(x => x.Set<DAYWORK_LABOUR>(), (DAYWORK_LABOUR x) => x.GUID); }
        }

        IRepository<DAYWORK_STAFF_ROLE, Guid> IBluePrintsEntitiesUnitOfWork.DAYWORK_STAFF_ROLES
        {
            get { return GetRepository(x => x.Set<DAYWORK_STAFF_ROLE>(), (DAYWORK_STAFF_ROLE x) => x.GUID); }
        }

        IRepository<DELIVERABLES_STATUS, Guid> IBluePrintsEntitiesUnitOfWork.DELIVERABLES_STATUSES
        {
            get { return GetRepository(x => x.Set<DELIVERABLES_STATUS>(), (DELIVERABLES_STATUS x) => x.GUID); }
        }

        IRepository<DSTATUS_DOCTYPE, Guid> IBluePrintsEntitiesUnitOfWork.DSTATUS_DOCTYPES
        {
            get { return GetRepository(x => x.Set<DSTATUS_DOCTYPE>(), (DSTATUS_DOCTYPE x) => x.GUID); }
        }

        IRepository<DISCIPLINE, Guid> IBluePrintsEntitiesUnitOfWork.DISCIPLINES
        {
            get { return GetRepository(x => x.Set<DISCIPLINE>(), (DISCIPLINE x) => x.GUID); }
        }

        IRepository<DISCIPLINE_DESC, Guid> IBluePrintsEntitiesUnitOfWork.DISCIPLINE_DESCS
        {
            get { return GetRepository(x => x.Set<DISCIPLINE_DESC>(), (DISCIPLINE_DESC x) => x.GUID); }
        }

        IRepository<DOCTYPE, Guid> IBluePrintsEntitiesUnitOfWork.DOCTYPES
        {
            get { return GetRepository(x => x.Set<DOCTYPE>(), (DOCTYPE x) => x.GUID); }
        }

        IRepository<DataPoint, Guid> IBluePrintsEntitiesUnitOfWork.DataPoints
        {
            get { return GetRepository(x => x.Set<DataPoint>(), (DataPoint x) => x.Guid_DataPoint); }
        }

        IRepository<ESTIMATE, Guid> IBluePrintsEntitiesUnitOfWork.ESTIMATES
        {
            get { return GetRepository(x => x.Set<ESTIMATE>(), (ESTIMATE x) => x.GUID); }
        }

        IRepository<FORECAST, Guid> IBluePrintsEntitiesUnitOfWork.FORECASTS
        {
            get { return GetRepository(x => x.Set<FORECAST>(), (FORECAST x) => x.GUID); }
        }

        IRepository<FORECAST_COMMENT, Guid> IBluePrintsEntitiesUnitOfWork.FORECAST_COMMENTS
        {
            get { return GetRepository(x => x.Set<FORECAST_COMMENT>(), (FORECAST_COMMENT x) => x.GUID); }
        }

        IRepository<FORECAST_EAC, Guid> IBluePrintsEntitiesUnitOfWork.FORECAST_EACS
        {
            get { return GetRepository(x => x.Set<FORECAST_EAC>(), (FORECAST_EAC x) => x.GUID); }
        }

        IRepository<FORECAST_HISTORY, Guid> IBluePrintsEntitiesUnitOfWork.FORECAST_HISTORIES
        {
            get { return GetRepository(x => x.Set<FORECAST_HISTORY>(), (FORECAST_HISTORY x) => x.GUID); }
        }

        IRepository<FORECAST_JOB, Guid> IBluePrintsEntitiesUnitOfWork.FORECAST_JOBS
        {
            get { return GetRepository(x => x.Set<FORECAST_JOB>(), (FORECAST_JOB x) => x.GUID); }
        }

        IRepository<FORECAST_JOB_SETTING, Guid> IBluePrintsEntitiesUnitOfWork.FORECAST_JOB_SETTINGS
        {
            get { return GetRepository(x => x.Set<FORECAST_JOB_SETTING>(), (FORECAST_JOB_SETTING x) => x.GUID); }
        }

        IRepository<FORECAST_JOB_HOUR, Guid> IBluePrintsEntitiesUnitOfWork.FORECAST_JOB_HOURS
        {
            get { return GetRepository(x => x.Set<FORECAST_JOB_HOUR>(), (FORECAST_JOB_HOUR x) => x.GUID); }
        }

        IRepository<FORECAST_PO, Guid> IBluePrintsEntitiesUnitOfWork.FORECAST_POS
        {
            get { return GetRepository(x => x.Set<FORECAST_PO>(), (FORECAST_PO x) => x.GUID); }
        }

        IRepository<FORECAST_PO_SETTING, Guid> IBluePrintsEntitiesUnitOfWork.FORECAST_PO_SETTINGS
        {
            get { return GetRepository(x => x.Set<FORECAST_PO_SETTING>(), (FORECAST_PO_SETTING x) => x.GUID); }
        }

        public IRepository<FORECAST_JOB_HOUR_SNAPSHOT, Guid> FORECAST_JOB_HOUR_SNAPSHOTS
        {
            get { return GetRepository(x => x.Set<FORECAST_JOB_HOUR_SNAPSHOT>(), (FORECAST_JOB_HOUR_SNAPSHOT x) => x.GUID); }
        }

        IRepository<ESTIMATE_ITEM, Guid> IBluePrintsEntitiesUnitOfWork.ESTIMATE_ITEMS
        {
            get { return GetRepository(x => x.Set<ESTIMATE_ITEM>(), (ESTIMATE_ITEM x) => x.GUID); }
        }

        IRepository<HOLIDAY, Guid> IBluePrintsEntitiesUnitOfWork.HOLIDAYS
        {
            get { return GetRepository(x => x.Set<HOLIDAY>(), (HOLIDAY x) => x.GUID); }
        }

        IRepository<HSE, Guid> IBluePrintsEntitiesUnitOfWork.HSES
        {
            get { return GetRepository(x => x.Set<HSE>(), (HSE x) => x.GUID); }
        }

        IRepository<HSE_INCIDENT, Guid> IBluePrintsEntitiesUnitOfWork.HSE_INCIDENTS
        {
            get { return GetRepository(x => x.Set<HSE_INCIDENT>(), (HSE_INCIDENT x) => x.GUID); }
        }

        IRepository<HSE_INJURY, Guid> IBluePrintsEntitiesUnitOfWork.HSE_INJURIES
        {
            get { return GetRepository(x => x.Set<HSE_INJURY>(), (HSE_INJURY x) => x.GUID); }
        }

        IRepository<JOBCOST_LINES_AUDIT, Guid> IBluePrintsEntitiesUnitOfWork.JOBCOST_LINES_AUDITS
        {
            get { return GetRepository(x => x.Set<JOBCOST_LINES_AUDIT>(), (JOBCOST_LINES_AUDIT x) => x.GUID); }
        }

        IRepository<OFFICE, Guid> IBluePrintsEntitiesUnitOfWork.OFFICES
        {
            get { return GetRepository(x => x.Set<OFFICE>(), (OFFICE x) => x.GUID); }
        }

        IRepository<PHASE, Guid> IBluePrintsEntitiesUnitOfWork.PHASES
        {
            get { return GetRepository(x => x.Set<PHASE>(), (PHASE x) => x.GUID); }
        }

        IRepository<PROGRESS_ITEM, Guid> IBluePrintsEntitiesUnitOfWork.PROGRESS_ITEMS
        {
            get { return GetRepository(x => x.Set<PROGRESS_ITEM>(), (PROGRESS_ITEM x) => x.GUID); }
        }

        IRepository<PROGRESS_ETC, Guid> IBluePrintsEntitiesUnitOfWork.PROGRESS_ETCS
        {
            get { return GetRepository(x => x.Set<PROGRESS_ETC>(), (PROGRESS_ETC x) => x.GUID); }
        }

        IRepository<PROGRESS, Guid> IBluePrintsEntitiesUnitOfWork.PROGRESSES
        {
            get { return GetRepository(x => x.Set<PROGRESS>(), (PROGRESS x) => x.GUID); }
        }

        IRepository<PROJECT_REPORT, Guid> IBluePrintsEntitiesUnitOfWork.PROJECT_REPORTS
        {
            get { return GetRepository(x => x.Set<PROJECT_REPORT>(), (PROJECT_REPORT x) => x.GUID); }
        }

        IRepository<PROJECT_CONTRACTOR, Guid> IBluePrintsEntitiesUnitOfWork.PROJECT_CONTRACTORS
        {
            get { return GetRepository(x => x.Set<PROJECT_CONTRACTOR>(), (PROJECT_CONTRACTOR x) => x.GUID); }
        }

        IRepository<REGISTER_CHANGE, Guid> IBluePrintsEntitiesUnitOfWork.REGISTER_CHANGE
        {
            get { return GetRepository(x => x.Set<REGISTER_CHANGE>(), (REGISTER_CHANGE x) => x.GUID); }
        }

        IRepository<REGISTER_CHANGE_ATTACHMENT, Guid> IBluePrintsEntitiesUnitOfWork.REGISTER_CHANGE_ATTACHMENTS
        {
            get { return GetRepository(x => x.Set<REGISTER_CHANGE_ATTACHMENT>(), (REGISTER_CHANGE_ATTACHMENT x) => x.GUID); }
        }

        IRepository<REGISTER_CLARIFICATION, Guid> IBluePrintsEntitiesUnitOfWork.REGISTER_CLARIFICATIONS
        {
            get { return GetRepository(x => x.Set<REGISTER_CLARIFICATION>(), (REGISTER_CLARIFICATION x) => x.GUID); }
        }

        IRepository<REGISTER_TQ, Guid> IBluePrintsEntitiesUnitOfWork.REGISTER_TQ
        {
            get { return GetRepository(x => x.Set<REGISTER_TQ>(), (REGISTER_TQ x) => x.GUID); }
        }

        IRepository<REGISTER_TQ_ATTACHMENT, Guid> IBluePrintsEntitiesUnitOfWork.REGISTER_TQ_ATTACHMENTS
        {
            get { return GetRepository(x => x.Set<REGISTER_TQ_ATTACHMENT>(), (REGISTER_TQ_ATTACHMENT x) => x.GUID); }
        }

        IRepository<REGISTER_HOLD, Guid> IBluePrintsEntitiesUnitOfWork.REGISTER_HOLD
        {
            get { return GetRepository(x => x.Set<REGISTER_HOLD>(), (REGISTER_HOLD x) => x.GUID); }
        }

        IRepository<REGISTER_HOLD_REF, Guid> IBluePrintsEntitiesUnitOfWork.REGISTER_HOLD_REF
        {
            get { return GetRepository(x => x.Set<REGISTER_HOLD_REF>(), (REGISTER_HOLD_REF x) => x.GUID); }
        }

        IRepository<REGISTER_ISSUE, Guid> IBluePrintsEntitiesUnitOfWork.REGISTER_ISSUE
        {
            get { return GetRepository(x => x.Set<REGISTER_ISSUE>(), (REGISTER_ISSUE x) => x.GUID); }
        }

        IRepository<REGISTER_RISK, Guid> IBluePrintsEntitiesUnitOfWork.REGISTER_RISK
        {
            get { return GetRepository(x => x.Set<REGISTER_RISK>(), (REGISTER_RISK x) => x.GUID); }
        }

        IRepository<REGISTER_LL, Guid> IBluePrintsEntitiesUnitOfWork.REGISTER_LL
        {
            get { return GetRepository(x => x.Set<REGISTER_LL>(), (REGISTER_LL x) => x.GUID); }
        }

        IRepository<ROSTER_STAFF, Guid> IBluePrintsEntitiesUnitOfWork.ROSTER_STAFFS
        {
            get { return GetRepository(x => x.Set<ROSTER_STAFF>(), (ROSTER_STAFF x) => x.GUID); }
        }

        IRepository<ROSTER_STAFF_STATUS, Guid> IBluePrintsEntitiesUnitOfWork.ROSTER_STAFF_STATUSES
        {
            get { return GetRepository(x => x.Set<ROSTER_STAFF_STATUS>(), (ROSTER_STAFF_STATUS x) => x.GUID); }
        }

        IRepository<REGISTER_NC, Guid> IBluePrintsEntitiesUnitOfWork.REGISTER_NC
        {
            get { return GetRepository(x => x.Set<REGISTER_NC>(), (REGISTER_NC x) => x.GUID); }
        }

        IRepository<PROJECT, Guid> IBluePrintsEntitiesUnitOfWork.PROJECTS
        {
            get { return GetRepository(x => x.Set<PROJECT>(), (PROJECT x) => x.GUID); }
        }

        IRepository<PROJECT_REVENUE, Guid> IBluePrintsEntitiesUnitOfWork.PROJECT_REVENUES
        {
            get { return GetRepository(x => x.Set<PROJECT_REVENUE>(), (PROJECT_REVENUE x) => x.GUID); }
        }

        IRepository<PROJECT_SUMMARY, Guid> IBluePrintsEntitiesUnitOfWork.PROJECT_SUMMARIES
        {
            get { return GetRepository(x => x.Set<PROJECT_SUMMARY>(), (PROJECT_SUMMARY x) => x.GUID); }
        }

        IRepository<PROJECT_SUMMARY_SETTING, Guid> IBluePrintsEntitiesUnitOfWork.PROJECT_SUMMARY_SETTINGS
        {
            get { return GetRepository(x => x.Set<PROJECT_SUMMARY_SETTING>(), (PROJECT_SUMMARY_SETTING x) => x.GUID); }
        }

        IRepository<PROJECT_DISCIPLINE, Guid> IBluePrintsEntitiesUnitOfWork.PROJECT_DISCIPLINES
        {
            get { return GetRepository(x => x.Set<PROJECT_DISCIPLINE>(), (PROJECT_DISCIPLINE x) => x.GUID); }
        }

        IRepository<PROJECT_PERMISSION, Guid> IBluePrintsEntitiesUnitOfWork.PROJECT_PERMISSIONS
        {
            get { return GetRepository(x => x.Set<PROJECT_PERMISSION>(), (PROJECT_PERMISSION x) => x.GUID); }
        }

        IRepository<RATE, Guid> IBluePrintsEntitiesUnitOfWork.RATES
        {
            get { return GetRepository(x => x.Set<RATE>(), (RATE x) => x.GUID); }
        }

        IRepository<RA_STUDY, Guid> IBluePrintsEntitiesUnitOfWork.RA_STUDIES
        {
            get { return GetRepository(x => x.Set<RA_STUDY>(), (RA_STUDY x) => x.GUID); }
        }

        IRepository<RA_STUDY_DATA, Guid> IBluePrintsEntitiesUnitOfWork.RA_STUDY_DATAS
        {
            get { return GetRepository(x => x.Set<RA_STUDY_DATA>(), (RA_STUDY_DATA x) => x.GUID); }
        }

        IRepository<RA_STUDY_DRAWING, Guid> IBluePrintsEntitiesUnitOfWork.RA_STUDY_DRAWINGS
        {
            get { return GetRepository(x => x.Set<RA_STUDY_DRAWING>(), (RA_STUDY_DRAWING x) => x.GUID); }
        }

        IRepository<RA_STUDY_TEAM, Guid> IBluePrintsEntitiesUnitOfWork.RA_STUDY_TEAMS
        {
            get { return GetRepository(x => x.Set<RA_STUDY_TEAM>(), (RA_STUDY_TEAM x) => x.GUID); }
        }

        IRepository<RA_STUDY_NODE, Guid> IBluePrintsEntitiesUnitOfWork.RA_STUDY_NODES
        {
            get { return GetRepository(x => x.Set<RA_STUDY_NODE>(), (RA_STUDY_NODE x) => x.GUID); }
        }

        IRepository<RA_STUDY_TYPE, Guid> IBluePrintsEntitiesUnitOfWork.RA_STUDY_TYPES
        {
            get { return GetRepository(x => x.Set<RA_STUDY_TYPE>(), (RA_STUDY_TYPE x) => x.GUID); }
        }
        
        IRepository<RA_GUIDE_PROMPT, Guid> IBluePrintsEntitiesUnitOfWork.RA_GUIDE_PROMPTS
        {
            get { return GetRepository(x => x.Set<RA_GUIDE_PROMPT>(), (RA_GUIDE_PROMPT x) => x.GUID); }
        }

        IRepository<RA_GUIDE_SUBPROMPT, Guid> IBluePrintsEntitiesUnitOfWork.RA_GUIDE_SUBPROMPTS
        {
            get { return GetRepository(x => x.Set<RA_GUIDE_SUBPROMPT>(), (RA_GUIDE_SUBPROMPT x) => x.GUID); }
        }

        IRepository<ROLE_COMMODITY, Guid> IBluePrintsEntitiesUnitOfWork.ROLE_COMMODITIES
        {
            get { return GetRepository(x => x.Set<ROLE_COMMODITY>(), (ROLE_COMMODITY x) => x.GUID); }
        }

        IRepository<REGISTER, Guid> IBluePrintsEntitiesUnitOfWork.REGISTERS
        {
            get { return GetRepository(x => x.Set<REGISTER>(), (REGISTER x) => x.GUID); }
        }

        IRepository<ROLE_PERMISSION, Guid> IBluePrintsEntitiesUnitOfWork.ROLE_PERMISSIONS
        {
            get { return GetRepository(x => x.Set<ROLE_PERMISSION>(), (ROLE_PERMISSION x) => x.GUID); }
        }

        IRepository<ROLE, Guid> IBluePrintsEntitiesUnitOfWork.ROLES
        {
            get { return GetRepository(x => x.Set<ROLE>(), (ROLE x) => x.GUID); }
        }

        IRepository<SETTINGS_GLOBAL, Guid> IBluePrintsEntitiesUnitOfWork.SETTINGS_GLOBALS
        {
            get { return GetRepository(x => x.Set<SETTINGS_GLOBAL>(), (SETTINGS_GLOBAL x) => x.GUID); }
        }

        IRepository<TENDER_PROFILE, Guid> IBluePrintsEntitiesUnitOfWork.TENDER_PROFILES
        {
            get { return GetRepository(x => x.Set<TENDER_PROFILE>(), (TENDER_PROFILE x) => x.GUID); }
        }

        IRepository<TENDER_PROFILE_ITEM, Guid> IBluePrintsEntitiesUnitOfWork.TENDER_PROFILE_ITEMS
        {
            get { return GetRepository(x => x.Set<TENDER_PROFILE_ITEM>(), (TENDER_PROFILE_ITEM x) => x.GUID); }
        }

        IRepository<UOM, Guid> IBluePrintsEntitiesUnitOfWork.UOMS
        {
            get { return GetRepository(x => x.Set<UOM>(), (UOM x) => x.GUID); }
        }

        IRepository<USER, Guid> IBluePrintsEntitiesUnitOfWork.USERS
        {
            get { return GetRepository(x => x.Set<USER>(), (USER x) => x.GUID); }
        }

        IRepository<USER_PINNED_PROJECT, Guid> IBluePrintsEntitiesUnitOfWork.USER_PINNED_PROJECTS
        {
            get { return GetRepository(x => x.Set<USER_PINNED_PROJECT>(), (USER_PINNED_PROJECT x) => x.GUID); }
        }

        IRepository<USER_PREFERENCE, Guid> IBluePrintsEntitiesUnitOfWork.USER_PREFERENCES
        {
            get { return GetRepository(x => x.Set<USER_PREFERENCE>(), (USER_PREFERENCE x) => x.GUID); }
        }

        IRepository<VARIATION_ITEM, Guid> IBluePrintsEntitiesUnitOfWork.VARIATION_ITEMS
        {
            get { return GetRepository(x => x.Set<VARIATION_ITEM>(), (VARIATION_ITEM x) => x.GUID); }
        }

        IRepository<VARIATION, Guid> IBluePrintsEntitiesUnitOfWork.VARIATIONS
        {
            get { return GetRepository(x => x.Set<VARIATION>(), (VARIATION x) => x.GUID); }
        }

        IRepository<SUBJOB_ASSIGNMENT, Guid> IBluePrintsEntitiesUnitOfWork.SUBJOB_ASSIGNMENTS
        {
            get { return GetRepository(x => x.Set<SUBJOB_ASSIGNMENT>(), (SUBJOB_ASSIGNMENT x) => x.GUID); }
        }

        IRepository<P6_ASSIGNMENT, Guid> IBluePrintsEntitiesUnitOfWork.P6_ASSIGNMENTS
        {
            get { return GetRepository(x => x.Set<P6_ASSIGNMENT>(), (P6_ASSIGNMENT x) => x.GUID); }
        }

        IRepository<SUBJOB, Guid> IBluePrintsEntitiesUnitOfWork.SUBJOBS
        {
            get { return GetRepository(x => x.Set<SUBJOB>(), (SUBJOB x) => x.GUID); }
        }

        IRepository<TRANSACTION_APPROVAL, Guid> IBluePrintsEntitiesUnitOfWork.TRANSACTION_APPROVALS
        {
            get { return GetRepository(x => x.Set<TRANSACTION_APPROVAL>(), (TRANSACTION_APPROVAL x) => x.GUID); }
        }

        IRepository<CLIENT, Guid> IBluePrintsEntitiesUnitOfWork.CLIENTS
        {
            get { return GetRepository(x => x.Set<CLIENT>(), (CLIENT x) => x.GUID); }
        }

        IRepository<MEETING, Guid> IBluePrintsEntitiesUnitOfWork.MEETINGS
        {
            get { return GetRepository(x => x.Set<MEETING>(), (MEETING x) => x.GUID); }
        }

        IRepository<MEETING_ACTION, Guid> IBluePrintsEntitiesUnitOfWork.MEETING_ACTIONS
        {
            get { return GetRepository(x => x.Set<MEETING_ACTION>(), (MEETING_ACTION x) => x.GUID); }
        }

        IRepository<MEETING_TYPE, Guid> IBluePrintsEntitiesUnitOfWork.MEETING_TYPES
        {
            get { return GetRepository(x => x.Set<MEETING_TYPE>(), (MEETING_TYPE x) => x.GUID); }
        }

        IRepository<MEETING_USER, Guid> IBluePrintsEntitiesUnitOfWork.MEETING_USERS
        {
            get { return GetRepository(x => x.Set<MEETING_USER>(), (MEETING_USER x) => x.GUID); }
        }

        IRepository<MINUTE_AGENDA, Guid> IBluePrintsEntitiesUnitOfWork.MINUTE_AGENDAS
        {
            get { return GetRepository(x => x.Set<MINUTE_AGENDA>(), (MINUTE_AGENDA x) => x.GUID); }
        }

        IRepository<MINUTE_TITLE, Guid> IBluePrintsEntitiesUnitOfWork.MINUTE_TITLES
        {
            get { return GetRepository(x => x.Set<MINUTE_TITLE>(), (MINUTE_TITLE x) => x.GUID); }
        }

        IRepository<CLIENT_PROJECT, Guid> IBluePrintsEntitiesUnitOfWork.CLIENT_PROJECTS
        {
            get { return GetRepository(x => x.Set<CLIENT_PROJECT>(), (CLIENT_PROJECT x) => x.GUID); }
        }

        IRepository<WORKPACK, Guid> IBluePrintsEntitiesUnitOfWork.WORKPACKS
        {
            get { return GetRepository(x => x.Set<WORKPACK>(), (WORKPACK x) => x.GUID); }
        }

        IRepository<VARIATION_CONSTRUCTION, Guid> IBluePrintsEntitiesUnitOfWork.VARIATION_CONSTRUCTIONS
        {
            get { return GetRepository(x => x.Set<VARIATION_CONSTRUCTION>(), (VARIATION_CONSTRUCTION x) => x.GUID); }
        }

        IRepository<VARIATION_CONSTRUCTION_ITEM, Guid> IBluePrintsEntitiesUnitOfWork.VARIATION_CONSTRUCTION_ITEMS
        {
            get { return GetRepository(x => x.Set<VARIATION_CONSTRUCTION_ITEM>(), (VARIATION_CONSTRUCTION_ITEM x) => x.GUID); }
        }

        IRepository<VARIATION_CONSTRUCTION_IMPACT, Guid> IBluePrintsEntitiesUnitOfWork.VARIATION_CONSTRUCTION_IMPACTS
        {
            get { return GetRepository(x => x.Set<VARIATION_CONSTRUCTION_IMPACT>(), (VARIATION_CONSTRUCTION_IMPACT x) => x.GUID); }
        }

        IRepository<X_VARIATION_QUERY, Guid> IBluePrintsEntitiesUnitOfWork.X_VARIATION_QUERY
        {
            get { return GetRepository(x => x.Set<X_VARIATION_QUERY>(), (X_VARIATION_QUERY x) => x.GUID); }
        }
            
        IRepository<X_JOBCOST_LINES_AUDIT, Guid> IBluePrintsEntitiesUnitOfWork.X_JOBCOST_LINES_AUDIT
        {
            get { return GetRepository(x => x.Set<X_JOBCOST_LINES_AUDIT>(), (X_JOBCOST_LINES_AUDIT x) => x.GUID); }
        }

        IRepository<TRANSACTION_AUDIT, Guid> IBluePrintsEntitiesUnitOfWork.TRANSACTION_AUDITS
        {
            get { return GetRepository(x => x.Set<TRANSACTION_AUDIT>(), (TRANSACTION_AUDIT x) => x.GUID); }
        }
    }
}