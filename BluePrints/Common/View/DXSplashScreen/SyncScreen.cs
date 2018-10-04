using BluePrints.View;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Core;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Linq;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using BaseModel.Data.Helpers;
using System;
using BluePrints.Data;
using BaseModel.Misc;
using System.Data.Entity;
using System.Threading;
using System.Windows.Threading;

namespace BluePrints.Common
{
    public class SyncInfo
    {
        public string Database { get; set; }
        public decimal CurrentLocalProgress { get; set; }
        public decimal CurrentRemoteProgress { get; set; }
        public decimal MaxLocalProgress { get; set; }
        public decimal MaxRemoteProgress { get; set; }
        public decimal LocalProgress => MaxLocalProgress == 0 ? 0 : CurrentLocalProgress / MaxLocalProgress;
        public decimal RemoteProgress => MaxRemoteProgress == 0 ? 0 : CurrentRemoteProgress / MaxRemoteProgress;

        public void Update()
        {
            this.RaisePropertiesChanged();
        }
    }

    public class SyncReport
    {
        public string TableName { get; set; }
        public string Project { get; set; }
        public string Properties { get; set; }
        public string Destination { get; set; }
        public string Action { get; set; }

        public void Update()
        {
            this.RaisePropertiesChanged();
        }
    }

    public class SyncScreenViewModel
    {
        public static SyncScreenViewModel Create()
        {
            return ViewModelSource.Create(() => new SyncScreenViewModel());
        }

        protected Dispatcher mainDispatcher = Application.Current.Dispatcher;
        public SyncScreenViewModel()
        {
            SyncInfos = new ObservableCollection<SyncInfo>();
            SyncReport = new ObservableCollection<SyncReport>();
            SyncData();
        }

        public ObservableCollection<SyncInfo> SyncInfos { get; set; }
        public ObservableCollection<SyncReport> SyncReport { get; set; }
        public void ResetCurrentProgress(string databaseTypeName)
        {
            SyncInfos.Clear();
        }

        private void threadSafeSetMaxLocalProgress(string databaseTypeName, decimal maxProgress)
        {
            mainDispatcher.BeginInvoke(new Action(() => setMaxLocalProgress(databaseTypeName, maxProgress)));
        }

        private void setMaxLocalProgress(string databaseTypeName, decimal maxProgress)
        {
            SyncInfo syncInfo = SyncInfos.FirstOrDefault(x => x.Database == databaseTypeName);
            if (syncInfo != null)
            {
                syncInfo.CurrentLocalProgress = 0;
                syncInfo.MaxLocalProgress = maxProgress;
            }
            else
                SyncInfos.Add(ViewModelSource.Create(() => new SyncInfo() { Database = databaseTypeName, MaxLocalProgress = maxProgress }));

            this.RaisePropertyChanged(x => x.SyncInfos);
        }

        private void threadSafeSetMaxRemoteProgress(string databaseTypeName, decimal maxProgress)
        {
            mainDispatcher.BeginInvoke(new Action(() => setMaxRemoteProgress(databaseTypeName, maxProgress)));
        }

        private void setMaxRemoteProgress(string databaseTypeName, decimal maxProgress)
        {
            SyncInfo syncInfo = SyncInfos.FirstOrDefault(x => x.Database == databaseTypeName);
            if (syncInfo != null)
            {
                syncInfo.CurrentRemoteProgress = 0;
                syncInfo.MaxRemoteProgress = maxProgress;
            }
            else
                SyncInfos.Add(ViewModelSource.Create(() => new SyncInfo() { Database = databaseTypeName, MaxRemoteProgress = maxProgress }));

            this.RaisePropertyChanged(x => x.SyncInfos);
        }

        private void threadSafeProgressLocal(string databaseTypeName, decimal progress)
        {
            mainDispatcher.BeginInvoke(new Action(() => progressLocal(databaseTypeName, progress)));
        }

