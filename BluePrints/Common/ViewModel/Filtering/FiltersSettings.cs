using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.ViewModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BluePrints.Data;
using BluePrints.Properties;
using BluePrints.ViewModels;

namespace BluePrints.Common.ViewModel.Filtering
{
    internal static class FiltersSettings
    {
        private static IBluePrintsEntitiesUnitOfWork bluePrintsEntitiesUnitOfWork = CreateUnitOfWork();

        public static FilterTreeViewModel<PROJECT, PROJECT, Guid> GetPROJECTFilterTree(object parentViewModel)
        {
            return FilterTreeViewModel<PROJECT, PROJECT, Guid>.Create(
                new FilterTreeModelPageSpecificSettings<Settings, PROJECT>(Settings.Default,
                    bluePrintsEntitiesUnitOfWork.PROJECTS, "PROJECT", x => x.AllStaticFilters,
                    x => x.PROJECTCustomFilter,
                    new[]
                    {
                        BindableBase.GetPropertyName(() => new PROJECT().GUID)
                    }),
                bluePrintsEntitiesUnitOfWork.PROJECTS,
                new Action<object, Action>(RegisterEntityChangedMessageHandler<PROJECT, Guid>), true
            ).SetParentViewModel(parentViewModel);
        }

        public static FilterTreeViewModel<DEPARTMENT, DEPARTMENT, Guid> GetDEPARTMENTFilterTree(object parentViewModel)
        {
            return FilterTreeViewModel<DEPARTMENT, DEPARTMENT, Guid>.Create(
                new FilterTreeModelPageSpecificSettings<Settings, DEPARTMENT>(Settings.Default,
                    bluePrintsEntitiesUnitOfWork.DEPARTMENTS, "DEPARTMENT", x => x.AllStaticFilters,
                    x => x.DEPARTMENTCustomFilter,
                    new[]
                    {
                        BindableBase.GetPropertyName(() => new DEPARTMENT().GUID)
                    }),
                bluePrintsEntitiesUnitOfWork.DEPARTMENTS,
                new Action<object, Action>(RegisterEntityChangedMessageHandler<DEPARTMENT, Guid>)
            ).SetParentViewModel(parentViewModel);
        }

        public static FilterTreeViewModel<DISCIPLINE, DISCIPLINE, Guid> GetDISCIPLINEFilterTree(object parentViewModel)
        {
            return FilterTreeViewModel<DISCIPLINE, DISCIPLINE, Guid>.Create(
                new FilterTreeModelPageSpecificSettings<Settings, DISCIPLINE>(Settings.Default,
                    bluePrintsEntitiesUnitOfWork.DISCIPLINES, "DISCIPLINE", x => x.AllStaticFilters,
                    x => x.DISCIPLINECustomFilter,
                    new[]
                    {
                        BindableBase.GetPropertyName(() => new DISCIPLINE().GUID)
                    }),
                bluePrintsEntitiesUnitOfWork.DISCIPLINES,
                new Action<object, Action>(RegisterEntityChangedMessageHandler<DISCIPLINE, Guid>)
            ).SetParentViewModel(parentViewModel);
        }

        public static FilterTreeViewModel<DOCTYPE, DOCTYPE, Guid> GetDOCTYPEFilterTree(object parentViewModel)
        {
            return FilterTreeViewModel<DOCTYPE, DOCTYPE, Guid>.Create(
                new FilterTreeModelPageSpecificSettings<Settings, DOCTYPE>(Settings.Default,
                    bluePrintsEntitiesUnitOfWork.DOCTYPES, "DOCTYPE", x => x.AllStaticFilters,
                    x => x.DOCTYPECustomFilter,
                    new[]
                    {
                        BindableBase.GetPropertyName(() => new DOCTYPE().GUID)
                    }),
                bluePrintsEntitiesUnitOfWork.DOCTYPES,
                new Action<object, Action>(RegisterEntityChangedMessageHandler<DOCTYPE, Guid>)
            ).SetParentViewModel(parentViewModel);
        }

        public static FilterTreeViewModel<PHASE, PHASE, Guid> GetPHASEFilterTree(object parentViewModel)
        {
            return FilterTreeViewModel<PHASE, PHASE, Guid>.Create(
                new FilterTreeModelPageSpecificSettings<Settings, PHASE>(Settings.Default,
                    bluePrintsEntitiesUnitOfWork.PHASES, "PHASE", x => x.AllStaticFilters, x => x.PHASECustomFilter,
                    new[]
                    {
                        BindableBase.GetPropertyName(() => new PHASE().GUID)
                    }),
                bluePrintsEntitiesUnitOfWork.PHASES,
                new Action<object, Action>(RegisterEntityChangedMessageHandler<PHASE, Guid>)
            ).SetParentViewModel(parentViewModel);
        }

        public static FilterTreeViewModel<AREA, PROJECT, Guid> GetAREAPROJECTFilterTree(object parentViewModel)
        {
            return FilterTreeViewModel<AREA, PROJECT, Guid>.Create(
                new FilterTreeModelPageSpecificSettings<Settings, PROJECT>(Settings.Default,
                    bluePrintsEntitiesUnitOfWork.PROJECTS, "AREA", x => x.AllStaticFilters, x => x.AREACustomFilter,
                    new[]
                    {
                        BindableBase.GetPropertyName(() => new AREA().GUID),
                        BindableBase.GetPropertyName(() => new AREA().GUID_PROJECT)
                    }),
                bluePrintsEntitiesUnitOfWork.AREAS,
                new Action<object, Action>(RegisterEntityChangedMessageHandler<AREA, Guid>)
            ).SetParentViewModel(parentViewModel);
        }

