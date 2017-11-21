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
        /// The STOCK_GROUP entities repository.
        /// </summary>
        IRepository<STOCK_GROUP, Guid> STOCK_GROUPS { get; }

        /// <summary>
        /// The DEPARTMENT entities repository.
        /// </summary>
        IRepository<DEPARTMENT, Guid> DEPARTMENTS { get; }

        /// <summary>
        /// The DataPoint entities repository.
        /// </summary>
        IRepository<DataPoint, Guid> DataPoints { get; }

        /// <summary>
        /// The DELIVERABLES_STATUS entities repository.
        /// </summary>
        IRepository<DELIVERABLES_STATUS, Guid> DELIVERABLES_STATUSES { get; }

        /// <summary>
        /// The DISCIPLINE entities repository.
        /// </summary>
        IRepository<DISCIPLINE, Guid> DISCIPLINES { get; }

        /// <summary>
        /// The DOCTYPE entities repository.
        /// </summary>
        IRepository<DOCTYPE, Guid> DOCTYPES { get; }

        /// <summary>
        /// The ESTIMATION_DIRECT entities repository.
        /// </summary>
        IRepository<ESTIMATION_DIRECT, Guid> ESTIMATION_DIRECTS { get; }

        /// <summary>
        /// The ESTIMATION_DIRECT_ITEM entities repository.
        /// </summary>
        IRepository<ESTIMATION_DIRECT_ITEM, Guid> ESTIMATION_DIRECT_ITEMS { get; }

        /// <summary>
        /// The HOLIDAY entities repository.
        /// </summary>
        IRepository<HOLIDAY, Guid> HOLIDAYS { get; }

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
        /// The PROJECT_REPORTS entities repository.
        /// </summary>
        IRepository<PROJECT_REPORT, Guid> PROJECT_REPORTS { get; }

        /// <summary>
        /// The PROJECT entities repository.
        /// </summary>
        IRepository<PROJECT, Guid> PROJECTS { get; }

        /// <summary>
        /// The RATE entities repository.
        /// </summary>
        IRepository<RATE, Guid> RATES { get; }

        /// <summary>
        /// The REGISTER_CHANGE entities repository.
        /// </summary>
        IRepository<REGISTER_CHANGE, Guid> REGISTER_CHANGE { get; }

        /// <summary>
        /// The REGISTER_HOLD entities repository.
        /// </summary>
        IRepository<REGISTER_HOLD, Guid> REGISTER_HOLD { get; }

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
        /// The SETTINGS_GLOBALS entities repository.
        /// </summary>
        IRepository<SETTINGS_GLOBAL, Guid> SETTINGS_GLOBALS { get; }

        /// <summary>
        /// The STOCK_CODES entities repository.
        /// </summary>
        IRepository<STOCK_CODE, Guid> STOCK_CODES { get; }

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