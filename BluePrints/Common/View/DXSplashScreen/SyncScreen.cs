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

        protected IMessageBoxService MessageBoxService
        {
            get { return this.GetRequiredService<IMessageBoxService>(); }
        }

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
            qtPriorityCompletionCount += 1;
            if (qtPriority < 25)
            {
                IEnumerable <ThreadPriority> threadPriorities = qt.Where(x => x.Priority == qtPriority);

                bool isCompleted = qtPriorityCompletionCount >= (threadPriorities.Count() * 2);

                if(isCompleted)
                {
                    qtPriorityCompletionCount = 0;
                    qtPriority += 1;

                    threadPriorities = qt.Where(x => x.Priority == qtPriority);
                    foreach (ThreadPriority threadPriority in threadPriorities)
                    {
                        threadPriority.LocalThread.Start();
                        threadPriority.RemoteThread.Start();
                    }
                }
            }
            else
            {
                //MessageBoxService.ShowMessage("Sync completed!", "High Five!", MessageButton.OK, MessageIcon.Information);
            }
        }

        List<ThreadPriority> qt;
        int qtPriority = 6;
        int qtPriorityCompletionCount = 12;
        //int qtPriority = 0;
        //int qtPriorityCompletionCount = 0;
        public void SyncData()
        {
            //ThreadPool.SetMaxThreads(1, 1);
            qt = new List<ThreadPriority>();
            qt.Add(createThread<OFFICE>(1));
            qt.Add(createThread<UOM>(1));
            qt.Add(createThread<PROJECT>(2));
            qt.Add(createThread<DISCIPLINE>(2));
            qt.Add(createThread<RA_STUDY_TYPE>(2));
            qt.Add(createThread<DEPARTMENT>(3));
            qt.Add(createThread<AREA>(3));
            qt.Add(createThread<COMMODITY_CODE>(3));
            qt.Add(createThread<MEETING_TYPE>(4));
            qt.Add(createThread<MEETING_ACTION>(4));
            qt.Add(createThread<RA_GUIDE_PROMPT>(4));
            qt.Add(createThread<RA_STUDY>(5));
            qt.Add(createThread<FORECAST>(5));
            qt.Add(createThread<FORECAST_JOB>(6));
            qt.Add(createThread<FORECAST_PO>(6));
            qt.Add(createThread<FORECAST_EAC>(6));
            qt.Add(createThread<BASELINE>(6));
            qt.Add(createThread<DELIVERABLES_STATUS>(6));
            qt.Add(createThread<DOCTYPE>(6));
            qt.Add(createThread<FORECAST_JOB_HOUR>(7));
            qt.Add(createThread<FORECAST_JOB_SETTING>(7));
            qt.Add(createThread<FORECAST_PO_SETTING>(7));
            qt.Add(createThread<PHASE>(7));
            qt.Add(createThread<SUBJOB>(7));
            qt.Add(createThread<USER>(7));
            qt.Add(createThread<VARIATION>(8));
            qt.Add(createThread<WORKPACK>(8));
            qt.Add(createThread<CLIENT>(8));
            qt.Add(createThread<ESTIMATE>(9));
            qt.Add(createThread<HSE>(10));
            qt.Add(createThread<MEETING>(10));
            qt.Add(createThread<MINUTE_AGENDA>(10));
            qt.Add(createThread<MINUTE_TITLE>(11));
            qt.Add(createThread<PROGRESS>(11));
            qt.Add(createThread<RA_GUIDE_SUBPROMPT>(11));
            qt.Add(createThread<RA_STUDY_NODE>(12));
            qt.Add(createThread<RA_STUDY_DRAWING>(12));
            qt.Add(createThread<BASELINE_ITEM>(12));
            qt.Add(createThread<REGISTER_HOLD>(13));
            qt.Add(createThread<ROLE>(13));
            qt.Add(createThread<BASELINE_ITEM_WORK>(14));
            qt.Add(createThread<CLIENT_PROJECT>(14));
            qt.Add(createThread<DAYWORK>(14));
            qt.Add(createThread<DAYWORK_EQUIPMENT>(15));
            qt.Add(createThread<DAYWORK_LABOUR>(15));
            qt.Add(createThread<DAYWORK_MATERIAL>(15));
            qt.Add(createThread<DAYWORK_STAFF_ROLE>(16));
            qt.Add(createThread<DSTATUS_DOCTYPE>(16));
            qt.Add(createThread<ESTIMATE_ITEM>(16));
            qt.Add(createThread<HOLIDAY>(17));
            qt.Add(createThread<HSE_INCIDENT>(17));
            qt.Add(createThread<HSE_INJURY>(17));
            qt.Add(createThread<MEETING_USER>(18));
            qt.Add(createThread<P6_ASSIGNMENT>(18));
            qt.Add(createThread<PROGRESS_ITEM>(18));
            qt.Add(createThread<PROJECT_DISCIPLINE>(19));
            qt.Add(createThread<PROJECT_REPORT>(19));
            qt.Add(createThread<RA_STUDY_DATA>(19));
            qt.Add(createThread<RA_STUDY_TEAM>(20));
            qt.Add(createThread<RATE>(20));
            qt.Add(createThread<REGISTER>(20));
            qt.Add(createThread<REGISTER_CHANGE>(21));
            qt.Add(createThread<REGISTER_HOLD_REF>(21));
            qt.Add(createThread<REGISTER_ISSUE>(21));
            qt.Add(createThread<REGISTER_LL>(22));
            qt.Add(createThread<REGISTER_NC>(22));
            qt.Add(createThread<REGISTER_RISK>(22));
            qt.Add(createThread<ROLE_COMMODITY>(23));
            qt.Add(createThread<ROLE_PERMISSION>(23));
            qt.Add(createThread<ROSTER_STAFF>(23));
            qt.Add(createThread<SUBJOB_ASSIGNMENT>(24));
            qt.Add(createThread<TENDER_PROFILE>(24));
            qt.Add(createThread<VARIATION_ITEM>(24));
            qt.Add(createThread<VARIATION_CONSTRUCTION>(25));
            qt.Add(createThread<TENDER_PROFILE_ITEM>(25));

            updateQueue();
        }

        private void threadSafeAddEntries<T>(DbSet<T> dbSet, T entry)
            where T : class, ICanSync, new()
        {
            if(!dbSet.Any(x  => x.GUID == entry.GUID))
                dbSet.Add(entry);
        }

        private ThreadPriority createThread<T>(int priority)
            where T : class, ICanSync, new()
        {
            ThreadPriority newPriority = new ThreadPriority();
            List<object> objParameter = new List<object>() { new Action<string, decimal>(threadSafeSetMaxLocalProgress), new Action<string, decimal>(threadSafeSetMaxRemoteProgress), new Action<string, decimal>(threadSafeProgressLocal), new Action<string, decimal>(threadSafeProgressRemote), new Action<SyncReport>(threadSafeAddSyncReport), new Action<DbSet<T>, T>(threadSafeAddEntries) };
            List<object> localParameter = new List<object>(objParameter);
            List<object> remoteParameter = new List<object>(objParameter);

            localParameter.Add(true);
            remoteParameter.Add(false);

            newPriority.LocalThread = new Thread(() => processDbSet<T>(localParameter.ToArray()));
            newPriority.RemoteThread = new Thread(() => processDbSet<T>(remoteParameter.ToArray()));
            newPriority.Priority = priority;

            return newPriority;
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
            bool isLocal = (bool)parameters[6];

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
            }

            localDataContext.Dispose();
            remoteDataContext.Dispose();
            updateQueue();
        }
    }

    public class ThreadPriority
    {
        public Thread LocalThread { get; set; }
        public Thread RemoteThread { get; set; }
        public int Priority { get; set; }
    }
}