        public static FilterTreeViewModel<AREA, AREA, Guid> GetAREAFilterTree(object parentViewModel)
        {
            return FilterTreeViewModel<AREA, AREA, Guid>.Create(
                new FilterTreeModelPageSpecificSettings<Settings, AREA>(Settings.Default,
                    bluePrintsEntitiesUnitOfWork.AREAS, "AREA", x => x.AllStaticFilters, x => x.AREACustomFilter,
                    new[]
                    {
                        BindableBase.GetPropertyName(() => new AREA().GUID),
                        BindableBase.GetPropertyName(() => new AREA().GUID_PROJECT)
                    }),
                bluePrintsEntitiesUnitOfWork.AREAS,
                new Action<object, Action>(RegisterEntityChangedMessageHandler<AREA, Guid>)
            ).SetParentViewModel(parentViewModel);
        }

        public static FilterTreeViewModel<BASELINE, BASELINE, Guid> GetBASELINEFilterTree(object parentViewModel)
        {
            return FilterTreeViewModel<BASELINE, BASELINE, Guid>.Create(
                new FilterTreeModelPageSpecificSettings<Settings, BASELINE>(Settings.Default,
                    bluePrintsEntitiesUnitOfWork.BASELINES, "BASELINE", x => x.AllStaticFilters,
                    x => x.BASELINECustomFilter,
                    new[]
                    {
                        BindableBase.GetPropertyName(() => new BASELINE().GUID),
                        BindableBase.GetPropertyName(() => new BASELINE().GUID_PROJECT)
                    }),
                bluePrintsEntitiesUnitOfWork.BASELINES,
                new Action<object, Action>(RegisterEntityChangedMessageHandler<BASELINE, Guid>)
            ).SetParentViewModel(parentViewModel);
        }

        public static FilterTreeViewModel<RATE, RATE, Guid> GetRATEFilterTree(object parentViewModel)
        {
            return FilterTreeViewModel<RATE, RATE, Guid>.Create(
                new FilterTreeModelPageSpecificSettings<Settings, RATE>(Settings.Default,
                    bluePrintsEntitiesUnitOfWork.RATES, "RATE", x => x.AllStaticFilters, x => x.RATECustomFilter,
                    new[]
                    {
                        BindableBase.GetPropertyName(() => new RATE().GUID),
                        BindableBase.GetPropertyName(() => new RATE().GUID_PROJECT)
                    }),
                bluePrintsEntitiesUnitOfWork.RATES,
                new Action<object, Action>(RegisterEntityChangedMessageHandler<RATE, Guid>)
            ).SetParentViewModel(parentViewModel);
        }

        public static FilterTreeViewModel<WORKPACK, WORKPACK, Guid> GetWORKPACKFilterTree(object parentViewModel)
        {
            return FilterTreeViewModel<WORKPACK, WORKPACK, Guid>.Create(
                new FilterTreeModelPageSpecificSettings<Settings, WORKPACK>(Settings.Default,
                    bluePrintsEntitiesUnitOfWork.WORKPACKS, "WORKPACK", x => x.AllStaticFilters,
                    x => x.WORKPACKCustomFilter,
                    new[]
                    {
                        BindableBase.GetPropertyName(() => new WORKPACK().GUID),
                        BindableBase.GetPropertyName(() => new WORKPACK().GUID_PROJECT)
                    }),
                bluePrintsEntitiesUnitOfWork.WORKPACKS,
                new Action<object, Action>(RegisterEntityChangedMessageHandler<WORKPACK, Guid>)
            ).SetParentViewModel(parentViewModel);
        }

        public static FilterTreeViewModel<PROGRESS, PROGRESS, Guid> GetPROGRESSFilterTree(object parentViewModel)
        {
            return FilterTreeViewModel<PROGRESS, PROGRESS, Guid>.Create(
                new FilterTreeModelPageSpecificSettings<Settings, PROGRESS>(Settings.Default,
                    bluePrintsEntitiesUnitOfWork.PROGRESSES, "PROGRESS", x => x.AllStaticFilters,
                    x => x.PROGRESSCustomFilter,
                    new[]
                    {
                        BindableBase.GetPropertyName(() => new PROGRESS().GUID),
                        BindableBase.GetPropertyName(() => new PROGRESS().GUID_PROJECT)
                    }),
                bluePrintsEntitiesUnitOfWork.PROGRESSES,
                new Action<object, Action>(RegisterEntityChangedMessageHandler<PROGRESS, Guid>)
            ).SetParentViewModel(parentViewModel);
        }

        public static FilterTreeViewModel<VARIATION, VARIATION, Guid> GetVARIATIONFilterTree(object parentViewModel)
        {
            return FilterTreeViewModel<VARIATION, VARIATION, Guid>.Create(
                new FilterTreeModelPageSpecificSettings<Settings, VARIATION>(Settings.Default,
                    bluePrintsEntitiesUnitOfWork.VARIATIONS, "VARIATION", x => x.AllStaticFilters,
                    x => x.VARIATIONCustomFilter,
                    new[]
                    {
                        BindableBase.GetPropertyName(() => new VARIATION().GUID),
                        BindableBase.GetPropertyName(() => new VARIATION().GUID_PROJECT)
                    }),
                bluePrintsEntitiesUnitOfWork.VARIATIONS,
                new Action<object, Action>(RegisterEntityChangedMessageHandler<VARIATION, Guid>)
            ).SetParentViewModel(parentViewModel);
        }

        private static IBluePrintsEntitiesUnitOfWork CreateUnitOfWork()
        {
            return BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
        }

        private static void RegisterEntityChangedMessageHandler<TEntity, TPrimaryKey>(object recipient, Action handler)
        {
            Messenger.Default.Register<EntityMessage<TEntity, TPrimaryKey>>(recipient, message => handler());
        }
    }
}