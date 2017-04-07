using System;
using BluePrints.Common.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;
using DevExpress.Mvvm;

namespace BluePrints.Common.Projections
{
    public abstract class ProjectionBase<TEntity> : BindableBase, IProjection<TEntity>
        where TEntity : class, IHaveGUID, new()
    {
        public TEntity Entity { get; set; }

        public ProjectionBase()
        {
            Entity = new TEntity();
        }

        public ProjectionBase(TEntity entity)
        {
            Entity = entity;
        }

        [Key]
        public Guid GUID
        {
            get { return Entity.GUID; }
            set { Entity.GUID = value; }
        }
    }

    public abstract class ProjectionMasterDetailBase<TEntity, TProjection> : ProjectionBase<TEntity>, IProjectionMasterDetail<TEntity, TProjection>
        where TEntity : class, IHaveGUID, new()
        where TProjection : class, IHaveGUID, new()
    {
        protected virtual ObservableCollection<TProjection> detailEntities { get; set; }
        public virtual ObservableCollection<TProjection> DetailEntities
        {
            get { return detailEntities; }
            set { detailEntities = value; }
        }

        public ProjectionMasterDetailBase()
            : base()
        {
            DetailEntities = new ObservableCollection<TProjection>();
        }
    }
}
