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
using BluePrints.Common.Resources;
using System.Threading.Tasks;
using System.ComponentModel;
using BluePrints.Common.ViewModel.Utils;

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

        DispatcherTimer syncDispatcher;
        public SyncScreenViewModel()
        {
            SyncInfos = new ObservableCollection<SyncInfo>();
            SyncReport = new ObservableCollection<SyncReport>();
            syncDispatcher = new DispatcherTimer();
            syncDispatcher.Interval = new TimeSpan(0, 0, 0, 1);
            syncDispatcher.Tick += SyncDispatcher_Tick;
            syncDispatcher.Start();
        }

        private void SyncDispatcher_Tick(object sender, EventArgs e)
        {
            syncDispatcher.Stop();
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

        private void updateQueue()
        {
            if(qt.Count > 0)
            {
                Thread queuedThread = qt.Dequeue();
                queuedThread.Start();
            }
        }

        Queue<Thread> qt;
        public void SyncData()
        {
            //ThreadPool.SetMaxThreads(1, 1);
            qt = new Queue<Thread>();
            qt.Enqueue(createThread<PROJECT>());
            qt.Enqueue(createThread<AREA>());
            qt.Enqueue(createThread<PROJECT>());
            qt.Enqueue(createThread<AREA>());
            qt.Enqueue(createThread<WORKPACK>());
            qt.Enqueue(createThread<BASELINE>());
            qt.Enqueue(createThread<PROGRESS>());
            qt.Enqueue(createThread<VARIATION>());
            qt.Enqueue(createThread<ESTIMATE>());
            qt.Enqueue(createThread<SUBJOB>());
            qt.Enqueue(createThread<PROGRESS_ITEM>());
            qt.Enqueue(createThread<BASELINE_ITEM_WORK>());
            qt.Enqueue(createThread<CLIENT>());
            qt.Enqueue(createThread<COMMODITY_CODE>());
            qt.Enqueue(createThread<CONSTRUCTION_CONFIG>());
            qt.Enqueue(createThread<DAYWORK>());
            qt.Enqueue(createThread<DAYWORK_EQUIPMENT>());
            qt.Enqueue(createThread<DAYWORK_MATERIAL>());
            qt.Enqueue(createThread<DAYWORK_LABOUR>());
            qt.Enqueue(createThread<DAYWORK_STAFF_ROLE>());
            qt.Enqueue(createThread<DELIVERABLES_STATUS>());
            qt.Enqueue(createThread<DEPARTMENT>());
            qt.Enqueue(createThread<DISCIPLINE>());
            qt.Enqueue(createThread<DOCTYPE>());
            qt.Enqueue(createThread<ESTIMATE_ITEM>());
            qt.Enqueue(createThread<FORECAST>());
            qt.Enqueue(createThread<HSE>());
            qt.Enqueue(createThread<HSE_INCIDENT>());
            qt.Enqueue(createThread<HSE_INJURY>());
            qt.Enqueue(createThread<HOLIDAY>());
            qt.Enqueue(createThread<MEETING>());
            qt.Enqueue(createThread<MEETING_USER>());
            qt.Enqueue(createThread<MINUTE_AGENDA>());
            qt.Enqueue(createThread<MINUTE_TITLE>());
            qt.Enqueue(createThread<P6_ASSIGNMENT>());
            qt.Enqueue(createThread<PHASE>());
            qt.Enqueue(createThread<PROJECT_DISCIPLINE>());
            qt.Enqueue(createThread<PROJECT_REPORT>());
            qt.Enqueue(createThread<RA_GUIDE_PROMPT>());
            qt.Enqueue(createThread<RA_GUIDE_SUBPROMPT>());
            qt.Enqueue(createThread<RA_STUDY>());
            qt.Enqueue(createThread<RA_STUDY_DATA>());
            qt.Enqueue(createThread<RA_STUDY_DRAWING>());
            qt.Enqueue(createThread<RA_STUDY_NODE>());
            qt.Enqueue(createThread<RA_STUDY_TYPE>());
            qt.Enqueue(createThread<RA_STUDY_TEAM>());
            qt.Enqueue(createThread<RATE>());
            qt.Enqueue(createThread<REGISTER>());
            qt.Enqueue(createThread<REGISTER_CHANGE>());
            qt.Enqueue(createThread<REGISTER_HOLD>());
            qt.Enqueue(createThread<REGISTER_HOLD_REF>());
            qt.Enqueue(createThread<REGISTER_ISSUE>());
            qt.Enqueue(createThread<REGISTER_LL>());
            qt.Enqueue(createThread<REGISTER_NC>());
            qt.Enqueue(createThread<REGISTER_RISK>());
            qt.Enqueue(createThread<ROLE>());
            qt.Enqueue(createThread<ROLE_PERMISSION>());
            qt.Enqueue(createThread<ROLE_COMMODITY>());
            qt.Enqueue(createThread<ROSTER_STAFF>());
            qt.Enqueue(createThread<ROSTER_STAFF_STATUS>());
            qt.Enqueue(createThread<SETTINGS_GLOBAL>());
            qt.Enqueue(createThread<STOCK_CODE>());
            qt.Enqueue(createThread<STOCK_GROUP>());
            qt.Enqueue(createThread<UOM>());
            qt.Enqueue(createThread<USER>());
            qt.Enqueue(createThread<VARIATION_ITEM>());
            qt.Enqueue(createThread<SUBJOB_ASSIGNMENT>());
            qt.Enqueue(createThread<VARIATION_REGISTER>());
            qt.Enqueue(createThread<OFFICE>());
            qt.Enqueue(createThread<CLIENT_PROJECT>());
            qt.Enqueue(createThread<MEETING_ACTION>());
            qt.Enqueue(createThread<MEETING_TYPE>());
            qt.Enqueue(createThread<TENDER_PROFILE>());
            qt.Enqueue(createThread<TENDER_PROFILE_ITEM>());
            updateQueue();
        }

        private void threadSafeAddEntries<T>(DbSet<T> dbSet, T entry)
            where T : class, ICanSync, new()
        {
            if(!dbSet.Any(x  => x.GUID == entry.GUID))
                dbSet.Add(entry);
        }

        private Thread createThread<T>()
            where T : class, ICanSync, new()
        {
            return new Thread(() => processDbSet<T>(new object[] { new Action<string, decimal>(threadSafeSetMaxLocalProgress), new Action<string, decimal>(threadSafeSetMaxRemoteProgress), new Action<string, decimal>(threadSafeProgressLocal), new Action<string, decimal>(threadSafeProgressRemote), new Action<SyncReport>(threadSafeAddSyncReport), new Action<DbSet<T>, T>(threadSafeAddEntries) }));
            //ThreadPool.QueueUserWorkItem(new WaitCallback(processDbSet<T>), new object[] { new Action<string, decimal>(threadSafeSetMaxLocalProgress), new Action<string, decimal>(threadSafeSetMaxRemoteProgress), new Action<string, decimal>(threadSafeProgressLocal), new Action<string, decimal>(threadSafeProgressRemote), new Action<SyncReport>(threadSafeAddSyncReport), new Action<DbSet<T>, T>(threadSafeAddEntries) });
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
            Action<DbSet<T>, T> addEntryAction = (Action<DbSet<T>, T>)parameters[5];
            //bool isLocal = (bool)parameters[5];

            BluePrintsNativeEntities localDataContext = new BluePrintsNativeEntities("name=BluePrintsPerthEntities");
            BluePrintsNativeEntities remoteDataContext = new BluePrintsNativeEntities("name=BluePrintsMontrealEntities");
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
            //if (isLocal)
            //{
            if(localTotalCount == 0)
            {
                setupLocalAction(typeName, 1);
                progressLocalAction(typeName, 1);
            }
            else
            {
                setupLocalAction(typeName, localTotalCount);
                int processedCount = 0;

                //List<T> localDbItems = localDbSet.ToList();
                //List<T> remoteDbItems = remoteDbSet.ToList();
                Dictionary<string, T> addRemoteEntries = new Dictionary<string, T>();
                foreach (var localData in localDbSet)
                {
                    bool? syncRemote = null;
                    DateTime localCreated = localData.CREATED;
                    DateTime? localUpdated = localData.UPDATED;
                    DateTime? localDeleted = localData.DELETED;
                    //bool isDataLocal = localData.Office.ToUpper().Contains("PERTH");
                    bool isDataLocal = false;
                    bool isDataGlobal = true;
                    //bool isDataGlobal = localData.Office.ToUpper().Contains(BluePrintsResources.GlobalOffice.ToUpper());

                    T remoteData = remoteDbSet.FirstOrDefault(x => x.GUID == localData.GUID);
                    if (remoteData != null)
                    {
                        DateTime remoteCreated = remoteData.CREATED;
                        DateTime? remoteUpdated = remoteData.UPDATED;
                        DateTime? remoteDeleted = remoteData.DELETED;

                        if(isDataGlobal)
                        {
                            //Delete
                            if (remoteDeleted != null && localDeleted != null)
                            {
                                if (remoteDeleted > localDeleted)
                                    syncRemote = true;
                                else if (remoteDeleted < localDeleted)
                                    syncRemote = false;
                            }
                            else if (remoteDeleted != null && localDeleted == null)
                            {
                                syncRemote = true;
                            }
                            else if (remoteDeleted == null && localDeleted != null)
                            {
                                syncRemote = false;
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
                        }
                        else
                        {
                            if(remoteData.CREATED != localData.CREATED || remoteData.UPDATED != localData.UPDATED || remoteData.DELETED != localData.DELETED)
                                syncRemote = !isDataLocal;
                        }

                        if (syncRemote == true)
                        {
                            finishAction(new SyncReport() { Action = "Update", Destination = "Local", Project = localData.Office, Properties = DataUtils.ShallowCopyDiffTracking(localData, remoteData), TableName = typeName });
                        }
                        else if (syncRemote == false)
                        {
                            finishAction(new SyncReport() { Action = "Update", Destination = "Remote", Project = localData.Office, Properties = DataUtils.ShallowCopyDiffTracking(remoteData, localData), TableName = typeName });
                        }
                    }
                    else
                    {
                        T remoteNewData = new T();
                        string action = "Add";
                        //mark local data as deleted if its a remote data and doesn't exists in remote
                        if (!isDataLocal && !isDataGlobal)
                        {
                            localData.DELETED = DateTime.Now;
                            action = "Add as Deleted";
                        }

                        DataUtils.ShallowCopy(remoteNewData, localData);
                        //addEntryAction(remoteDbSet, remoteNewData);
                        //addRemoteEntries.Add(remoteNewData.GUID.ToString(), remoteNewData);
                        remoteDbSet.Add(remoteNewData);

                        finishAction(new SyncReport() { Action = action, Destination = "Remote", Project = localData.Office, Properties = string.Empty, TableName = typeName });
                    }

                    progressLocalAction(typeName, 1);
                    //processedCount += 1;
                    //if(processedCount >= 1000)
                    //{
                    //    processedCount = 0;
                    //    //remoteDbSet.AddRange(addRemoteEntries.Select(x => x.Value));
                    //    try
                    //    {
                    //        localDataContext.SaveChanges();
                    //        remoteDataContext.SaveChanges();
                    //    }
                    //    catch
                    //    {

                    //    }
                    //    //addRemoteEntries.Clear();
                    //}
                }

                try
                {
                    localDataContext.SaveChanges();
                    remoteDataContext.SaveChanges();
                }
                catch
                {

                }
                progressLocalAction(typeName, localDataContextCount);
            }
            //}
            //else
            //{
            if (remoteTotalCount == 0)
            {
                setupRemoteAction(typeName, 1);
                progressRemoteAction(typeName, 1);
            }
            else
            {
                setupRemoteAction(typeName, remoteTotalCount);
                int processedCount = 0;
                //List<T> remoteDbItems = remoteDbSet.ToList();
                //List<T> localDbItems = localDbSet.ToList();
                Dictionary<string, T> addLocalEntries = new Dictionary<string, T>();
                foreach (var remoteData in remoteDbSet)
                {
                    //bool isDataRemote = remoteData.Office.ToUpper().Contains("MONTREAL");
                    bool isDataRemote = false;
                    //bool isDataGlobal = remoteData.Office.ToUpper().Contains(BluePrintsResources.GlobalOffice.ToUpper());
                    bool isDataGlobal = true;

                    T localData = localDbSet.FirstOrDefault(x => x.GUID == remoteData.GUID);
                    if (localData == null)
                    {
                        T localNewData = new T();
                        string action = "Add";
                        //mark remote data as deleted if it doesn't exists in local
                        if (!isDataRemote && !isDataGlobal)
                        {
                            remoteData.DELETED = DateTime.Now;
                            action = "Add as Deleted";
                        }

                        DataUtils.ShallowCopy(localNewData, remoteData);
                        localDbSet.Add(localNewData);
                        //addEntryAction(localDbSet, localNewData);
                        //addLocalEntries.Add(localNewData.GUID.ToString(), localNewData);
                        finishAction(new SyncReport() { Action = action, Destination = "Local", Project = remoteData.Office, Properties = string.Empty, TableName = typeName });
                    }

                    progressRemoteAction(typeName, 1);
                    //processedCount += 1;
                    //if(processedCount >= 1000)
                    //{
                    //    processedCount = 0;
                    //    //localDbSet.AddRange(addLocalEntries.Select(x => x.Value));
                    //    try
                    //    {
                    //        localDataContext.SaveChanges();
                    //        remoteDataContext.SaveChanges();
                    //    }
                    //    catch
                    //    {

                    //    }
                    //    //addLocalEntries.Clear();
                    //}
                }

                try
                {
                    localDataContext.SaveChanges();
                    remoteDataContext.SaveChanges();
                }
                catch
                {

                }

                progressRemoteAction(typeName, remoteDataContextCount);
            }
            //}

            localDataContext.Dispose();
            remoteDataContext.Dispose();
            updateQueue();
        }
    }

}
