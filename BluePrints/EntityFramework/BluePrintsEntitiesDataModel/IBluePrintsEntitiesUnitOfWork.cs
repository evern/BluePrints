using BluePrints.Common.DataModel;
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
        /// The COMMODITY_CODES entities repository.
        /// </summary>
        IRepository<COMMODITY_CODE, Guid> COMMODITY_CODES { get; }

        /// <summary>
        /// The COMMODITY_GROUP_DIRECT entities repository.
        /// </summary>
        IRepository<COMMODITY_GROUP_DIRECT, Guid> COMMODITY_GROUP_DIRECT { get; }

        /// <summary>
        /// The DEPARTMENT entities repository.
        /// </summary>
        IRepository<DEPARTMENT, Guid> DEPARTMENTS { get; }

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
        /// The ESTIMATION_INDIRECT entities repository.
        /// </summary>
        IRepository<ESTIMATION_INDIRECT, Guid> ESTIMATION_INDIRECTS { get; }

        /// <summary>
        /// The ESTIMATION_INDIRECT_ITEM entities repository.
        /// </summary>
        IRepository<ESTIMATION_INDIRECT_ITEM, Guid> ESTIMATION_INDIRECT_ITEMS { get; }

        /// <summary>
        /// The INDIRECT_TYPE entities repository.
        /// </summary>
        IRepository<INDIRECT_TYPE, Guid> INDIRECT_TYPES { get; }

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
        /// The WORKPACK_ASSIGNMENTS entities repository.
        /// </summary>
        IRepository<WORKPACK_ASSIGNMENT, Guid> WORKPACK_ASSIGNMENTS { get; }

        /// <summary>
        /// The BASELINE_ITEM_ASSIGNMENTS entities repository.
        /// </summary>
        IRepository<BASELINE_ITEM_ASSIGNMENT, Guid> BASELINE_ITEM_ASSIGNMENTS { get; }

        /// <summary>
        /// The WORKPACK entities repository.
        /// </summary>
        IRepository<WORKPACK, Guid> WORKPACKS { get; }
    }
}