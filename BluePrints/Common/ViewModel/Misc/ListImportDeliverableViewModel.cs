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
    public class ListImportDeliverableViewModel<BASELINE_ITEMProgress>
    {
        public static ListImportDeliverableViewModel<BASELINE_ITEMProgress> Create(IEnumerable<BASELINE_ITEMProgress> enumerableObjects)
        {
            return ViewModelSource.Create(() => new ListImportDeliverableViewModel<BASELINE_ITEMProgress>(enumerableObjects));
        }

        [ServiceProperty(Key = "DefaultTableViewService")]
        protected virtual ITableViewService TableViewService { get { return null; } }

        [ServiceProperty(Key = "DefaultGridControlService")]
        public virtual IGridControlService GridControlService { get { return null; } }

        ObservableCollection<BASELINE_ITEMProgress> sourceObjects;
        public ObservableCollection<BASELINE_ITEMProgress> SourceObjects
        {
            get
            {
                if (sourceObjects == null)
                    sourceObjects = new ObservableCollection<BASELINE_ITEMProgress>();

                return sourceObjects;
            }
        }

        public string Message { get; set; }

        protected ObservableCollection<BASELINE_ITEMProgress> selectedentities { get; set; }
        public ObservableCollection<BASELINE_ITEMProgress> SelectedEntities
        {
            get { return selectedentities; }
            set { selectedentities = value; }
        }

        BASELINE_ITEMProgress selectedEntity;
        public virtual BASELINE_ITEMProgress SelectedEntity
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
        IEnumerable<BASELINE_ITEMProgress> enumerableDocuments;
        string infoObjectStr = BluePrintsResources.ReferenceInfoLineName;
        string infoObjectPathStr = "Press click import files or add empty row and manually enter details to add custom reference";

        protected ListImportDeliverableViewModel(IEnumerable<BASELINE_ITEMProgress> enumerableObjects)
        {
            SelectedEntities = new ObservableCollection<BASELINE_ITEMProgress>();
            SelectedEntities.CollectionChanged += SelectedEntities_CollectionChanged;
            enumerableDocuments = enumerableObjects;
            if(enumerableDocuments != null)
            {
                if(enumerableDocuments.Count() == 0)
                {
                    //workaround an issue where grid doesn't show anything subsequently if there's no object at binding time
                    //SourceObjects.Add(new BASELINE_ITEMProgress() { AttachmentName = infoObjectStr, AttachmentPath = infoObjectPathStr });
                }
                else
                {
                    foreach (BASELINE_ITEMProgress enumerableDocument in enumerableDocuments)
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

        public List<BASELINE_ITEMProgress> GetSelectedDocuments()
        {
            List<BASELINE_ITEMProgress> returnDocuments = new List<BASELINE_ITEMProgress>();
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
            }
        }

        public void CloseDialog()
        {
            this.GetService<ICurrentDialogService>().Close();
        }
    }
}