using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Loader;
using BaseModel.ViewModel.Services;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Data;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace BluePrints.ViewModels
{
    public class HSECollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <HSE, HSEReportProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of HSECollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static HSECollectionViewModelWrapper Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new HSECollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the HSECollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the HSECollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected HSECollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        public DateTime yearSelect { get; set; }
        public DateTime dateFrom { get; set; }
        public DateTime dateTo { get; set; }
        public PROJECT loadPROJECT { get; set; }
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected override void resolveParameters(object parameter)
        {
            GetDateRange();
            dateFrom = new DateTime(yearSelect.Year - 1, 7, 1);
            dateTo = new DateTime(yearSelect.Year, 6, 1);

            if (parameter != null)
            {
                var PROJECTParameter = (EntitiesParameter<PROJECT>)parameter;
                loadPROJECT = PROJECTParameter.GetEntity();
            }
        }

        protected override void initializeEntitiesLoadersDescription()
        {
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.HSES);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<HSE>, IQueryable<HSEReportProjection>> specifyMainViewModelProjection()
        {
            if(loadPROJECT == null)
                return query => HSEReportProjectionQueries.UnwrapHSE(query.Where(x => x.HSE_DATE >= dateFrom && x.HSE_DATE <= dateTo));
            else
                return query => HSEReportProjectionQueries.UnwrapHSE(query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).Where(x => x.HSE_DATE >= dateFrom && x.HSE_DATE <= dateTo));
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<HSEReportProjection> entities)
        {
            MainViewModel.AlwaysSkipMessage = true;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        public override string UnifiedValueValidation(HSEReportProjection projection, string field_name, object new_value)
        {
            return string.Empty;
        }

        public override string UnifiedRowValidation(HSEReportProjection projection)
        {
            return string.Empty;
        }
        #endregion

        #region View Properties
        private DevExpress.Mvvm.IDialogService DateFromToDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("DateFromToDialogService"); }
        }

        public void GetDateRange()
        {
            UICommand okCommand = new UICommand()
            {
                Id = TimesheetDateDialogAction.Ok,
                Caption = "Ok",
                IsCancel = true,
                IsDefault = false,
            };

            UICommand currentCommand = new UICommand()
            {
                Id = TimesheetDateDialogAction.UseWeekStart,
                Caption = "Use current year",
                IsCancel = true,
                IsDefault = false,
            };

            var yearSelectViewModel = YearSelectViewModel.Create();
            yearSelectViewModel.YearSelect = DateTime.Now.Month < 7 ? DateTime.Now : DateTime.Now.AddYears(1);
            UICommand result = DateFromToDialogService.ShowDialog(new List<UICommand>() { okCommand, currentCommand }, "Year Select", "YearSelect", yearSelectViewModel);

            if (result == okCommand)
                yearSelect = yearSelectViewModel.YearSelect;
            else
                yearSelect = DateTime.Now.Month < 7 ? DateTime.Now : DateTime.Now.AddYears(1);
        }

        [ServiceProperty(Key = "PivotGridControlService")]
        public virtual IPivotGridControlService PivotGridControlService { get { return null; } }


        protected override string ExportExcelFilename()
        {
            return "hse_stats_export.xlsx";
        }

        public override void ExportToExcel()
        {
            string ResultPath = string.Empty;
            if (FolderBrowserDialogService.ShowDialog())
            {
                ResultPath = FolderBrowserDialogService.ResultPath;
                PivotGridControlService.ExportToExcel(ResultPath + "\\" + ExportExcelFilename());
            }
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "HSECollectionViewModelWrapper"; }
        }
        #endregion
    }
}