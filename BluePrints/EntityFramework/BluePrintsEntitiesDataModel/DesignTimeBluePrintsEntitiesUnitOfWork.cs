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

        IRepository<COMMODITY_CODE, Guid> IBluePrintsEntitiesUnitOfWork.COMMODITY_CODES
        {
            get { return GetRepository((COMMODITY_CODE x) => x.GUID); }
        }

        IRepository<COMMODITY_GROUP_DIRECT, Guid> IBluePrintsEntitiesUnitOfWork.COMMODITY_GROUP_DIRECT
        {
            get { return GetRepository((COMMODITY_GROUP_DIRECT x) => x.GUID); }
        }

        IRepository<DEPARTMENT, Guid> IBluePrintsEntitiesUnitOfWork.DEPARTMENTS
        {
            get { return GetRepository((DEPARTMENT x) => x.GUID); }
        }

        IRepository<DELIVERABLES_STATUS, Guid> IBluePrintsEntitiesUnitOfWork.DELIVERABLES_STATUSES
        {
            get { return GetRepository((DELIVERABLES_STATUS x) => x.GUID); }
        }

        IRepository<DISCIPLINE, Guid> IBluePrintsEntitiesUnitOfWork.DISCIPLINES
        {
            get { return GetRepository((DISCIPLINE x) => x.GUID); }
        }

        IRepository<DOCTYPE, Guid> IBluePrintsEntitiesUnitOfWork.DOCTYPES
        {
            get { return GetRepository((DOCTYPE x) => x.GUID); }
        }

        IRepository<ESTIMATION_DIRECT, Guid> IBluePrintsEntitiesUnitOfWork.ESTIMATION_DIRECTS
        {
            get { return GetRepository((ESTIMATION_DIRECT x) => x.GUID); }
        }

        IRepository<ESTIMATION_DIRECT_ITEM, Guid> IBluePrintsEntitiesUnitOfWork.ESTIMATION_DIRECT_ITEMS
        {
            get { return GetRepository((ESTIMATION_DIRECT_ITEM x) => x.GUID); }
        }

        IRepository<ESTIMATION_INDIRECT, Guid> IBluePrintsEntitiesUnitOfWork.ESTIMATION_INDIRECTS
        {
            get { return GetRepository((ESTIMATION_INDIRECT x) => x.GUID); }
        }

        IRepository<ESTIMATION_INDIRECT_ITEM, Guid> IBluePrintsEntitiesUnitOfWork.ESTIMATION_INDIRECT_ITEMS
        {
            get { return GetRepository((ESTIMATION_INDIRECT_ITEM x) => x.GUID); }
        }

        IRepository<INDIRECT_TYPE, Guid> IBluePrintsEntitiesUnitOfWork.INDIRECT_TYPES
        {
            get { return GetRepository((INDIRECT_TYPE x) => x.GUID); }
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

        IRepository<RATE, Guid> IBluePrintsEntitiesUnitOfWork.RATES
        {
            get { return GetRepository((RATE x) => x.GUID); }
        }

        IRepository<REGISTER_CHANGE, Guid> IBluePrintsEntitiesUnitOfWork.REGISTER_CHANGE
        {
            get { return GetRepository((REGISTER_CHANGE x) => x.GUID); }
        }

        IRepository<REGISTER_HOLD, Guid> IBluePrintsEntitiesUnitOfWork.REGISTER_HOLD
        {
            get { return GetRepository((REGISTER_HOLD x) => x.GUID); }
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

        IRepository<ROLE, Guid> IBluePrintsEntitiesUnitOfWork.ROLES
        {
            get { return GetRepository((ROLE x) => x.GUID); }
        }

        IRepository<SETTINGS_GLOBAL, Guid> IBluePrintsEntitiesUnitOfWork.SETTINGS_GLOBALS
        {
            get { return GetRepository((SETTINGS_GLOBAL x) => x.GUID); }
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

        IRepository<WORKPACK_ASSIGNMENT, Guid> IBluePrintsEntitiesUnitOfWork.WORKPACK_ASSIGNMENTS
        {
            get { return GetRepository((WORKPACK_ASSIGNMENT x) => x.GUID); }
        }

        IRepository<BASELINE_ITEM_ASSIGNMENT, Guid> IBluePrintsEntitiesUnitOfWork.BASELINE_ITEM_ASSIGNMENTS
        {
            get { return GetRepository((BASELINE_ITEM_ASSIGNMENT x) => x.GUID); }
        }

        IRepository<WORKPACK, Guid> IBluePrintsEntitiesUnitOfWork.WORKPACKS
        {
            get { return GetRepository((WORKPACK x) => x.GUID); }
        }
    }
}