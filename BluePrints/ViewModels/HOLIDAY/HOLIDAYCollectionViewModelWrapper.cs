using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class HOLIDAYCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <HOLIDAY, HOLIDAY, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of HOLIDAYCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static HOLIDAYCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new HOLIDAYCollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the HOLIDAYCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the HOLIDAYCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected HOLIDAYCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private PROJECT loadPROJECT;
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
        }

        protected override void addEntitiesLoader()
        {
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.HOLIDAYS);
        }

        protected override Func<IRepositoryQuery<HOLIDAY>, IQueryable<HOLIDAY>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.HOLIDAY_DATE);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<HOLIDAY> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #endregion

        #region View Properties
        private DevExpress.Mvvm.IDialogService DateFromToDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("DateFromToDialogService"); }
        }

        public bool CanAddDateRange()
        {
            return !IsLoading;
        }

        public void AddDateRange()
        {
            var dateFromToViewModel = DateFromToDialogViewModel.Create();
            if (DateFromToDialogService.ShowDialog(MessageButton.OKCancel, "Select Date Range to Add", "DateFromTo", dateFromToViewModel) == MessageResult.OK)
            {
                DateTime dateFrom = dateFromToViewModel.DateFrom;
                DateTime dateTo = dateFromToViewModel.DateTo;

                DateTime dateToAdd = dateFrom;
                while (dateToAdd <= dateTo)
                {
                    HOLIDAY lookupExistingHoliday = Entities.FirstOrDefault(x => x.HOLIDAY_DATE.Date == dateToAdd.Date);
                    if(lookupExistingHoliday == null)
                    {
                        HOLIDAY newHoliday = new HOLIDAY();
                        newHoliday.HOLIDAY_DATE = dateToAdd.Date;
                        MainViewModel.Save(newHoliday);
                    }

                    dateToAdd = dateToAdd.AddDays(1);
                };
            }
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "HOLIDAYCollectionViewModelWrapper"; }
        }

        #endregion

        #region Collection Call Backs
        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(HOLIDAY projection, out bool isNew)
        {
            projection.GUID_PROJECT = loadPROJECT.GUID;
            return base.OnBeforeProjectionSaveIsContinue(projection, out isNew);
        }

        public override string UnifiedRowValidation(HOLIDAY projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(HOLIDAY projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }
        #endregion
    }
}