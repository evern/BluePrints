using System;
using System.Linq;
using DevExpress.Mvvm.POCO;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Data;
using BaseModel.ViewModel.Base;
using BaseModel.DataModel;
using BaseModel.ViewModel.Loader;
using System.Collections.Generic;
using System.Windows.Threading;
using System.ComponentModel;
using BaseModel.Misc;
using DevExpress.Xpf.Grid.TreeList;
using DevExpress.Mvvm;
using DevExpress.Xpf.Grid;
using System.Threading;
using BaseModel.ViewModel.Document;
using BluePrints.Common.Resources;
using System.Globalization;
using BluePrints.Common.Projections;
using BaseModel.Data.Helpers;
using BluePrints.Common;
using BluePrints.Common.Base;
using System.Collections.ObjectModel;
using DevExpress.Xpf.Editors;
using BluePrints.Common.Reports;
using System.IO;
using DevExpress.Xpf.Printing;
using System.Windows;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the RA_STUDY collection view model.
    /// </summary>
    public partial class RA_STUDYSingleObjectViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <RA_STUDY, RA_STUDY, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of RA_STUDYCollectionViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static RA_STUDYSingleObjectViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new RA_STUDYSingleObjectViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the RA_STUDYCollectionViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the RA_STUDYCollectionViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected RA_STUDYSingleObjectViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        public RA_STUDY EditingEntity { get; set; }

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> BluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected override void resolveParameters(object parameter)
        {
            EditingEntity = ((EntitiesParameter<RA_STUDY>)parameter).GetEntity();
            //var PROJECTParameter = (EntitiesParameter<Data.PROJECT>)parameter;
            //loadPROJECT = PROJECTParameter.GetEntity();
        }

        protected override void initializeEntitiesLoadersDescription()
        {
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(BluePrintsUnitOfWorkFactory, x => x.PROJECTS);
            loaderCollection.AddLoaderDescription<RA_STUDY_TYPE, RA_STUDY_TYPE, Guid, IBluePrintsEntitiesUnitOfWork>(BluePrintsUnitOfWorkFactory, x => x.RA_STUDY_TYPES);
            loaderCollection.AddLoaderDescription<RA_GUIDE_PROMPT, RA_GUIDE_PROMPT, Guid, IBluePrintsEntitiesUnitOfWork>(BluePrintsUnitOfWorkFactory, x => x.RA_GUIDE_PROMPTS);
            loaderCollection.AddLoaderDescription<RA_GUIDE_SUBPROMPT, RA_GUIDE_SUBPROMPT, Guid, IBluePrintsEntitiesUnitOfWork>(BluePrintsUnitOfWorkFactory, x => x.RA_GUIDE_SUBPROMPTS);
            loaderCollection.AddLoaderDescription(BluePrintsUnitOfWorkFactory, x => x.RA_STUDY_TEAMS, RA_STUDY_TEAMProjectionFunc);
            loaderCollection.AddLoaderDescription(BluePrintsUnitOfWorkFactory, x => x.RA_STUDY_DRAWINGS, RA_STUDY_DRAWINGProjectionFunc);
            loaderCollection.AddLoaderDescription(BluePrintsUnitOfWorkFactory, x => x.RA_STUDY_NODES, RA_STUDY_NODEProjectionFunc);
            loaderCollection.AddLoaderDescription(BluePrintsUnitOfWorkFactory, x => x.RA_STUDY_DATAS, RA_STUDY_DATAProjectionFunc);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(BluePrintsUnitOfWorkFactory, x => x.USERS);
            loaderCollection.AddLoaderDescription(BluePrintsUnitOfWorkFactory, x => x.PROJECT_REPORTS, PROJECT_REPORTProjectionFunc, null, true);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(BluePrintsUnitOfWorkFactory, x => x.RA_STUDIES);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<RA_STUDY>, IQueryable<RA_STUDY>> specifyMainViewModelProjection()
        {
            return query => query;
        }

        private Func<IRepositoryQuery<PROJECT_REPORT>, IQueryable<PROJECT_REPORT>> PROJECT_REPORTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == EditingEntity.GUID_PROJECT && x.REPORT_TYPE == ReportType.Risk_Assessment.ToString());
        }

        private Func<IRepositoryQuery<RA_STUDY_TEAM>, IQueryable<RA_STUDY_TEAM>> RA_STUDY_TEAMProjectionFunc()
        {
            return query => query.Where(x => x.GUID_STUDY == EditingEntity.GUID);
        }

        private Func<IRepositoryQuery<RA_STUDY_NODE>, IQueryable<RA_STUDY_NODE>> RA_STUDY_NODEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_STUDY == EditingEntity.GUID);
        }

        private Func<IRepositoryQuery<RA_STUDY_DRAWING>, IQueryable<RA_STUDY_DRAWING>> RA_STUDY_DRAWINGProjectionFunc()
        {
            return query => query.Where(x => x.GUID_STUDY == EditingEntity.GUID);
        }

        private Func<IRepositoryQuery<RA_STUDY_DATA>, IQueryable<RA_STUDY_DATA>> RA_STUDY_DATAProjectionFunc()
        {
            return query => guideStudyDataProjection(query);
        }

        private IQueryable<RA_STUDY_DATA> guideStudyDataProjection(IRepositoryQuery<RA_STUDY_DATA> studyData)
        {
            List<RA_STUDY_DATA> studyDataCollection = studyData.Where(x => x.RA_STUDY_NODE.GUID_STUDY == EditingEntity.GUID).ToList();
            studyDataCollection.ForEach(x => x.SetGuideSubPrompts(RA_GUIDE_SUBPROMPTCollection));

            return studyDataCollection.AsQueryable();
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<RA_STUDY> entities)
        {
            RA_STUDY_TEAMViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeTeamEntitySaved;
            RA_STUDY_DRAWINGViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeDrawingEntitySaved;
            RA_STUDY_NODEViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeNodeEntitySaved;
            RA_STUDY_DATAViewModel.UnifiedValueChangingCallback = studyDataUnifiedCellValueChanging;
            RA_STUDY_DATAViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeDataEntitySaved;
            MainViewModel.SetParentViewModel(this);
            RA_STUDY_TEAMViewModel.SetParentViewModel(this);
            RA_STUDY_DRAWINGViewModel.SetParentViewModel(this);
            RA_STUDY_NODEViewModel.SetParentViewModel(this);
            RA_STUDY_DATAViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        private void studyDataUnifiedCellValueChanging(string field_name, object old_value, object new_value, RA_STUDY_DATA projection, bool isNew)
        {
            if (field_name == BindableBase.GetPropertyName(() => new RA_STUDY_DATA().GUID_GUIDE_PROMPT))
            {
                //rate is not instantiated with commodity codes to be selected, hence initialization begins here
                if (isNew && new_value != null)
                {
                    projection.SetGuideSubPrompts(RA_GUIDE_SUBPROMPTCollection);
                }

                Guid? oldValue = projection.GuideSubPromptId;
                Guid? newValue = null;
                projection.GuideSubPromptId = newValue;
                RA_STUDY_DATAViewModel.EntitiesUndoRedoManager.PauseActionId();
                RA_STUDY_DATAViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new RA_STUDY_DATA().GuideSubPromptId), oldValue, newValue, EntityMessageType.Changed);
                projection.Update();
            }
        }
        #endregion

        #region View Behavior

        #endregion

        #region View Properties
        public void EditValueChanged(EditValueChangedEventArgs e)
        {
            if (MainViewModel == null || EditingEntity == null)
                return;
            
            string fieldName = ((BaseEdit)e.OriginalSource).Tag.ToString();
            DataUtils.TrySetNestedValue(fieldName, EditingEntity, e.NewValue);

            MainViewModel.Save(EditingEntity);
        }

        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public bool OnBeforeTeamEntitySaved(RA_STUDY_TEAM entity)
        {
            entity.GUID_STUDY = EditingEntity.GUID;
            return true;
        }

        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public bool OnBeforeDrawingEntitySaved(RA_STUDY_DRAWING entity)
        {
            entity.GUID_STUDY = EditingEntity.GUID;
            return true;
        }

        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public bool OnBeforeNodeEntitySaved(RA_STUDY_NODE entity)
        {
            entity.GUID_STUDY = EditingEntity.GUID;
            return true;
        }

        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public bool OnBeforeDataEntitySaved(RA_STUDY_DATA entity)
        {
            if (RA_STUDY_DATACollection == null)
                return false;

            entity.NUMBER = RA_STUDY_DATACollection.Where(x => x.GUID != Guid.Empty).Count() + 1;
            return true;
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "RA_STUDYSingleObjectViewModelWrapper_V1"; }
        }

        public IEnumerable<USER> USERCollection
        {
            get
            {
                var collection = GetEntities<USER>();
                if (collection == null)
                    return new List<USER>();

                //need to call ToList for tokenComboBoxEditSettings to work
                return collection.OrderBy(x => x.NAME).ToList();
            }
        }

        public IEnumerable<RA_STUDY_TYPE> RA_STUDY_TYPECollection
        {
            get
            {
                var collection = GetEntities<RA_STUDY_TYPE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.STUDY_TYPE);
                return collection;
            }
        }

        public IEnumerable<PROJECT> PROJECTCollection
        {
            get
            {
                var collection = GetEntities<PROJECT>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NUMBER);
                return collection;
            }
        }
        
        public CollectionViewModel<RA_STUDY_TEAM, RA_STUDY_TEAM, Guid, IBluePrintsEntitiesUnitOfWork> RA_STUDY_TEAMViewModel
        {
            get
            {
                if (loaderCollection == null)
                    return null;

                return (CollectionViewModel<RA_STUDY_TEAM, RA_STUDY_TEAM, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<RA_STUDY_TEAM>();
            }
        }


        public CollectionViewModel<RA_STUDY_DRAWING, RA_STUDY_DRAWING, Guid, IBluePrintsEntitiesUnitOfWork> RA_STUDY_DRAWINGViewModel
        {
            get
            {
                if (loaderCollection == null)
                    return null;

                return (CollectionViewModel<RA_STUDY_DRAWING, RA_STUDY_DRAWING, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<RA_STUDY_DRAWING>();
            }
        }

        public CollectionViewModel<RA_STUDY_NODE, RA_STUDY_NODE, Guid, IBluePrintsEntitiesUnitOfWork> RA_STUDY_NODEViewModel
        {
            get
            {
                if (loaderCollection == null)
                    return null;

                return (CollectionViewModel<RA_STUDY_NODE, RA_STUDY_NODE, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<RA_STUDY_NODE>();
            }
        }

        public CollectionViewModel<RA_STUDY_DATA, RA_STUDY_DATA, Guid, IBluePrintsEntitiesUnitOfWork> RA_STUDY_DATAViewModel
        {
            get
            {
                if (loaderCollection == null)
                    return null;

                return (CollectionViewModel<RA_STUDY_DATA, RA_STUDY_DATA, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<RA_STUDY_DATA>();
            }
        }

        public IEnumerable<RA_STUDY_DATA> RA_STUDY_DATACollection
        {
            get
            {
                var collection = GetEntities<RA_STUDY_DATA>();
                return collection;
            }
        }

        public IEnumerable<RA_STUDY_TEAM> RA_STUDY_TEAMCollection
        {
            get
            {
                var collection = GetEntities<RA_STUDY_TEAM>();
                return collection;
            }
        }

        public IEnumerable<RA_STUDY_DRAWING> RA_STUDY_DRAWINGCollection
        {
            get
            {
                var collection = GetEntities<RA_STUDY_DRAWING>();
                return collection;
            }
        }

        public IEnumerable<RA_STUDY_NODE> RA_STUDY_NODECollection
        {
            get
            {
                var collection = GetEntities<RA_STUDY_NODE>();
                return collection;
            }
        }

        public IEnumerable<RA_GUIDE_PROMPT> RA_GUIDE_PROMPTCollection
        {
            get
            {
                var collection = GetEntities<RA_GUIDE_PROMPT>();
                return collection;
            }
        }

        public IEnumerable<RA_GUIDE_SUBPROMPT> RA_GUIDE_SUBPROMPTCollection
        {
            get
            {
                var collection = GetEntities<RA_GUIDE_SUBPROMPT>();
                return collection;
            }
        }
        #endregion

        #region Navigation
        public override string UnifiedRowValidation(RA_STUDY projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(RA_STUDY projection, string field_name, object new_value)
        {
            return string.Empty;
        }
        #endregion


        #region Reporting
        public bool CanEditReport()
        {
            if (MainViewModel == null || DisplayEntities == null)
                return false;

            return true;
        }

        public bool CanViewReport()
        {
            if (MainViewModel == null || DisplayEntities == null)
                return false;

            return true;
        }

        public void EditReport()
        {
            var reportDesigner = new UserReportDesigner(EditingEntity.PROJECT,
                (CollectionViewModel<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork>)
                loaderCollection.GetViewModel<PROJECT_REPORT>(), ReportType.Risk_Assessment);
            if (reportDesigner.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                reportDesigner.Dispose();
            else
                reportDesigner.Dispose();
        }

        XtraReportStudyData risk_assessmentReport;
        public void ViewReport()
        {
            risk_assessmentReport = new XtraReportStudyData();
            PROJECT_REPORT dbProjectReport = loaderCollection.GetObject<PROJECT_REPORT>();
            if (dbProjectReport != null)
            {
                var reportString = dbProjectReport.REPORT.ToString();
                using (var sw = new StreamWriter(new MemoryStream()))
                {
                    sw.Write(reportString);
                    sw.Flush();
                    risk_assessmentReport.LoadLayout(sw.BaseStream);
                }
            }

            risk_assessmentReport.AssignProperties(EditingEntity, RA_STUDY_DATACollection, RA_STUDY_DRAWINGCollection, RA_STUDY_NODECollection, RA_STUDY_TEAMCollection);
            DocumentPreviewWindow previewWindow = new DocumentPreviewWindow();
            previewWindow.PreviewControl.DocumentSource = risk_assessmentReport;
            previewWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            previewWindow.WindowState = WindowState.Maximized;
            risk_assessmentReport.RequestParameters = false;
            risk_assessmentReport.CreateDocument(true);
            previewWindow.Show();
        }
        #endregion

    }
}
