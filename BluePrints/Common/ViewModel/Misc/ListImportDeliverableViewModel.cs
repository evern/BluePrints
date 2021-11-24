using BaseModel.Data.Helpers;
using BaseModel.ViewModel.Services;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;

namespace BaseModel.ViewModel.Dialogs
{
    public class ListImportDeliverableViewModel<BASELINE_ITEMProgressImportWrapper>
    {
        public static ListImportDeliverableViewModel<BASELINE_ITEMProgressImportWrapper> Create(IEnumerable<BASELINE_ITEMProgressImportWrapper> enumerableObjects, IEnumerable<PHASE> PHASECollection, IEnumerable<AREA> AREACollection, IEnumerable<DISCIPLINE> DISCIPLINECollection, IEnumerable<DOCTYPE> DOCTYPECollection, IEnumerable<DEPARTMENT> DEPARTMENTCollection)
        {
            return ViewModelSource.Create(() => new ListImportDeliverableViewModel<BASELINE_ITEMProgressImportWrapper>(enumerableObjects, PHASECollection, AREACollection, DISCIPLINECollection, DOCTYPECollection, DEPARTMENTCollection));
        }

        [ServiceProperty(Key = "DefaultTableViewService")]
        protected virtual ITableViewService TableViewService { get { return null; } }

        [ServiceProperty(Key = "DefaultGridControlService")]
        public virtual IGridControlService GridControlService { get { return null; } }

        ObservableCollection<BASELINE_ITEMProgressImportWrapper> sourceObjects;
        public int InternalNumSortIndex => 1;
        public ObservableCollection<BASELINE_ITEMProgressImportWrapper> SourceObjects
        {
            get
            {
                if (sourceObjects == null)
                    sourceObjects = new ObservableCollection<BASELINE_ITEMProgressImportWrapper>();

                return sourceObjects;
            }
        }

        public string Message { get; set; }

        protected ObservableCollection<BASELINE_ITEMProgressImportWrapper> selectedentities { get; set; }
        public ObservableCollection<BASELINE_ITEMProgressImportWrapper> SelectedEntities
        {
            get { return selectedentities; }
            set { selectedentities = value; }
        }

        BASELINE_ITEMProgressImportWrapper selectedEntity;
        public virtual BASELINE_ITEMProgressImportWrapper SelectedEntity
        {
            get => selectedEntity;
            set
            {
                selectedEntity = value;
            }
        }

        protected IMessageBoxService MessageBoxService
        {
            get { return this.GetRequiredService<IMessageBoxService>(); }
        }

        protected IOpenFileDialogService OpenFileDialogService
        {
            get { return this.GetService<IOpenFileDialogService>(); }
        }

        DispatcherTimer delayedRefreshTimer;
        IEnumerable<BASELINE_ITEMProgressImportWrapper> enumerableDocuments;

        protected ListImportDeliverableViewModel(IEnumerable<BASELINE_ITEMProgressImportWrapper> enumerableObjects, IEnumerable<PHASE> PHASECollection, IEnumerable<AREA> AREACollection, IEnumerable<DISCIPLINE> DISCIPLINECollection, IEnumerable<DOCTYPE> DOCTYPECollection, IEnumerable<DEPARTMENT> DEPARTMENTCollection)
        {
            SelectedEntities = new ObservableCollection<BASELINE_ITEMProgressImportWrapper>();
            SelectedEntities.CollectionChanged += SelectedEntities_CollectionChanged;
            enumerableDocuments = enumerableObjects;
            this.PHASECollection = PHASECollection;
            this.AREACollection = AREACollection;
            this.DISCIPLINECollection = DISCIPLINECollection;
            this.DOCTYPECollection = DOCTYPECollection;
            this.DEPARTMENTCollection = DEPARTMENTCollection;

            if(enumerableDocuments != null)
            {
                if(enumerableDocuments.Count() == 0)
                {
                    //workaround an issue where grid doesn't show anything subsequently if there's no object at binding time
                    //SourceObjects.Add(new BASELINE_ITEMProgressImportWrapper() { AttachmentName = infoObjectStr, AttachmentPath = infoObjectPathStr });
                }
                else
                {
                    foreach (BASELINE_ITEMProgressImportWrapper enumerableDocument in enumerableDocuments)
                    {
                        SourceObjects.Add(enumerableDocument);
                    }
                }
            }

            delayedRefreshTimer = new DispatcherTimer();
            delayedRefreshTimer.Interval = new TimeSpan(0, 0, 0, 1);
            delayedRefreshTimer.Tick += delayedRefreshTimer_Tick;
            delayedRefreshTimer.Start();
        }

