using BaseModel.Data.Helpers;
using BaseModel.ViewModel.Services;
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
    public class ListImportDocumentsViewModel
    {
        public static ListImportDocumentsViewModel Create(IEnumerable<REGISTER_TQ_ATTACHMENT> enumerableObjects)
        {
            return ViewModelSource.Create(() => new ListImportDocumentsViewModel(enumerableObjects));
        }

        [ServiceProperty(Key = "DefaultTableViewService")]
        protected virtual ITableViewService TableViewService { get { return null; } }

        [ServiceProperty(Key = "DefaultGridControlService")]
        public virtual IGridControlService GridControlService { get { return null; } }

        ObservableCollection<REGISTER_TQ_ATTACHMENT> sourceObjects;
        public ObservableCollection<REGISTER_TQ_ATTACHMENT> SourceObjects
        {
            get
            {
                if (sourceObjects == null)
                    sourceObjects = new ObservableCollection<REGISTER_TQ_ATTACHMENT>();

                return sourceObjects;
            }
        }

        public string Message { get; set; }

        protected ObservableCollection<REGISTER_TQ_ATTACHMENT> selectedentities { get; set; }
        public ObservableCollection<REGISTER_TQ_ATTACHMENT> SelectedEntities
        {
            get { return selectedentities; }
            set { selectedentities = value; }
        }

        REGISTER_TQ_ATTACHMENT selectedEntity;
        public virtual REGISTER_TQ_ATTACHMENT SelectedEntity
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
        IEnumerable<REGISTER_TQ_ATTACHMENT> enumerableDocuments;
        protected ListImportDocumentsViewModel(IEnumerable<REGISTER_TQ_ATTACHMENT> enumerableObjects)
        {
            SelectedEntities = new ObservableCollection<REGISTER_TQ_ATTACHMENT>();
            SelectedEntities.CollectionChanged += SelectedEntities_CollectionChanged;
            enumerableDocuments = enumerableObjects;
            if(enumerableDocuments != null)
                foreach (REGISTER_TQ_ATTACHMENT enumerableDocument in enumerableDocuments)
                {
                    SourceObjects.Add(enumerableDocument);
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
                refreshGrid();
            }
        }

        public void CloseDialog()
        {
            this.GetService<ICurrentDialogService>().Close();
        }

        public virtual void DeleteFiles()
        {
            List<REGISTER_TQ_ATTACHMENT> removeEntities = new List<REGISTER_TQ_ATTACHMENT>();
            removeEntities.AddRange(SelectedEntities);
            for(int i = 0;i < removeEntities.Count;i++)
            {
                REGISTER_TQ_ATTACHMENT document = removeEntities[i];
                SourceObjects.Remove(document);
            }

            refreshGrid();
        }

        /// <summary>
        /// workaround a bug where grid doesn't refresh when first loaded and when items are removed
        /// </summary>
        private void refreshGrid()
        {
            GridControlService.GridControl.ItemsSource = null;
            GridControlService.GridControl.ItemsSource = SourceObjects;
            GridControlService.RefreshData();
        }

        public virtual void ManualAddFile()
        {
            TableViewService.AddNewRow();
            refreshGrid();
        }

        public virtual void ImportFiles()
        {
            string ResultPath = string.Empty;
            if (OpenFileDialogService.ShowDialog())
            {
                foreach(IFileInfo file in OpenFileDialogService.Files)
                {
                    SourceObjects.Add(new REGISTER_TQ_ATTACHMENT() { ATTACHMENT_PATH = file.GetFullName(), ATTACHMENT_NAME = file.Name });
                }
            }

            refreshGrid();
        }
    }
}