        private void progressLocal(string databaseTypeName, decimal progress)
        {
            SyncInfo syncInfo = SyncInfos.FirstOrDefault(x => x.Database == databaseTypeName);
            if (syncInfo != null)
            {
                syncInfo.CurrentLocalProgress += progress;
                syncInfo.Update();
            }
        }

        private void threadSafeProgressRemote(string databaseTypeName, decimal progress)
        {
            mainDispatcher.BeginInvoke(new Action(() => progressRemote(databaseTypeName, progress)));
        }

        private void progressRemote(string databaseTypeName, decimal progress)
        {
            SyncInfo syncInfo = SyncInfos.FirstOrDefault(x => x.Database == databaseTypeName);
            if (syncInfo != null)
            {
                syncInfo.CurrentRemoteProgress += progress;
                syncInfo.Update();
            }
        }

        private void threadSafeAddSyncReport(SyncReport syncReport)
        {
            mainDispatcher.BeginInvoke(new Action(() => addSyncReport(syncReport)));
        }

        private void addSyncReport(SyncReport syncReport)
        {
            SyncReport newSyncReport = ViewModelSource.Create(() => new SyncReport());
            DataUtils.ShallowCopy(newSyncReport, syncReport);
            SyncReport.Add(newSyncReport);
            this.RaisePropertyChanged(x => x.SyncReport);
        }

        public void SyncData()
        {
            QueueWorkItem<BASELINE>();
            QueueWorkItem<BASELINE_ITEM>();
            QueueWorkItem<BASELINE_ITEM_WORK>();
            QueueWorkItem<CLIENT>();
            QueueWorkItem<COMMODITY_CODE>();
            QueueWorkItem<CONSTRUCTION_CONFIG>();
            QueueWorkItem<DAYWORK>();
            QueueWorkItem<DAYWORK_EQUIPMENT>();
            QueueWorkItem<DAYWORK_MATERIAL>();
            QueueWorkItem<DAYWORK_LABOUR>();
            QueueWorkItem<DAYWORK_STAFF_ROLE>();
            QueueWorkItem<DELIVERABLES_STATUS>();
            QueueWorkItem<DEPARTMENT>();
            QueueWorkItem<DISCIPLINE>();
            QueueWorkItem<DOCTYPE>();
            QueueWorkItem<ESTIMATE>();
            QueueWorkItem<ESTIMATE_ITEM>();
            QueueWorkItem<FORECAST>();
            QueueWorkItem<HSE>();
            QueueWorkItem<HSE_INCIDENT>();
            QueueWorkItem<HSE_INJURY>();
            QueueWorkItem<HOLIDAY>();
            QueueWorkItem<MEETING>();
            QueueWorkItem<MEETING_USER>();
            QueueWorkItem<MINUTE_AGENDA>();
            QueueWorkItem<MINUTE_TITLE>();
            QueueWorkItem<P6_ASSIGNMENT>();
            QueueWorkItem<PHASE>();
            QueueWorkItem<PROGRESS>();
            QueueWorkItem<PROGRESS_ITEM>();
            QueueWorkItem<PROJECT>();
            QueueWorkItem<PROJECT_DISCIPLINE>();
            QueueWorkItem<PROJECT_REPORT>();
            QueueWorkItem<RA_GUIDE_PROMPT>();
            QueueWorkItem<RA_GUIDE_SUBPROMPT>();
            QueueWorkItem<RA_STUDY>();
            QueueWorkItem<RA_STUDY_DATA>();
            QueueWorkItem<RA_STUDY_DRAWING>();
            QueueWorkItem<RA_STUDY_NODE>();
            QueueWorkItem<RA_STUDY_TYPE>();
            QueueWorkItem<RA_STUDY_TEAM>();
            QueueWorkItem<RATE>();
            QueueWorkItem<REGISTER>();
            QueueWorkItem<REGISTER_CHANGE>();
            QueueWorkItem<REGISTER_HOLD>();
            QueueWorkItem<REGISTER_HOLD_REF>();
            QueueWorkItem<REGISTER_ISSUE>();
            QueueWorkItem<REGISTER_LL>();
            QueueWorkItem<REGISTER_NC>();
            QueueWorkItem<REGISTER_RISK>();
            QueueWorkItem<ROLE>();
            QueueWorkItem<ROLE_PERMISSION>();
            QueueWorkItem<ROLE_COMMODITY>();
            QueueWorkItem<ROSTER_STAFF>();
            QueueWorkItem<ROSTER_STAFF_STATUS>();
            QueueWorkItem<SETTINGS_GLOBAL>();
            QueueWorkItem<STOCK_CODE>();
            QueueWorkItem<STOCK_GROUP>();
            QueueWorkItem<UOM>();
            QueueWorkItem<USER>();
            QueueWorkItem<VARIATION>();
            QueueWorkItem<VARIATION_ITEM>();
            QueueWorkItem<SUBJOB>();
            QueueWorkItem<SUBJOB_ASSIGNMENT>();
            QueueWorkItem<VARIATION_REGISTER>();
        }