        public List<BASELINE_ITEMProgressImportWrapper> GetSelectedDocuments()
        {
            List<BASELINE_ITEMProgressImportWrapper> returnDocuments = new List<BASELINE_ITEMProgressImportWrapper>();
            foreach(BASELINE_ITEMProgressImportWrapper sourceObject in SourceObjects)
            {
                //if (sourceObject.AttachmentName != BluePrintsResources.ReferenceInfoLineName && sourceObject.AttachmentName != null && sourceObject.AttachmentName != string.Empty)
                    returnDocuments.Add(sourceObject);
            }

            return returnDocuments;
        }

        private void SelectedEntities_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            
        }

        private void delayedRefreshTimer_Tick(object sender, System.EventArgs e)
        {
            if(GridControlService.GridControl != null && GridControlService.GridControl.ItemsSource != null)
            {
                delayedRefreshTimer.Stop();
            }
        }

        public void CloseDialog()
        {
            this.GetService<ICurrentDialogService>().Close();
        }

        protected virtual IFolderBrowserDialogService FolderBrowserDialogService { get { return this.GetService<IFolderBrowserDialogService>(); } }
        public virtual void ExportToExcel()
        {
            string ResultPath = string.Empty;
            if (FolderBrowserDialogService.ShowDialog())
            {
                ResultPath = FolderBrowserDialogService.ResultPath;
                bool result = TableViewService.ExportToXls(ResultPath + "\\DocumentsImportReport.xlsx", false);

                if (!result)
                    MessageBoxService.ShowMessage("Export failed because the file is in use", "Warning", MessageButton.OK, MessageIcon.Warning);
            }
        }

        public IEnumerable<PHASE> PHASECollection { get; set; }
        public IEnumerable<AREA> AREACollection { get; set; }
        public IEnumerable<DISCIPLINE> DISCIPLINECollection { get; set; }
        public IEnumerable<DOCTYPE> DOCTYPECollection { get; set; }
        public IEnumerable<DEPARTMENT> DEPARTMENTCollection { get; set; }
        public string AreaHeaderString => ColumnHeaderResources.AreaHeaderString;
        public string SubAreaHeaderString => ColumnHeaderResources.SubAreaHeaderString;
        public string DisciplineHeaderString => ColumnHeaderResources.DisciplineHeaderString;
        public string DisciplineNumberHeaderString => ColumnHeaderResources.DisciplineNumberHeaderString;
        public string DocumentTypeHeaderString => ColumnHeaderResources.DocumentTypeHeaderString;
        public string DeliverableTypeHeaderString => ColumnHeaderResources.DeliverableTypeHeaderString;
        public string DepartmentHeaderString => ColumnHeaderResources.DepartmentHeaderString;
        public string ClientNumberHeaderString => ColumnHeaderResources.ClientNumberHeaderString;
        public string PrimaryTitleHeaderString => ColumnHeaderResources.PrimaryTitleHeaderString;
        public string SecondaryTitleHeaderString => ColumnHeaderResources.SecondaryTitleHeaderString;
        public string CommentsHeaderString => ColumnHeaderResources.CommentsHeaderString;
        public string ResourceHeaderString => ColumnHeaderResources.ResourceHeaderString;
        public string SubJobHeaderString => ColumnHeaderResources.SubJobHeaderString;
        public string OfficeHeaderString => ColumnHeaderResources.OfficeHeaderString;
        public string PhaseHeaderString => ColumnHeaderResources.PhaseHeaderString;
        public string InternalNumberHeaderString => ColumnHeaderResources.InternalNumberHeaderString;
        public string CurrentPercentageHeaderString => ColumnHeaderResources.CurrentPercentageHeaderString;
    }
}