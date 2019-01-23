using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Loader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Base
{
    public abstract class BluePrintsProjectionTreeCollectionWrapper<TMainEntity, TMainProjectionEntity, TMainEntityPrimaryKey,
        TMainEntityUnitOfWork> : ProjectionTreeCollectionWrapper<TMainEntity, TMainProjectionEntity, TMainEntityPrimaryKey,
        TMainEntityUnitOfWork>
            where TMainEntity : class, IGuidEntityKey, new()
            where TMainProjectionEntity : class, IProjection<TMainEntity>, IHaveSortOrder, INewEntityName, IHaveExpandState, IGuidParentEntityKey, ICanUpdate, new()
            where TMainEntityUnitOfWork : IUnitOfWork
    {
        public SpellCheckerModule SpellCheckerModule { get; set; }

        public override void OnLoaded()
        {
            SpellCheckerModule = new SpellCheckerModule();
            SpellCheckerModule.ApplySpellCheckMode(true);
            base.OnLoaded();
        }
    }

    public abstract class BluePrintsEntitiesTreeCollectionWrapper<TMainEntity, TMainProjectionEntity, TMainEntityPrimaryKey,
    TMainEntityUnitOfWork> : EntitiesTreeCollectionWrapper<TMainEntity, TMainProjectionEntity, TMainEntityPrimaryKey,
    TMainEntityUnitOfWork>
            where TMainEntity : class, IGuidEntityKey, new()
            where TMainProjectionEntity : class, IGuidEntityKey, IHaveSortOrder, INewEntityName, IHaveExpandState, IGuidParentEntityKey, ICanUpdate, new()
            where TMainEntityUnitOfWork : IUnitOfWork
    {
        public SpellCheckerModule SpellCheckerModule { get; set; }

        public override void OnLoaded()
        {
            SpellCheckerModule = new SpellCheckerModule();
            SpellCheckerModule.ApplySpellCheckMode(true);
            base.OnLoaded();
        }
    }
}