        private void QueueWorkItem<T>()
            where T : class, ICanSync, new()
        {
            if(typeof(T) == typeof(BASELINE_ITEM))
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(processBaselineItemDbSet), new object[] { new Action<string, decimal>(threadSafeSetMaxLocalProgress), new Action<string, decimal>(threadSafeSetMaxRemoteProgress), new Action<string, decimal>(threadSafeProgressLocal), new Action<string, decimal>(threadSafeProgressRemote), new Action<SyncReport>(threadSafeAddSyncReport), true });
                ThreadPool.QueueUserWorkItem(new WaitCallback(processBaselineItemDbSet), new object[] { new Action<string, decimal>(threadSafeSetMaxLocalProgress), new Action<string, decimal>(threadSafeSetMaxRemoteProgress), new Action<string, decimal>(threadSafeProgressLocal), new Action<string, decimal>(threadSafeProgressRemote), new Action<SyncReport>(threadSafeAddSyncReport), false });
            }
            else
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(processDbSet<T>), new object[] { new Action<string, decimal>(threadSafeSetMaxLocalProgress), new Action<string, decimal>(threadSafeSetMaxRemoteProgress), new Action<string, decimal>(threadSafeProgressLocal), new Action<string, decimal>(threadSafeProgressRemote), new Action<SyncReport>(threadSafeAddSyncReport), true });
                ThreadPool.QueueUserWorkItem(new WaitCallback(processDbSet<T>), new object[] { new Action<string, decimal>(threadSafeSetMaxLocalProgress), new Action<string, decimal>(threadSafeSetMaxRemoteProgress), new Action<string, decimal>(threadSafeProgressLocal), new Action<string, decimal>(threadSafeProgressRemote), new Action<SyncReport>(threadSafeAddSyncReport), false });
            }
        }

        private void processBaselineItemDbSet(object state)
        {
            object[] parameters = (object[])state;
            Action<string, decimal> setupLocalAction = (Action<string, decimal>)parameters[0];
            Action<string, decimal> setupRemoteAction = (Action<string, decimal>)parameters[1];
            Action<string, decimal> progressLocalAction = (Action<string, decimal>)parameters[2];
            Action<string, decimal> progressRemoteAction = (Action<string, decimal>)parameters[3];
            Action<SyncReport> finishAction = (Action<SyncReport>)parameters[4];
            bool isLocal = (bool)parameters[5];

            BluePrintsNativeEntities localDataContext = new BluePrintsNativeEntities("name=BluePrintsLocalEntities");
            BluePrintsNativeEntities remoteDataContext = new BluePrintsNativeEntities("name=BluePrintsRemoteEntities");
            var localDbSet = localDataContext.BASELINE_ITEM;
            var remoteDbSet = remoteDataContext.BASELINE_ITEM;

            //List<BASELINE_ITEM> remoteDbSetList = remoteDbSet.ToList();
            string typeName = typeof(BASELINE_ITEM).Name.ToString();

            decimal localCount = localDbSet.Count();
            //includes 20% for datacontext save
            decimal localDataContextCount = localCount * 0.2m;
            decimal localTotalCount = localCount + localDataContextCount;

            decimal remoteCount = remoteDbSet.Count();
            //includes 20% for datacontext save
            decimal remoteDataContextCount = remoteCount * 0.2m;
            decimal remoteTotalCount = remoteCount + remoteDataContextCount;
            if (isLocal)
            {
                if (localTotalCount == 0)
                {
                    setupLocalAction(typeName, 1);
                    progressLocalAction(typeName, 1);
                }
                else
                {
                    setupLocalAction(typeName, localTotalCount);

                    foreach (var localData in localDbSet)
                    {
                        bool? syncRemote = null;
                        bool? deleteRemote = null;
                        DateTime localCreated = localData.CREATED;
                        DateTime? localUpdated = localData.UPDATED;
                        DateTime? localDeleted = localData.DELETED;

                        BASELINE_ITEM remoteData = remoteDbSet.FirstOrDefault(x => x.GUID == localData.GUID);
                        if (remoteData != null)
                        {
                            DateTime remoteCreated = remoteData.CREATED;
                            DateTime? remoteUpdated = remoteData.UPDATED;
                            DateTime? remoteDeleted = remoteData.DELETED;

                            //Delete
                            if (remoteDeleted != null && localDeleted != null)
                            {
                                if (remoteDeleted > localDeleted)
                                    deleteRemote = true;
                                else if (remoteDeleted < localDeleted)
                                    deleteRemote = false;
                            }
                            else if (remoteDeleted != null && localDeleted == null)
                            {
                                deleteRemote = true;
                            }
                            else if (remoteDeleted == null && localDeleted != null)
                            {
                                deleteRemote = false;
                            }

                            //Update
                            else if (remoteUpdated != null && localUpdated != null)
                            {
                                if (remoteUpdated > localUpdated)
                                    syncRemote = true;
                                else if (remoteUpdated < localUpdated)
                                    syncRemote = false;
                            }
                            else if (remoteUpdated != null && localUpdated == null)
                            {
                                syncRemote = true;
                            }
                            else if (remoteUpdated == null && localUpdated != null)
                            {
                                syncRemote = false;
                            }

                            //Created
                            if (remoteCreated > localCreated)
                                syncRemote = true;
                            else if (remoteCreated < localCreated)
                                syncRemote = false;


                            if (syncRemote == true)
                            {
                                finishAction(new SyncReport() { Action = "Update", Destination = "Local", Properties = DataUtils.ShallowCopyDiffTracking(localData, remoteData), Project = getProjectNumber(localData), TableName = typeName });
                            }
                            else if (syncRemote == false)
                            {
                                finishAction(new SyncReport() { Action = "Update", Destination = "Remote", Properties = DataUtils.ShallowCopyDiffTracking(remoteData, localData), Project = getProjectNumber(remoteData), TableName = typeName });
                            }

                            if (deleteRemote == true)
                            {
                                DataUtils.ShallowCopy(localData, remoteData);
                                finishAction(new SyncReport() { Action = "Delete", Destination = "Local", Properties = string.Empty, Project = getProjectNumber(localData), TableName = typeName });
                            }
                            else if (deleteRemote == false)
                            {
                                DataUtils.ShallowCopy(remoteData, localData);
                                finishAction(new SyncReport() { Action = "Delete", Destination = "Remote", Properties = string.Empty, Project = getProjectNumber(remoteData), TableName = typeName });
                            }
                        }
                        else
                        {
                            BASELINE_ITEM remoteNewData = new BASELINE_ITEM();
                            DataUtils.ShallowCopy(remoteNewData, localData);
                            remoteDbSet.Add(remoteNewData);
                            finishAction(new SyncReport() { Action = "Add", Destination = "Remote", Properties = string.Empty, Project = getProjectNumber(localData), TableName = typeName });
                        }

                        progressLocalAction(typeName, 1);
                    }

                    localDataContext.SaveChanges();
                    remoteDataContext.SaveChanges();
                    progressLocalAction(typeName, localDataContextCount);
                }
            }
            else
            {
                if (remoteTotalCount == 0)
                {
                    setupRemoteAction(typeName, 1);
                    progressRemoteAction(typeName, 1);
                }
                else
                {
                    setupRemoteAction(typeName, remoteTotalCount);
                    //List<BASELINE_ITEM> localDbSetList = localDbSet.ToList();
                    foreach (var remoteData in remoteDbSet)
                    {
                        BASELINE_ITEM localData = localDbSet.FirstOrDefault(x => x.GUID == remoteData.GUID);
                        if (localData == null)
                        {
                            BASELINE_ITEM localNewData = new BASELINE_ITEM();
                            DataUtils.ShallowCopy(localNewData, remoteData);
                            localDbSet.Add(localNewData);
                            finishAction(new SyncReport() { Action = "Add", Destination = "Local", Properties = string.Empty, Project = getProjectNumber(remoteData), TableName = typeName });
                        }

                        progressRemoteAction(typeName, 1);
                    }

                    localDataContext.SaveChanges();
                    progressRemoteAction(typeName, remoteDataContextCount);
                }
            }

            localDataContext.Dispose();
            remoteDataContext.Dispose();
        }

        private string getProjectNumber(BASELINE_ITEM baselineItem)
        {
            if (baselineItem.BASELINE != null)
                return "Project " + baselineItem.BASELINE.PROJECT.NUMBER + " " + baselineItem.BASELINE.PROJECT.OFFICE.NAME;
            else if (baselineItem.VARIATION != null)
                return "Variation " + baselineItem.VARIATION.PROJECT.NUMBER + " " + baselineItem.VARIATION.PROJECT.OFFICE.NAME;

            return string.Empty;
        }

        private string compareAndCopyBaselineItem(BASELINE_ITEM copyItem, BASELINE_ITEM compareItem)
        {
            string propertyDiff = string.Empty;
            

            return propertyDiff;
        }

        private void processDbSet<T>(object state)
            where T : class, ICanSync, new()
        {
            object[] parameters = (object[])state;
            Action<string, decimal> setupLocalAction = (Action<string, decimal>)parameters[0];
            Action<string, decimal> setupRemoteAction = (Action<string, decimal>)parameters[1];
            Action<string, decimal> progressLocalAction = (Action<string, decimal>)parameters[2];
            Action<string, decimal> progressRemoteAction = (Action<string, decimal>)parameters[3];
            Action<SyncReport> finishAction = (Action<SyncReport>)parameters[4];
            bool isLocal = (bool)parameters[5];

            BluePrintsNativeEntities localDataContext = new BluePrintsNativeEntities("name=BluePrintsLocalEntities");
            BluePrintsNativeEntities remoteDataContext = new BluePrintsNativeEntities("name=BluePrintsRemoteEntities");
            DbSet<T> localDbSet = localDataContext.Set<T>();
            DbSet<T> remoteDbSet = remoteDataContext.Set<T>();
            //List<T> remoteDbSetList = remoteDbSet.ToList();
            string typeName = typeof(T).Name.ToString();

            decimal localCount = localDbSet.Count();
            //includes 20% for datacontext save
            decimal localDataContextCount = localCount * 0.2m;
            decimal localTotalCount = localCount + localDataContextCount;

            decimal remoteCount = remoteDbSet.Count();
            //includes 20% for datacontext save
            decimal remoteDataContextCount = remoteCount * 0.2m;
            decimal remoteTotalCount = remoteCount + remoteDataContextCount;
            if (isLocal)
            {
                if(localTotalCount == 0)
                {
                    setupLocalAction(typeName, 1);
                    progressLocalAction(typeName, 1);
                }
                else
                {
                    setupLocalAction(typeName, localTotalCount);

                    foreach (var localData in localDbSet)
                    {
                        bool? syncRemote = null;
                        bool? deleteRemote = null;
                        DateTime localCreated = localData.CREATED;
                        DateTime? localUpdated = localData.UPDATED;
                        DateTime? localDeleted = localData.DELETED;

                        T remoteData = remoteDbSet.FirstOrDefault(x => x.GUID == localData.GUID);
                        if (remoteData != null)
                        {
                            DateTime remoteCreated = remoteData.CREATED;
                            DateTime? remoteUpdated = remoteData.UPDATED;
                            DateTime? remoteDeleted = remoteData.DELETED;

                            //Delete
                            if (remoteDeleted != null && localDeleted != null)
                            {
                                if (remoteDeleted > localDeleted)
                                    deleteRemote = true;
                                else if (remoteDeleted < localDeleted)
                                    deleteRemote = false;
                            }
                            else if (remoteDeleted != null && localDeleted == null)
                            {
                                deleteRemote = true;
                            }
                            else if (remoteDeleted == null && localDeleted != null)
                            {
                                deleteRemote = false;
                            }

                            //Update
                            else if (remoteUpdated != null && localUpdated != null)
                            {
                                if (remoteUpdated > localUpdated)
                                    syncRemote = true;
                                else if (remoteUpdated < localUpdated)
                                    syncRemote = false;
                            }
                            else if (remoteUpdated != null && localUpdated == null)
                            {
                                syncRemote = true;
                            }
                            else if (remoteUpdated == null && localUpdated != null)
                            {
                                syncRemote = false;
                            }

                            //Created
                            if (remoteCreated > localCreated)
                                syncRemote = true;
                            else if (remoteCreated < localCreated)
                                syncRemote = false;


                            if (syncRemote == true)
                            {
                                finishAction(new SyncReport() { Action = "Update", Destination = "Local", Properties = DataUtils.ShallowCopyDiffTracking(localData, remoteData), TableName = typeName });
                            }
                            else if (syncRemote == false)
                            {
                                finishAction(new SyncReport() { Action = "Update", Destination = "Remote", Properties = DataUtils.ShallowCopyDiffTracking(remoteData, localData), TableName = typeName });
                            }

                            if(deleteRemote == true)
                            {
                                DataUtils.ShallowCopy(localData, remoteData);
                                finishAction(new SyncReport() { Action = "Delete", Destination = "Local", Properties = string.Empty, TableName = typeName });
                            }
                            else if(deleteRemote == false)
                            {
                                DataUtils.ShallowCopy(remoteData, localData);
                                finishAction(new SyncReport() { Action = "Delete", Destination = "Remote", Properties = string.Empty, TableName = typeName });
                            }
                        }
                        else
                        {
                            T remoteNewData = new T();
                            remoteDbSet.Add(remoteNewData);
                            DataUtils.ShallowCopy(remoteNewData, localData);
                            finishAction(new SyncReport() { Action = "Add", Destination = "Remote", Properties = string.Empty, TableName = typeName });
                        }

                        progressLocalAction(typeName, 1);
                    }

                    localDataContext.SaveChanges();
                    remoteDataContext.SaveChanges();
                    progressLocalAction(typeName, localDataContextCount);
                }
            }
            else
            {
                if (remoteTotalCount == 0)
                {
                    setupRemoteAction(typeName, 1);
                    progressRemoteAction(typeName, 1);
                }
                else
                {
                    setupRemoteAction(typeName, remoteTotalCount);
                    //List<T> localDbSetList = localDbSet.ToList();
                    foreach (var remoteData in remoteDbSet)
                    {
                        T localData = localDbSet.FirstOrDefault(x => x.GUID == remoteData.GUID);
                        if (localData == null)
                        {
                            T localNewData = new T();
                            localDbSet.Add(localNewData);
                            DataUtils.ShallowCopy(localNewData, remoteData);
                            finishAction(new SyncReport() { Action = "Add", Destination = "Local", Properties = string.Empty, TableName = typeName });
                        }

                        progressRemoteAction(typeName, 1);
                    }

                    localDataContext.SaveChanges();
                    progressRemoteAction(typeName, remoteDataContextCount);
                }
            }

            localDataContext.Dispose();
            remoteDataContext.Dispose();
        }
    }
}
