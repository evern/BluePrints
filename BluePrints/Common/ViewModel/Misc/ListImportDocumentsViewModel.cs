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
    public class ListImportDocumentsViewModel<T>
        where T : IAmAttachmentPath, new()
    {
        public static ListImportDocumentsViewModel<T> Create(IEnumerable<T> enumerableObjects)
        {
            return ViewModelSource.Create(() => new ListImportDocumentsViewModel<T>(enumerableObjects));
        }

        [ServiceProperty(Key = "DefaultTableViewService")]
        protected virtual ITableViewService TableViewService { get { return null; } }

        [ServiceProperty(Key = "DefaultGridControlService")]
        public virtual IGridControlService GridControlService { get { return null; } }

        ObservableCollection<T> sourceObjects;
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
        string infoObjectStr = BluePrintsResources.ReferenceInfoLineName;
        string infoObjectPathStr = "Press click import files or add empty row and manually enter details to add custom reference";

        protected ListImportDocumentsViewModel(IEnumerable<T> enumerableObjects)
        {
            SelectedEntities = new ObservableCollection<T>();
            SelectedEntities.CollectionChanged += SelectedEntities_CollectionChanged;
            enumerableDocuments = enumerableObjects;
            if(enumerableDocuments != null)
            {
                if(enumerableDocuments.Count() == 0)
                {
                    //workaround an issue where grid doesn't show anything subsequently if there's no object at binding time
                    SourceObjects.Add(new T() { AttachmentName = infoObjectStr, AttachmentPath = infoObjectPathStr });
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

        public List<T> GetSelectedDocuments()
        {
            List<T> returnDocuments = new List<T>();
            foreach(T sourceObject in SourceObjects)
            {
                if (sourceObject.AttachmentName != BluePrintsResources.ReferenceInfoLineName && sourceObject.AttachmentName != null && sourceObject.AttachmentName != string.Empty)
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
                refreshGrid();
            }
        }

        public void CloseDialog()
        {
            this.GetService<ICurrentDialogService>().Close();
        }

        public virtual void DeleteFiles()
        {
            List<T> removeEntities = new List<T>();
            removeEntities.AddRange(SelectedEntities);
            for(int i = 0;i < removeEntities.Count;i++)
            {
                T document = removeEntities[i];
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
                    SourceObjects.Add(new T() { AttachmentPath = file.GetFullName(), AttachmentName = file.Name });
                }
            }

            refreshGrid();
        }
    }
}