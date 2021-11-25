using BaseModel.Data.Helpers;
using BaseModel.ViewModel.Services;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Misc;
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
    public class ListImportDeliverableViewModel<T>
        where T : BASELINE_ITEMProgressImportWrapper
    {
        public static ListImportDeliverableViewModel<T> Create(IEnumerable<T> enumerableObjects, IEnumerable<PHASE> PHASECollection, IEnumerable<AREA> AREACollection, IEnumerable<DISCIPLINE> DISCIPLINECollection, IEnumerable<DOCTYPE> DOCTYPECollection, IEnumerable<DEPARTMENT> DEPARTMENTCollection)
        {
            return ViewModelSource.Create(() => new ListImportDeliverableViewModel<T>(enumerableObjects, PHASECollection, AREACollection, DISCIPLINECollection, DOCTYPECollection, DEPARTMENTCollection));
        }

        [ServiceProperty(Key = "DefaultTableViewService")]
        protected virtual ITableViewService TableViewService { get { return null; } }

        [ServiceProperty(Key = "DefaultGridControlService")]
        public virtual IGridControlService GridControlService { get { return null; } }

        ObservableCollection<T> sourceObjects;
        public int InternalNumSortIndex => 1;
        public ObservableCollection<T> SourceObjects
        {
            get
            {
                if (sourceObjects == null)
                    sourceObjects = new ObservableCollection<T>();

                return sourceObjects;
            }
        }

        public string Message { get; set; }

        protected ObservableCollection<T> selectedentities { get; set; }
        public ObservableCollection<T> SelectedEntities
        {
            get { return selectedentities; }
            set { selectedentities = value; }
        }

        T selectedEntity;
        public virtual T SelectedEntity
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
        IEnumerable<T> enumerableDocuments;

        protected ListImportDeliverableViewModel(IEnumerable<T> enumerableObjects, IEnumerable<PHASE> PHASECollection, IEnumerable<AREA> AREACollection, IEnumerable<DISCIPLINE> DISCIPLINECollection, IEnumerable<DOCTYPE> DOCTYPECollection, IEnumerable<DEPARTMENT> DEPARTMENTCollection)
        {
            SelectedEntities = new ObservableCollection<T>();
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
                    foreach (T enumerableDocument in enumerableDocuments)
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

        private void SelectedEntities_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            
        }

        private void delayedRefreshTimer_Tick(object sender, System.EventArgs e)
        {
            if(GridControlService.GridControl != null && GridControlService.GridControl.ItemsSource != null)
            {
                delayedRefreshTimer.Stop();
                GridControlService.RefreshData();
                TableViewService.ApplyBestFit();
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
                bool result = TableViewService.ExportToXls(ResultPath + "\\DocumentsImportReport.xlsx", true);

                if (!result)
                    MessageBoxService.ShowMessage("Export failed because the file is in use", "Warning", MessageButton.OK, MessageIcon.Warning);
            }
        }

        public bool CanCheckSelected()
        {
            return SelectedEntities.Count > 0;
        }

        public void CheckSelected()
        {
            foreach(T selectedDeliverable in SelectedEntities)
            {
                if (selectedDeliverable.CanImport)
                {
                    selectedDeliverable.Import = true;
                    selectedDeliverable.RaisePropertyChanged(x => x.Import);
                }
            }
        }

        public bool CanUncheckSelected()
        {
            return CanCheckSelected();
        }

        public void UncheckSelected()
        {
            foreach (T selectedDeliverable in SelectedEntities)
            {
                selectedDeliverable.Import = false;
                selectedDeliverable.RaisePropertyChanged(x => x.Import);
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
        public string BudgetHourHeaderString => ColumnHeaderResources.BudgetHourHeaderString;
    }
}