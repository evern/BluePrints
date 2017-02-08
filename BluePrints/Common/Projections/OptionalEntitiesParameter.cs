using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    public class EntitiesParameter<TEntity>
        where TEntity : class
    {
        private TEntity entity;

        public EntitiesParameter(TEntity entity)
        {
            this.entity = entity;
        }

        public TEntity GetEntity()
        {
            return entity;
        }

        public override string ToString()
        {
            if (entity != null)
                return entity.GetType().GetProperty("GUID").GetValue(entity).ToString();
            else
                return string.Empty;
        }
    }

    public class OptionalEntitiesParameter<TEntity, TSecondEntity>
        where TEntity : class
        where TSecondEntity : class
    {
        private TEntity entity;
        private TSecondEntity secondEntity;

        public OptionalEntitiesParameter(TEntity entity, TSecondEntity secondEntity)
        {
            this.entity = entity;
            this.secondEntity = secondEntity;
        }

        public TEntity GetFirstEntity()
        {
            return entity;
        }

        public TSecondEntity GetSecondEntity()
        {
            return secondEntity;
        }

        public override string ToString()
        {
            if (entity != null)
                if (entity.GetType() == typeof(Guid))
                    return entity.GetType().GetProperty("GUID").GetValue(entity).ToString();
                else
                    return string.Empty;
            else if (secondEntity != null)
                if (secondEntity.GetType() == typeof(Guid))
                    return secondEntity.GetType().GetProperty("GUID").GetValue(entity).ToString();
                else
                    return string.Empty;
            else
                return string.Empty;
        }
    }
}