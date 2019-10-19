using BaseModel.DataModel;
using BaseModel.Misc;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single BASELINE object view model.
    /// </summary>
    public partial class EXO_MasterJobCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <BASELINE_ITEM, ExoSubJobProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of BASELINE_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static EXO_MasterJobCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new EXO_MasterJobCollectionViewModelWrapper(unitOfWorkFactory));
        }
        
        /// <summary>
        /// Initializes a new instance of the BASELINEViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the BASELINEViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected EXO_MasterJobCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {

        }

        #region Database Operations
        private readonly IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
#if MONTREAL
        private IPrimeroEntitiesUnitOfWork primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(true).CreateUnitOfWork();
#else
        private IPrimeroEntitiesUnitOfWork primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
#endif

        protected override void resolveParameters(object parameter)
        {
        }

        protected override void addEntitiesLoader()
        {
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
                    JOBCOST_FLAGS masterFlag = primeroUnitOfWork.JOBCOST_FLAGS.FirstOrDefault(x => x.JOBNO == masterJob.JOBNO);
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
                        slaveSubJob.ISACTIVE = masterJob.ISACTIVE;
                        slaveSubJob.BRANCHNO = masterJob.BRANCHNO;

                        if(masterFlag != null)
                        {
                            JOBCOST_FLAGS slaveFlag = primeroUnitOfWork.JOBCOST_FLAGS.FirstOrDefault(x => x.JOBNO == slaveSubJob.JOBNO);
                            if(slaveFlag != null)
                            {
                                slaveFlag.INVOICEREADY = masterFlag.INVOICEREADY;
                                slaveFlag.ISACTIVE = masterFlag.ISACTIVE;
                                slaveFlag.ISCOMPLETE = masterFlag.ISCOMPLETE;
                                slaveFlag.ISARCHIVED = masterFlag.ISARCHIVED;
                                slaveFlag.FLAG01 = masterFlag.FLAG01;
                                slaveFlag.FLAG02 = masterFlag.FLAG02;
                                slaveFlag.FLAG03 = masterFlag.FLAG03;
                                slaveFlag.FLAG04 = masterFlag.FLAG04;
                                slaveFlag.FLAG05 = masterFlag.FLAG05;
                                slaveFlag.FLAG06 = masterFlag.FLAG06;
                                slaveFlag.FLAG07 = masterFlag.FLAG07;
                                slaveFlag.FLAG08 = masterFlag.FLAG08;
                                slaveFlag.FLAG09 = masterFlag.FLAG09;
                                slaveFlag.FLAG10 = masterFlag.FLAG10;
                                slaveFlag.FLAG11 = masterFlag.FLAG11;
                                slaveFlag.FLAG12 = masterFlag.FLAG12;
                                slaveFlag.FLAG13 = masterFlag.FLAG13;
                                slaveFlag.FLAG14 = masterFlag.FLAG14;
                                slaveFlag.FLAG15 = masterFlag.FLAG15;
                            }
                        }

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
        public override string ViewName
        {
            get
            {
                //return "BASELINE_ITEMSViewModelWrapper" + view_project_specific_affix;
                return "EXO_MasterJobCollectionViewModelWrapper";
            }
        }

        public override string UnifiedRowValidation(ExoSubJobProjection projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(ExoSubJobProjection projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }
    }
}