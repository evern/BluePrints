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
    public abstract class BluePrintsEntitiesAutoNumberCollectionWrapper<TMainEntity, TMainProjectionEntity, TMainEntityPrimaryKey,
        TMainEntityUnitOfWork> : EntitiesAutoNumberCollectionWrapper<TMainEntity, TMainProjectionEntity, TMainEntityPrimaryKey,
        TMainEntityUnitOfWork>
            where TMainEntity : class, IGuidEntityKey, IEntityNumber, new()
            where TMainProjectionEntity : class, IGuidEntityKey, IEntityNumber, ICanUpdate, new()
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
