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

        IRepository<ESTIMATION_DIRECT, Guid> IBluePrintsEntitiesUnitOfWork.ESTIMATION_DIRECTS
        {
            get { return GetRepository(x => x.Set<ESTIMATION_DIRECT>(), (ESTIMATION_DIRECT x) => x.GUID); }
        }

        IRepository<ESTIMATION_DIRECT_ITEM, Guid> IBluePrintsEntitiesUnitOfWork.ESTIMATION_DIRECT_ITEMS
        {
            get { return GetRepository(x => x.Set<ESTIMATION_DIRECT_ITEM>(), (ESTIMATION_DIRECT_ITEM x) => x.GUID); }
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

        IRepository<RATE, Guid> IBluePrintsEntitiesUnitOfWork.RATES
        {
            get { return GetRepository(x => x.Set<RATE>(), (RATE x) => x.GUID); }
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

        IRepository<WORKPACK_ASSIGNMENT, Guid> IBluePrintsEntitiesUnitOfWork.WORKPACK_ASSIGNMENTS
        {
            get { return GetRepository(x => x.Set<WORKPACK_ASSIGNMENT>(), (WORKPACK_ASSIGNMENT x) => x.GUID); }
        }

        IRepository<P6_ASSIGNMENT, Guid> IBluePrintsEntitiesUnitOfWork.P6_ASSIGNMENTS
        {
            get { return GetRepository(x => x.Set<P6_ASSIGNMENT>(), (P6_ASSIGNMENT x) => x.GUID); }
        }

        IRepository<WORKPACK, Guid> IBluePrintsEntitiesUnitOfWork.WORKPACKS
        {
            get { return GetRepository(x => x.Set<WORKPACK>(), (WORKPACK x) => x.GUID); }
        }
    }
}