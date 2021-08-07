using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.Properties;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.Common.Filtering
{
    internal static class FiltersSettings {
        public static FilterTreeViewModel<BASELINE_ITEMProgress, Guid> GetBASELINE_ITEMProgressFilterTree(ISupportFiltering<BASELINE_ITEMProgress> parentViewModel, IEnumerable<BASELINE_ITEMProgress> entities)
        {
            return FilterTreeViewModel<BASELINE_ITEMProgress, Guid>.Create(new FilterTreeModelPageSpecificSettings<Settings>(Settings.Default, "Status", x => x.BASELINE_ITEMProgressStaticFilters, x => x.BASELINE_ITEMProgressCustomFilters
            ), parentViewModel, entities).SetParentViewModel(parentViewModel);
        }

        static IBluePrintsEntitiesUnitOfWork CreateUnitOfWork() {
            return BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
        }

        static void RegisterEntityChangedMessageHandler<TEntity, TPrimaryKey>(object recipient, Action handler) {
            Messenger.Default.Register<EntityMessage<TEntity, TPrimaryKey>>(recipient, message => handler());
        }
    }
}
