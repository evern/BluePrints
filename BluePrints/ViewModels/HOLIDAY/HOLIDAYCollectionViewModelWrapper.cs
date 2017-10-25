using BaseModel.DataModel;
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

        protected override void resolveParameters(object parameter)
        {
        }

        protected override void initializeEntitiesLoadersDescription()
        {
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.HOLIDAYS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<HOLIDAY>, IQueryable<HOLIDAY>> specifyMainViewModelProjection()
        {
            return query => query.OrderBy(x => x.HOLIDAY_DATE);
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
                    HOLIDAY lookupExistingHoliday = DisplayEntities.FirstOrDefault(x => x.HOLIDAY_DATE.Date == dateToAdd.Date);
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
        protected override string ViewName
        {
            get { return "HOLIDAYCollectionViewModelWrapper"; }
        }

        #endregion
    }
}