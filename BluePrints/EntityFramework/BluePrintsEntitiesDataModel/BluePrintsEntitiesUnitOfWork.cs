using BaseModel.DataModel;
using BaseModel.DataModel.EntityFramework;
using BluePrints.Data;
using System;

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

        IRepository<STOCK_GROUP, Guid> IBluePrintsEntitiesUnitOfWork.STOCK_GROUPS
        {
            get { return GetRepository(x => x.Set<STOCK_GROUP>(), (STOCK_GROUP x) => x.GUID); }
        }

        IRepository<COMMODITY_CODE, Guid> IBluePrintsEntitiesUnitOfWork.COMMODITY_CODES
        {
            get { return GetRepository(x => x.Set<COMMODITY_CODE>(), (COMMODITY_CODE x) => x.GUID); }
        }

        IRepository<DEPARTMENT, Guid> IBluePrintsEntitiesUnitOfWork.DEPARTMENTS
        {
            get { return GetRepository(x => x.Set<DEPARTMENT>(), (DEPARTMENT x) => x.GUID); }
        }

        IRepository<DELIVERABLES_STATUS, Guid> IBluePrintsEntitiesUnitOfWork.DELIVERABLES_STATUSES
        {
            get { return GetRepository(x => x.Set<DELIVERABLES_STATUS>(), (DELIVERABLES_STATUS x) => x.GUID); }
        }

        IRepository<DISCIPLINE, Guid> IBluePrintsEntitiesUnitOfWork.DISCIPLINES
        {
            get { return GetRepository(x => x.Set<DISCIPLINE>(), (DISCIPLINE x) => x.GUID); }
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

        IRepository<ESTIMATE_ITEM, Guid> IBluePrintsEntitiesUnitOfWork.ESTIMATE_ITEMS
        {
            get { return GetRepository(x => x.Set<ESTIMATE_ITEM>(), (ESTIMATE_ITEM x) => x.GUID); }
        }

        IRepository<HOLIDAY, Guid> IBluePrintsEntitiesUnitOfWork.HOLIDAYS
        {
            get { return GetRepository(x => x.Set<HOLIDAY>(), (HOLIDAY x) => x.GUID); }
        }

        IRepository<PHASE, Guid> IBluePrintsEntitiesUnitOfWork.PHASES
        {
            get { return GetRepository(x => x.Set<PHASE>(), (PHASE x) => x.GUID); }
        }

        IRepository<PROGRESS_ITEM, Guid> IBluePrintsEntitiesUnitOfWork.PROGRESS_ITEMS
        {
            get { return GetRepository(x => x.Set<PROGRESS_ITEM>(), (PROGRESS_ITEM x) => x.GUID); }
        }

        IRepository<PROGRESS, Guid> IBluePrintsEntitiesUnitOfWork.PROGRESSES
        {
            get { return GetRepository(x => x.Set<PROGRESS>(), (PROGRESS x) => x.GUID); }
        }

        IRepository<PROJECT_REPORT, Guid> IBluePrintsEntitiesUnitOfWork.PROJECT_REPORTS
        {
            get { return GetRepository(x => x.Set<PROJECT_REPORT>(), (PROJECT_REPORT x) => x.GUID); }
        }

        IRepository<REGISTER_CHANGE, Guid> IBluePrintsEntitiesUnitOfWork.REGISTER_CHANGE
        {
            get { return GetRepository(x => x.Set<REGISTER_CHANGE>(), (REGISTER_CHANGE x) => x.GUID); }
        }

        IRepository<REGISTER_HOLD, Guid> IBluePrintsEntitiesUnitOfWork.REGISTER_HOLD
        {
            get { return GetRepository(x => x.Set<REGISTER_HOLD>(), (REGISTER_HOLD x) => x.GUID); }
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

        IRepository<REGISTER_NC, Guid> IBluePrintsEntitiesUnitOfWork.REGISTER_NC
        {
            get { return GetRepository(x => x.Set<REGISTER_NC>(), (REGISTER_NC x) => x.GUID); }
        }

        IRepository<PROJECT, Guid> IBluePrintsEntitiesUnitOfWork.PROJECTS
        {
            get { return GetRepository(x => x.Set<PROJECT>(), (PROJECT x) => x.GUID); }
        }

        IRepository<PROJECT_DISCIPLINE, Guid> IBluePrintsEntitiesUnitOfWork.PROJECT_DISCIPLINES
        {
            get { return GetRepository(x => x.Set<PROJECT_DISCIPLINE>(), (PROJECT_DISCIPLINE x) => x.GUID); }
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

        IRepository<STOCK_CODE, Guid> IBluePrintsEntitiesUnitOfWork.STOCK_CODES
        {
            get { return GetRepository(x => x.Set<STOCK_CODE>(), (STOCK_CODE x) => x.GUID); }
        }

        IRepository<UOM, Guid> IBluePrintsEntitiesUnitOfWork.UOMS
        {
            get { return GetRepository(x => x.Set<UOM>(), (UOM x) => x.GUID); }
        }

        IRepository<USER, Guid> IBluePrintsEntitiesUnitOfWork.USERS
        {
            get { return GetRepository(x => x.Set<USER>(), (USER x) => x.GUID); }
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
    }
}