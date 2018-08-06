using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Helpers;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Common.Reports;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using BluePrints.Reports;
using DevExpress.Data.Filtering;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Printing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single BASELINE object view model.
    /// </summary>
    public partial class EXOAllSubjobViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <BASELINE_ITEM, ExoSubJobProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of BASELINE_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static EXOAllSubjobViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new EXOAllSubjobViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the BASELINEViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the BASELINEViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected EXOAllSubjobViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        private readonly IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private readonly IPrimeroEntitiesUnitOfWork primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();

        protected override void resolveParameters(object parameter)
        {
        }

        protected override void initializeEntitiesLoadersDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
        }
        #endregion

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<ExoSubJobProjection>> specifyMainViewModelProjection()
        {
            return query => ExoQueries.GetMasterExoLines(primeroUnitOfWork).OrderBy(x => x.SubJob.Code).AsQueryable();
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ExoSubJobProjection> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
        }

        protected override void OnSelectedEntitiesChanged()
        {
        }
        
        public void StandardizeLines()
        {
            int updatedLines = 0;
            LoadingScreenManager.ShowLoadingScreen(DisplaySelectedEntities.Count);
            foreach(var entity in DisplaySelectedEntities)
            {
                if (entity.SubJob == null || entity.SubJob.Id == null)
                    continue;

                IEnumerable<JOBCOST_HDR> slaveSubJobs = ExoQueries.GetSlaveExoLines(primeroUnitOfWork, (int)entity.SubJob.Id);
                JOBCOST_HDR masterJob = slaveSubJobs.FirstOrDefault(x => x.JOBNO == (int)entity.SubJob.Id);
                if(masterJob != null)
                {
                    foreach (JOBCOST_HDR slaveSubJob in slaveSubJobs)
                    {
                        slaveSubJob.ACCNO = masterJob.ACCNO;
                        slaveSubJob.STATUS = masterJob.STATUS;
                        slaveSubJob.CATEGORY = masterJob.CATEGORY;
                        slaveSubJob.JOBTYPE = masterJob.JOBTYPE;
                        slaveSubJob.STAFFNO = masterJob.STAFFNO;
                        slaveSubJob.ACTIONBY = masterJob.ACTIONBY;
                        slaveSubJob.CONTACTNO = masterJob.CONTACTNO;
                        slaveSubJob.DELADDR1 = masterJob.DELADDR1;
                        slaveSubJob.DELADDR2 = masterJob.DELADDR2;
                        slaveSubJob.DELADDR3 = masterJob.DELADDR3;
                        slaveSubJob.DELADDR4 = masterJob.DELADDR4;
                        slaveSubJob.DELADDR5 = masterJob.DELADDR5;
                        slaveSubJob.DELADDR6 = masterJob.DELADDR6;
                        slaveSubJob.WIPLOC = masterJob.WIPLOC;
                        slaveSubJob.EXCHRATE = masterJob.EXCHRATE;

                        updatedLines += 1;
                    }
                }

                LoadingScreenManager.Progress();
            }

            primeroUnitOfWork.SaveChanges();

            LoadingScreenManager.CloseLoadingScreen();
            MessageBoxService.ShowMessage(updatedLines + " updated to match master job");
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get
            {
                //return "BASELINE_ITEMSViewModelWrapper" + view_project_specific_affix;
                return "EXOAllSubjobViewModelWrapper";
            }
        }

        public override string UnifiedRowValidation(ExoSubJobProjection projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(ExoSubJobProjection projection, string field_name, object new_value)
        {
            return string.Empty;
        }
    }
}