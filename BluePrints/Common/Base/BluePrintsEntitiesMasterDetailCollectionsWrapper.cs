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
    public abstract class BluePrintsEntitiesMasterDetailCollectionsWrapper<TMainEntity, TMainProjectionEntity, TMainEntityPrimaryKey,
        TMainEntityUnitOfWork> : ProjectionMasterDetailCollectionsWrapper<TMainEntity, TMainProjectionEntity, TMainEntityPrimaryKey,
        TMainEntityUnitOfWork>
        where TMainEntity : class, IGuidEntityKey, IHaveCreatedDate, IGuidParentEntityKey, new()
        where TMainProjectionEntity : class, IProjectionMasterDetail<TMainEntity, TMainProjectionEntity>, ICanUpdate, new()
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

    public abstract class BluePrintsEntitiesMasterOtherDetailCollectionsWrapper<TMainEntity, TChildEntity, TMainProjectionEntity, TMainEntityPrimaryKey,
    TMainEntityUnitOfWork> : ProjectionMasterOtherDetailCollectionsWrapper<TMainEntity, TChildEntity, TMainProjectionEntity, TMainEntityPrimaryKey,
    TMainEntityUnitOfWork>
        where TMainEntity : class, IGuidEntityKey, new()
        where TChildEntity : class, IGuidEntityKey, IGuidParentEntityKey, new()
        where TMainProjectionEntity : class, IProjectionMasterOtherDetail<TMainEntity, TChildEntity>, ICanUpdate, new()
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

    public abstract class BluePrintsEntitiesStaticMasterOtherDetailCollectionsWrapper<TStaticEntity, TMainEntity, TChildEntity, TAllEntityPrimaryKey,
        TMainEntityUnitOfWork> : ProjectionStaticMasterOtherDetailCollectionsWrapper<TStaticEntity, TMainEntity, TChildEntity, TAllEntityPrimaryKey,
        TMainEntityUnitOfWork>
        where TStaticEntity : class, IHaveDetail<TMainEntity>, IGuidEntityKey, ICanUpdate, IHaveSortOrder, new()
        where TMainEntity : class, IHaveDetail<TChildEntity>, IGuidEntityKey, IGuidParentEntityKey, ICanUpdate, new()
        where TChildEntity : class, IGuidEntityKey, IGuidParentEntityKey, new()
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
