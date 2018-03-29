using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Helpers;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
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

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single BASELINE object view model.
    /// </summary>
    public partial class EXODesignSubjobViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <BASELINE_ITEM, ExoSubJobProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of BASELINE_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static EXODesignSubjobViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new EXODesignSubjobViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the BASELINEViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the BASELINEViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected EXODesignSubjobViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        private Data.PROJECT loadPROJECT;
        private BASELINE liveBASELINE;
        private PROGRESS livePROGRESS;
        private readonly IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private readonly IPrimeroEntitiesUnitOfWork primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();

        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<Data.PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
        }

        protected override void initializeEntitiesLoadersDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc, assign_baseline);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc, assign_progress);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.SUBJOBS, SUBJOBProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DELIVERABLES_STATUSES, DELIVERABLES_STATUSProjectionFunc);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);
        }

        private Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> WORKPACKProjectionFunc()
        {
            return query => query.Where(x => x.SUBJOB.GUID_PROJECT == loadPROJECT.GUID && x.SUBJOB.PHASE.PHASE_TYPE == PhaseType.Design);
        }

        private Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> BASELINEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == BaselineStatus.Live);
        }

        private void assign_baseline(BASELINE entity)
        {
            if (entity == null && !SupressCompulsoryEntityNotFoundMessage)
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("Live baseline not found")));

            liveBASELINE = entity;
        }

        private void assign_progress(PROGRESS progress)
        {
            if (progress == null && !SupressCompulsoryEntityNotFoundMessage)
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("Live progress not found")));

            livePROGRESS = progress;
        }

        private Func<IRepositoryQuery<PROGRESS_ITEM>, IQueryable<PROGRESS_ITEM>> PROGRESS_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROGRESS == livePROGRESS.GUID);
        }

        private Func<IRepositoryQuery<SUBJOB>, IQueryable<SUBJOB>> SUBJOBProjectionFunc()
        {
            //legacy subjob restrictions
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<Data.PHASE>, IQueryable<Data.PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => x.PHASE_TYPE == PhaseType.Design);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<DELIVERABLES_STATUS>, IQueryable<DELIVERABLES_STATUS>> DELIVERABLES_STATUSProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == ProgressStatus.Live);

        }

        private Func<IRepositoryQuery<PROJECT_REPORT>, IQueryable<PROJECT_REPORT>> PROJECT_REPORTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.REPORT_TYPE == ReportType.Baseline_Report.ToString());
        }
        #endregion

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<ExoSubJobProjection>> specifyMainViewModelProjection()
        {
            return query => ExoQueries.GetExoSubJobProjection(query.Where(x => x.GUID_BASELINE == liveBASELINE.GUID), WORKPACKCollection, loadPROJECT, livePROGRESS, RATECollection, PROGRESS_ITEMCollection, primeroUnitOfWork);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ExoSubJobProjection> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        public void UploadToExo()
        {
            foreach(ExoSubJobProjection selectedLine in DisplaySelectedEntities)
            {
                if(!selectedLine.IsSubJobExistsInExo)
                {
                    int? subJobId = FindExistingOrAddSubJob(selectedLine.SubJob);
                    if (subJobId != null)
                    {
                        selectedLine.SubJob.Id = subJobId;
                    }
                }
            }
        }

        private int? FindExistingOrAddSubJob(PrimeroSubJob subJob)
        {
            var pUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            JOBCOST_HDR existingSubJobs = ExoQueries.GetProjectSubJob(pUnitOfWork, loadPROJECT, subJob.Code);
            //if(existingSubJobs != null)
            //    return (int)existingSubJobs.JOBNO;
            //else
            //{
                JOBCOST_HDR masterJob = ExoQueries.GetProjectSubJob(pUnitOfWork, loadPROJECT, loadPROJECT.NUMBER);
                if (masterJob != null)
                {
                    JOBCOST_HDR newExoSubJob = new JOBCOST_HDR();
                    newExoSubJob.ESTIMATE = 0;
                    newExoSubJob.INVOICED = 0;
                    newExoSubJob.THETIME = 0;
                    newExoSubJob.MATERIALS = 0;
                    newExoSubJob.DEF_OVERHEAD = 0;
                    newExoSubJob.MATERIALSCOST = 0;
                    newExoSubJob.ESTIMATECOST = 0;
                    newExoSubJob.THETIMECOST = 0;
                    newExoSubJob.INVOICEDCOST = 0;
                    newExoSubJob.JOBCODE = subJob.Code;
                    newExoSubJob.ACCNO = masterJob.ACCNO;
                    newExoSubJob.CUSTORDNO = string.Empty;
                    newExoSubJob.STATUS = "C";
                    newExoSubJob.TITLE = string.Empty;
                    newExoSubJob.CATEGORY = masterJob.CATEGORY;
                    newExoSubJob.JOBTYPE = masterJob.JOBTYPE;
                    newExoSubJob.STAFFNO = masterJob.STAFFNO;
                    newExoSubJob.ACTIONBY = masterJob.ACTIONBY;
                    newExoSubJob.MASTER_JOBNO = masterJob.JOBNO;
                    newExoSubJob.COSTGL = 0;
                    newExoSubJob.SALESGL = 0;
                    newExoSubJob.SERIALNO = string.Empty;
                    newExoSubJob.CONTACT = string.Empty;
                    newExoSubJob.PRIVATE_NOTE = string.Empty;
                    newExoSubJob.COSTSUBGL = 0;
                    newExoSubJob.SALESSUBGL = 0;
                    newExoSubJob.CONTACTNO = masterJob.CONTACTNO;
                    newExoSubJob.DELADDR1 = masterJob.DELADDR1;
                    newExoSubJob.DELADDR2 = masterJob.DELADDR2;
                    newExoSubJob.DELADDR3 = masterJob.DELADDR3;
                    newExoSubJob.DELADDR4 = masterJob.DELADDR4;
                    newExoSubJob.DELADDR5 = masterJob.DELADDR5;
                    newExoSubJob.DELADDR6 = masterJob.DELADDR6;
                    newExoSubJob.WRITE_OFF_COST = masterJob.WRITE_OFF_COST;
                    newExoSubJob.TOTAL_HOURS = 0;
                    newExoSubJob.EST_HOURS = 0;
                    newExoSubJob.ASSET_COST = 0;
                    newExoSubJob.ASSET_VALUE = 0;
                    newExoSubJob.BRANCHNO = 0;
                    newExoSubJob.ISACTIVE = "Y";
                    newExoSubJob.HASUNBILLED = "N";
                    newExoSubJob.INVOICEREADY = "N";
                    newExoSubJob.CALLBACKDATE = DateTime.Now;
                    newExoSubJob.ENTRYDATE = DateTime.Now;
                    newExoSubJob.TOTALVALUE = 0;
                    newExoSubJob.TOTALCOST = 0;
                    newExoSubJob.WIPLOC = masterJob.WIPLOC;
                    newExoSubJob.EXCHRATE = masterJob.EXCHRATE;
                    newExoSubJob.RETENTION_RATE = 0;
                    newExoSubJob.RETENTION2_MIN = 0;
                    newExoSubJob.RETENTION2_RATE = 0;
                    newExoSubJob.RETENTION3_MIN = 0;
                    newExoSubJob.RETENTION3_RATE = 0;
                    newExoSubJob.ALLOWANCE = 0;
                    newExoSubJob.BILLINGMODE = 0;
                    newExoSubJob.DESCRIPTION = string.Empty;
                    newExoSubJob.CAMPAIGN_WAVE_SEQNO = -1;
                    newExoSubJob.OPPORTUNITY_SEQNO = -1;
                    newExoSubJob.LINECHARGE_WRITEOFF = 0;
                    newExoSubJob.INVOICE_VIA_MASTER = "Y";
                    pUnitOfWork.JOBCOST_HDR.Add(newExoSubJob);
                    pUnitOfWork.SaveChanges();
                    return newExoSubJob.JOBNO;
                }
                else
                    return null;
            //}
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get
            {
                //return "BASELINE_ITEMSViewModelWrapper" + view_project_specific_affix;
                return "ExoDesignSubJobViewModelWrapper";
            }
        }

        public override string UnifiedValueValidation(ExoSubJobProjection projection, string field_name, object new_value)
        {
            return string.Empty;
        }


        public IEnumerable<WORKPACK> WORKPACKCollection
        {
            get
            {
                var collection = GetEntities<WORKPACK>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }


        public IEnumerable<RATE> RATECollection
        {
            get
            {
                return GetEntities<RATE>();
            }
        }

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMCollection
        {
            get
            {
                return GetEntities<PROGRESS_ITEM>();
            }
        }
    }
}