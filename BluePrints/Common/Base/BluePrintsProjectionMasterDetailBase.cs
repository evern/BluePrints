using BaseModel.Misc;
using System;

namespace BluePrints.Common.Base
{
    /// <summary>
    /// Provides mapping back to key naming convention for projection
    /// Projection key property name must be the same with entity key name for repository results to map back to projections
    /// </summary>
    /// <typeparam name="TEntity">Entity with Guid typed key</typeparam>
    /// <typeparam name="TProjection">Projection with Guid typed key</typeparam>
    public abstract class BluePrintsProjectionMasterDetailBase<TEntity, TProjection> : ProjectionMasterDetailBase<TEntity, TProjection>
        where TEntity : class, IGuidEntityKey, new()
        where TProjection : class, IGuidEntityKey, new()
    {
        public Guid GUID
        {
            get { return base.EntityKey; }
            set { base.EntityKey = value; }
        }
    }
}
