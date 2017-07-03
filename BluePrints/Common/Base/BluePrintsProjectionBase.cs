using BaseModel.Misc;
using BluePrints.Common.Projections;
using BluePrints.Data;
using System;
using System.Linq;
using System.Collections.Generic;
using BluePrints.Common.ViewModel.Reporting;

namespace BluePrints.Common.Base
{
    /// <summary>
    /// Provides mapping back to key naming convention for projection
    /// Projection key property name must be the same with entity key name for repository results to map back to projections
    /// </summary>
    /// <typeparam name="TEntity">Entity with Guid typed key</typeparam>
    public abstract class BluePrintsProjectionBase<TEntity> : ProjectionBase<TEntity>
        where TEntity : class, IGuidEntityKey, new()
    {
        public Guid GUID
        {
            get { return base.EntityKey; }
            set { base.EntityKey = value; }
        }
    }
}
