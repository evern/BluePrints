using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using BluePrints.Common.Utils;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Data;
using BluePrints.Common.ViewModel;
using DevExpress.Xpf.Grid;
using System.Threading;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data.Helpers;
using BluePrints.Common;
using BluePrints.Common.Helpers;
using BluePrints.Common.Projections;
using System.Windows.Threading;
using System.Windows;
using DevExpress.Xpf.Bars;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Views;
using BluePrints.Reports;
using System.IO;
using DevExpress.Xpf.Printing;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Settings;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single ESTIMATION object view model.
    /// </summary>
    public partial class ESTIMATION_ITEMSViewModelWrapper : CollectionViewModelsWrapper<ESTIMATION_ITEM, ESTIMATION_ITEMProjection, Guid, IBluePrintsEntitiesUnitOfWork, CollectionViewModel<ESTIMATION_ITEM, ESTIMATION_ITEMProjection, Guid, IBluePrintsEntitiesUnitOfWork>>
    {
        public Action ShowWORKPACKInternalName1;
        public Action ShowWORKPACKInternalName2;
        /// <summary>
        /// Creates a new instance of ESTIMATION_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static ESTIMATION_ITEMSViewModelWrapper Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new ESTIMATION_ITEMSViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the ESTIMATIONViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the ESTIMATIONViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected ESTIMATION_ITEMSViewModelWrapper(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        PROJECT loadPROJECT;
        ESTIMATION loadESTIMATION;
        DEPARTMENT loadDEPARTMENT;

        bool isQueryForLiveStatus;
        IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected override void InitializeParameters(object parameter)
        {
            OptionalEntitiesParameter<PROJECT, ESTIMATION> receiveParameter = (OptionalEntitiesParameter<PROJECT, ESTIMATION>)parameter;
            this.loadPROJECT = receiveParameter.GetFirstEntity();
            this.loadESTIMATION = receiveParameter.GetSecondEntity();

            if (this.loadPROJECT != null)
                isQueryForLiveStatus = true;
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddEntitiesLoader<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(0, bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, null, isContinueLoadingAfterPROJECT, OnEntitiesChanged);
            loaderCollection.AddEntitiesLoader<ESTIMATION, ESTIMATION, Guid, IBluePrintsEntitiesUnitOfWork>(1, bluePrintsUnitOfWorkFactory, x => x.ESTIMATIONS, ESTIMATIONProjectionFunc, typeof(PROJECT), isContinueLoadingAfterESTIMATION, OnEntitiesChanged);
            loaderCollection.AddEntitiesLoader<COMMODITY, COMMODITY, Guid, IBluePrintsEntitiesUnitOfWork>(2, bluePrintsUnitOfWorkFactory, x => x.COMMODITIES, COMMODITYProjectionFunc, typeof(PROJECT), null, OnEntitiesChanged);
            loaderCollection.AddEntitiesLoader<AREA, AREA, Guid, IBluePrintsEntitiesUnitOfWork>(4, bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc, typeof(PROJECT));
            loaderCollection.AddEntitiesLoader<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(5, bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddEntitiesLoader<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(6, bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS, DEPARTMENTProjectionFunc, null, isContinueLoadingAfterDEPARTMENT, OnEntitiesChanged);
            loaderCollection.AddEntitiesLoader<RATE, RATE, Guid, IBluePrintsEntitiesUnitOfWork>(7, bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc, typeof(PROJECT), null, OnEntitiesChanged);
            loaderCollection.AddEntitiesLoader<COMMODITY_CODE, COMMODITY_CODE, Guid, IBluePrintsEntitiesUnitOfWork>(8, bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, null, null, null, OnEntitiesChanged);
            loaderCollection.AddEntitiesLoader<WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork>(9, bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc, null, null, OnEntitiesChanged);
            
            InvokeEntitiesLoaderDescriptionLoading();
        }

        bool isContinueLoadingAfterPROJECT(IEnumerable<PROJECT> entities)
        {
            if(entities.Count() == 0)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "PROJECT"))));
                return false;
            }

            this.loadPROJECT = entities.First();
            return true;
        }

        bool isContinueLoadingAfterESTIMATION(IEnumerable<ESTIMATION> entities)
        {
            if (entities.Count() == 0)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "ESTIMATION"))));
                return false;
            }

            this.loadESTIMATION = entities.First();
            return true;
        }

        bool isContinueLoadingAfterDEPARTMENT(IEnumerable<DEPARTMENT> entities)
        {
            if (entities.Count() == 0)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "DEPARTMENT"))));
                return false;
            }

            this.loadDEPARTMENT = entities.First();
            return true;
        }

        Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            if (isQueryForLiveStatus)
                return query => query.Where(x => x.GUID == this.loadPROJECT.GUID);
            else
                return query => query.Where(x => x.GUID == this.loadESTIMATION.GUID_PROJECT);
        }

        Func<IRepositoryQuery<ESTIMATION>, IQueryable<ESTIMATION>> ESTIMATIONProjectionFunc()
        {
            if (isQueryForLiveStatus)
                return query => query.Where(x => x.GUID_PROJECT == this.loadPROJECT.GUID && x.STATUS == EstimationStatus.Live);
            else
                return query => query.Where(x => x.GUID == this.loadESTIMATION.GUID);
        }

        Func<IRepositoryQuery<COMMODITY>, IQueryable<COMMODITY>> COMMODITYProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == this.loadPROJECT.GUID);
        }

        Func<IRepositoryQuery<DEPARTMENT>, IQueryable<DEPARTMENT>> DEPARTMENTProjectionFunc()
        {
            return query => query.Where(x => x.NAME == CommonResources.DefaultConstructionDepartment);
        }

        Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> WORKPACKProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.TYPE != WorkpackType.Design);
        }

        Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(this.bluePrintsUnitOfWorkFactory, x => x.ESTIMATION_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoader.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<ESTIMATION_ITEM>, IQueryable<ESTIMATION_ITEMProjection>> ConstructMainViewModelProjection()
        {
            Func<IQueryable<RATE>> getRATESFunc = loaderCollection.GetCollectionFunc<RATE>();
            Func<ESTIMATION> getESTIMATIONFunc = loaderCollection.GetObjectFunc<ESTIMATION>();
            Func<DEPARTMENT> getDEPARTMENTFunc = loaderCollection.GetObjectFunc<DEPARTMENT>();

            return query => ESTIMATION_ITEMProjectionQueries.JoinCOMMODITYAndRATESOnESTIMATION_ITEMS(query, getESTIMATIONFunc, getDEPARTMENTFunc, getRATESFunc);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ESTIMATION_ITEMProjection> entities)
        {
            MainViewModel.ApplyProjectionPropertiesToEntityCallBack = this.ApplyProjectionPropertiesToEntity;
            MainViewModel.OnEntitySavedCallBack = this.OnEntitiesSavedCallBack;
            MainViewModel.SetParentViewModel(this);
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
        }

        protected override void OnEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender)
        {
            if (sender == MainViewModel)
                return;


            if (loadESTIMATION != null && changedType == typeof(ESTIMATION) && loadESTIMATION.GUID.ToString() == key.ToString() ||
                loadPROJECT != null && changedType == typeof(PROJECT) && loadPROJECT.GUID.ToString() == key.ToString() ||
                loadDEPARTMENT != null && changedType == typeof(DEPARTMENT) && loadDEPARTMENT.GUID.ToString() == key.ToString())
            {
                if (messageType == EntityMessageType.Added)
                    MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Restored, StringFormatUtils.GetEntityNameByType(changedType)));
                else if (messageType == EntityMessageType.Deleted)
                    MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, StringFormatUtils.GetEntityNameByType(changedType)));
            }

            if (loadPROJECT != null || loadESTIMATION != null || loadDEPARTMENT != null)
            {
                if (MainViewModel != null)
                    mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.Refresh()));
                else if (loadPROJECT != null || loadESTIMATION != null)
                    mainThreadDispatcher.BeginInvoke(new Action(() => InitializeAndLoadEntitiesLoaderDescription()));

                mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
            }
        }

        #region Collection Call Backs
        public void ApplyProjectionPropertiesToEntity(ESTIMATION_ITEMProjection projectionEntity, ESTIMATION_ITEM entity)
        {
            projectionEntity.ESTIMATION_ITEM.GUID_ESTIMATION = loadESTIMATION.GUID;
            DataUtils.ShallowCopy(entity, projectionEntity.ESTIMATION_ITEM);
            //workaround for created because Save() only sets the projection primary key, this is used for property redo where the interceptor only tampers with UPDATED and CREATED is left as null
            if (entity.CREATED.Date.Year == 1)
                projectionEntity.ESTIMATION_ITEM.CREATED = DateTime.Now;

            entity.CREATED = projectionEntity.ESTIMATION_ITEM.CREATED;
        }

        public void OnEntitiesSavedCallBack(Guid primaryKey, ESTIMATION_ITEMProjection projectionEntity, ESTIMATION_ITEM entity, bool isNewEntity)
        {
            projectionEntity.GUID = entity.GUID;
            projectionEntity.ESTIMATION_ITEM.GUID = entity.GUID;
            projectionEntity.ESTIMATION_ITEM.GUID_ORIGINAL = entity.GUID_ORIGINAL;

            //set this virtual property so the view will display the COMMODITY information
            if (isNewEntity)
            {
                COMMODITY findCOMMODITY = COMMODITYCollection.FirstOrDefault(x => x.GUID == projectionEntity.ESTIMATION_ITEM.GUID_COMMODITY);
                if(findCOMMODITY != null)
                {
                    projectionEntity.ESTIMATION_ITEM.COMMODITY = findCOMMODITY;
                }
            }
        }
        #endregion
        #endregion

        #region View Behavior
        /// <summary>
        /// Influence column(s) when changes happens in other column
        /// </summary>
        public void CellValueChanging(CellValueChangedEventArgs e)
        {
            if (e.RowHandle != GridControl.NewItemRowHandle)
                return;

            ESTIMATION_ITEMProjection activeESTIMATION_ITEM = (ESTIMATION_ITEMProjection)e.Row;
            if (e.Column.FieldName == BindableBase.GetPropertyName(() => new ESTIMATION_ITEMProjection().ESTIMATION_ITEM) + "." + BindableBase.GetPropertyName(() => new ESTIMATION_ITEM().GUID_COMMODITY))
            {
                COMMODITY chosenCOMMODITY = COMMODITYCollection.FirstOrDefault(entity => entity.GUID == (Guid)e.Value);
                if (chosenCOMMODITY != null)
                {
                    COMMODITY_CODE chosenCOMMODITY_CODE = COMMODITY_CODECollection.FirstOrDefault(x => x.GUID == chosenCOMMODITY.GUID_COMMODITYCODE);
                    activeESTIMATION_ITEM.ESTIMATION_ITEM.GUID_DISCIPLINE = chosenCOMMODITY_CODE.GUID_DISCIPLINE;
                    MainViewModel.UpdateSelectedEntity();
                }
            }
        }

        public void dragDropManager_Drop(object sender, DevExpress.Xpf.Grid.DragDrop.GridDropEventArgs e)
        {
            foreach (TreeListNode obj in e.DraggedRows)
            {
                COMMODITYProjection droppedCOMMODITY = obj.Content as COMMODITYProjection;
                if (droppedCOMMODITY != null)
                {
                    ESTIMATION_ITEMProjection newESTIMATION_ITEM = new ESTIMATION_ITEMProjection();
                    newESTIMATION_ITEM.ESTIMATION_ITEM.GUID_COMMODITY = droppedCOMMODITY.GUID;
                    COMMODITY_CODE droppedCOMMODITY_CODE = COMMODITY_CODECollection.FirstOrDefault(x => x.GUID == droppedCOMMODITY.COMMODITY.GUID_COMMODITYCODE);
                    newESTIMATION_ITEM.ESTIMATION_ITEM.GUID_DISCIPLINE = droppedCOMMODITY_CODE.GUID_DISCIPLINE;
                    string errorMessage = string.Empty;
                    if (!MainViewModel.IsValidEntity(newESTIMATION_ITEM, ref errorMessage))
                    {
                        mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage(errorMessage)));
                    }
                    else
                    {
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(newESTIMATION_ITEM, null, null, null, EntityMessageType.Added);
                        MainViewModel.Save(newESTIMATION_ITEM);
                    }
                }
                else
                {
                    COMMODITY_CODE droppedCOMMODITY_CODE = obj.Content as COMMODITY_CODE;
                    if(droppedCOMMODITY_CODE != null)
                    {
                        COMMODITY existingCOMMODITY = COMMODITYCollection.FirstOrDefault(x => x.GUID == droppedCOMMODITY_CODE.GUID);
                        if(existingCOMMODITY == null)
                        {
                            COMMODITY newCOMMODITY = new COMMODITY();
                            newCOMMODITY.GUID_COMMODITYCODE = droppedCOMMODITY_CODE.GUID;
                            newCOMMODITY.GUID_PROJECT = loadPROJECT.GUID;
                            newCOMMODITY.CREATED = DateTime.Now;
                            ((CollectionViewModel<COMMODITY, COMMODITY, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<COMMODITY>()).Save(newCOMMODITY);

                            ESTIMATION_ITEMProjection newESTIMATION_ITEM = new ESTIMATION_ITEMProjection();
                            newESTIMATION_ITEM.ESTIMATION_ITEM.GUID_COMMODITY = newCOMMODITY.GUID;

                            string errorMessage = string.Empty;
                            if (!MainViewModel.IsValidEntity(newESTIMATION_ITEM, ref errorMessage))
                            {
                                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage(errorMessage)));
                            }
                            else
                            {
                                MainViewModel.EntitiesUndoRedoManager.AddUndo(newESTIMATION_ITEM, null, null, null, EntityMessageType.Added);
                                MainViewModel.Save(newESTIMATION_ITEM);
                            }
                        }
                    }
                }
            }

            e.Handled = true;
        }
        #endregion

        #region View Commands
        public bool CanAutoPopulate(object button)
        {
            if (MainViewModel == null || MainViewModel.SelectedEntities.Count == 0)
                return false;

            return true;
        }

        public void AutoPopulate(object button)
        {
            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            var info = GridPopupMenuBase.GetGridMenuInfo((DependencyObject)button) as GridMenuInfo;

            string disciplineFieldName = BindableBase.GetPropertyName(() => new ESTIMATION_ITEMProjection().ESTIMATION_ITEM) + "." + BindableBase.GetPropertyName(() => new ESTIMATION_ITEM().GUID_DISCIPLINE);
            string areaFieldName = BindableBase.GetPropertyName(() => new ESTIMATION_ITEMProjection().ESTIMATION_ITEM) + "." + BindableBase.GetPropertyName(() => new ESTIMATION_ITEM().GUID_AREA);
            string supplyWorkpackFieldName = BindableBase.GetPropertyName(() => new ESTIMATION_ITEMProjection().ESTIMATION_ITEM) + "." + BindableBase.GetPropertyName(() => new ESTIMATION_ITEM().GUID_SUPPLYWORKPACK);
            string installWorkpackFieldName = BindableBase.GetPropertyName(() => new ESTIMATION_ITEMProjection().ESTIMATION_ITEM) + "." + BindableBase.GetPropertyName(() => new ESTIMATION_ITEM().GUID_INSTALLWORKPACK);

            List<ESTIMATION_ITEMProjection> entitiesToSave = new List<ESTIMATION_ITEMProjection>();

            foreach (ESTIMATION_ITEMProjection entity in MainViewModel.SelectedEntities)
            {
                if (info.Column.FieldName == disciplineFieldName || info.Column.FieldName == areaFieldName)
                {
                    WORKPACK entityWORKPACK = WORKPACKCollection.FirstOrDefault(x => x.GUID == entity.ESTIMATION_ITEM.GUID_SUPPLYWORKPACK);
                    if (entityWORKPACK == null)
                        entityWORKPACK = WORKPACKCollection.FirstOrDefault(x => x.GUID == entity.ESTIMATION_ITEM.GUID_INSTALLWORKPACK);

                    if (entityWORKPACK == null)
                        continue;

                    if (info.Column.FieldName == disciplineFieldName)
                        MainViewModel.SetNestedValueWithUndo(entity, info.Column.FieldName, entityWORKPACK.GUID_DDISCIPLINE);
                    else if (info.Column.FieldName == areaFieldName)
                        MainViewModel.SetNestedValueWithUndo(entity, info.Column.FieldName, entityWORKPACK.GUID_DAREA);

                    entitiesToSave.Add(entity);
                }
                else if (info.Column.FieldName == supplyWorkpackFieldName || info.Column.FieldName == installWorkpackFieldName)
                {
                    if (entity.ESTIMATION_ITEM.GUID_DISCIPLINE == Guid.Empty || entity.ESTIMATION_ITEM.GUID_AREA == Guid.Empty)
                        continue;

                    WORKPACK findWORKPACK;
                    if(info.Column.FieldName == supplyWorkpackFieldName)
                        findWORKPACK = WORKPACKCollection.FirstOrDefault(x => x.GUID_DDEPARTMENT == loadDEPARTMENT.GUID && x.GUID_DDISCIPLINE == entity.ESTIMATION_ITEM.GUID_DISCIPLINE && x.TYPE == WorkpackType.Supply);
                    else
                        findWORKPACK = WORKPACKCollection.FirstOrDefault(x => x.GUID_DDEPARTMENT == loadDEPARTMENT.GUID && x.GUID_DDISCIPLINE == entity.ESTIMATION_ITEM.GUID_DISCIPLINE && x.TYPE == WorkpackType.Install);

                    if (findWORKPACK == null)
                    {
                        WORKPACK newWORKPACK = new WORKPACK();
                        newWORKPACK.GUID_PROJECT = loadPROJECT.GUID;
                        newWORKPACK.GUID_DAREA = (Guid)entity.ESTIMATION_ITEM.GUID_AREA;
                        newWORKPACK.GUID_DDISCIPLINE = (Guid)entity.ESTIMATION_ITEM.GUID_DISCIPLINE;
                        newWORKPACK.GUID_DDEPARTMENT = loadDEPARTMENT.GUID;
                        newWORKPACK.INTERNAL_NAME2 = BluePrintDataUtils.WORKPACK_Generate_InstallSupplyInternalNumber(loadPROJECT, newWORKPACK, WORKPACKCollection, loaderCollection.GetViewModel<AREA>(), loaderCollection.GetViewModel<DISCIPLINE>(), loaderCollection.GetViewModel<PHASE>(), info.Column.FieldName == installWorkpackFieldName);

                        newWORKPACK.STARTDATE = DateTime.Now;
                        newWORKPACK.ENDDATE = BluePrintDataUtils.WORKPACK_Calculate_EndDate(newWORKPACK.STARTDATE, loadPROJECT);
                        DateTime reviewStartDate = newWORKPACK.STARTDATE;
                        DateTime reviewEndDate = newWORKPACK.ENDDATE;
                        BluePrintDataUtils.WORKPACK_Calculate_ReviewPeriod(ref reviewStartDate, ref reviewEndDate, loadPROJECT, false);
                        newWORKPACK.REVIEWSTARTDATE = reviewStartDate;
                        newWORKPACK.REVIEWENDDATE = reviewEndDate;
                        newWORKPACK.AUTOGENERATED = true;
                        newWORKPACK.TYPE = info.Column.FieldName == supplyWorkpackFieldName ? WorkpackType.Supply : WorkpackType.Install;
                        ((CollectionViewModel<WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<WORKPACK>()).Save(newWORKPACK);

                        MainViewModel.SetNestedValueWithUndo(entity, info.Column.FieldName, newWORKPACK.GUID);
                    }
                    else
                        MainViewModel.SetNestedValueWithUndo(entity, info.Column.FieldName, findWORKPACK.GUID);

                    entitiesToSave.Add(entity);
                }
            }

            MainViewModel.BulkSave(entitiesToSave);
            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }
        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get
            {
                return "ESTIMATION_ITEMWrapperView";
            }
        }

        public IEnumerable<AREA> AREACollection
        {
            get
            {
                var collection = GetEntities<AREA>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NUM);
                return collection;
            }
        }

        public IEnumerable<DISCIPLINE> DISCIPLINECollection
        {
            get
            {
                var collection = GetEntities<DISCIPLINE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public IEnumerable<COMMODITY> COMMODITYCollection
        {
            get
            {
                return GetEntities<COMMODITY>();
            }
        }

        public IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection
        {
            get
            {
                return GetEntities<COMMODITY_CODE>();
            }
        }

        public IEnumerable<WORKPACK> SupplyWORKPACKCollection
        {
            get
            {
                return GetEntities<WORKPACK>() == null ? null : GetEntities<WORKPACK>().Where(x => x.TYPE == WorkpackType.Supply).OrderBy(x => x.INTERNAL_NAME2);
            }
        }

        public IEnumerable<WORKPACK> InstallWORKPACKCollection
        {
            get
            {
                return GetEntities<WORKPACK>() == null ? null : GetEntities<WORKPACK>().Where(x => x.TYPE == WorkpackType.Install).OrderBy(x => x.INTERNAL_NAME2);
            }
        }


        public IEnumerable<WORKPACK> WORKPACKCollection
        {
            get
            {
                var collection = GetEntities<WORKPACK>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NAME2);
                return collection;
            }
        }
        #endregion

        //#region Reporting
        //public bool CanEditReport()
        //{
        //    if (MainViewModel == null || MainViewModel.Entities.Count == 0)
        //        return false;

        //    return true;
        //}

        //public bool CanViewReport()
        //{
        //    if (MainViewModel == null || MainViewModel.Entities.Count == 0)
        //        return false;

        //    return true;
        //}

        //public void EditReport()
        //{
        //    REPORTDesigner reportDesigner = new REPORTDesigner(loadPROJECT, (CollectionViewModel<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<PROJECT_REPORT>(), ReportType.Baseline_Report);
        //    if (reportDesigner.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //        reportDesigner.Dispose();
        //    else
        //        reportDesigner.Dispose();
        //}

        //public void ViewReport()
        //{
        //    XtraReportESTIMATION_ITEMS baselineReport = new XtraReportESTIMATION_ITEMS();
        //    PROJECT_REPORT dbProjectReport = loaderCollection.GetObject<PROJECT_REPORT>();
        //    if (dbProjectReport != null)
        //    {
        //        string reportString = dbProjectReport.REPORT.ToString();
        //        using (StreamWriter sw = new StreamWriter(new MemoryStream()))
        //        {
        //            sw.Write(reportString);
        //            sw.Flush();
        //            baselineReport.LoadLayout(sw.BaseStream);
        //        }
        //    }

        //    make sure disciplines are all populated
        //    PopulateNavigationalProperties();
        //    baselineReport.AssignProperties(loadPROJECT, loadESTIMATION, MainViewModel.Entities);
        //    DocumentPreviewWindow previewWindow = new DocumentPreviewWindow();
        //    previewWindow.PreviewControl.DocumentSource = baselineReport;
        //    previewWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        //    previewWindow.WindowState = WindowState.Maximized;
        //    baselineReport.RequestParameters = false;
        //    baselineReport.CreateDocument(true);
        //    previewWindow.ShowDialog();
        //}


        //void PopulateNavigationalProperties()
        //{
        //    foreach (ESTIMATION_ITEMProjection projection in MainViewModel.Entities)
        //    {
        //        if (projection.ESTIMATION_ITEM.GUID_DISCIPLINE != null && projection.ESTIMATION_ITEM.DISCIPLINE == null)
        //        {
        //            projection.ESTIMATION_ITEM.DISCIPLINE = DISCIPLINECollection.FirstOrDefault(x => x.GUID == projection.ESTIMATION_ITEM.GUID_DISCIPLINE);
        //        }

        //        if (projection.ESTIMATION_ITEM.GUID_AREA != null && projection.ESTIMATION_ITEM.AREA == null)
        //        {
        //            projection.ESTIMATION_ITEM.AREA = AREACollection.FirstOrDefault(x => x.GUID == projection.ESTIMATION_ITEM.GUID_AREA);
        //        }
        //    }
        //}
        //#endregion
    }
}
