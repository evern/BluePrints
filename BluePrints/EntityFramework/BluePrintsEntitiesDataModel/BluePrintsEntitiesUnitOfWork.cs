using System;
using System.Linq;
using System.Data;
using System.Linq.Expressions;
using System.Collections.Generic;
using BluePrints.Common.Utils;
using BluePrints.Common.DataModel;
using BluePrints.Common.DataModel.DesignTime;
using BluePrints.Common.DataModel.EntityFramework;
using BluePrints.Data;

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

        IRepository<BASELINE, Guid> IBluePrintsEntitiesUnitOfWork.BASELINES
        {
            get { return GetRepository(x => x.Set<BASELINE>(), (BASELINE x) => x.GUID); }
        }

        IRepository<COMMODITY, Guid> IBluePrintsEntitiesUnitOfWork.COMMODITIES
        {
            get { return GetRepository(x => x.Set<COMMODITY>(), (COMMODITY x) => x.GUID); }
        }

        IRepository<COMMODITY_CODE, Guid> IBluePrintsEntitiesUnitOfWork.COMMODITY_CODES
        {
            get { return GetRepository(x => x.Set<COMMODITY_CODE>(), (COMMODITY_CODE x) => x.GUID); }
        }

        IRepository<DEPARTMENT, Guid> IBluePrintsEntitiesUnitOfWork.DEPARTMENTS
        {
            get { return GetRepository(x => x.Set<DEPARTMENT>(), (DEPARTMENT x) => x.GUID); }
        }

        IRepository<DISCIPLINE, Guid> IBluePrintsEntitiesUnitOfWork.DISCIPLINES
        {
            get { return GetRepository(x => x.Set<DISCIPLINE>(), (DISCIPLINE x) => x.GUID); }
        }

        IRepository<DOCTYPE, Guid> IBluePrintsEntitiesUnitOfWork.DOCTYPES
        {
            get { return GetRepository(x => x.Set<DOCTYPE>(), (DOCTYPE x) => x.GUID); }
        }

        IRepository<ESTIMATION_ITEM, Guid> IBluePrintsEntitiesUnitOfWork.ESTIMATION_ITEMS
        {
            get { return GetRepository(x => x.Set<ESTIMATION_ITEM>(), (ESTIMATION_ITEM x) => x.GUID); }
        }

        IRepository<ESTIMATION, Guid> IBluePrintsEntitiesUnitOfWork.ESTIMATIONS
        {
            get { return GetRepository(x => x.Set<ESTIMATION>(), (ESTIMATION x) => x.GUID); }
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

        IRepository<WORKPACK, Guid> IBluePrintsEntitiesUnitOfWork.WORKPACKS
        {
            get { return GetRepository(x => x.Set<WORKPACK>(), (WORKPACK x) => x.GUID); }
        }
    }
}
