using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Helpers;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Helpers;
using BluePrints.Common.Projections;
using BluePrints.Common.Reports;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using BluePrints.Reports;
using DevExpress.Data.Filtering;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Printing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single BASELINE object view model.
    /// </summary>
    public partial class EXO_JobPermissionCollectionViewModelWrapper : EXO_SubjobCollectionViewModelWrapper
    {
        /// <summary>
        /// Creates a new instance of BASELINE_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static new EXO_JobPermissionCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new EXO_JobPermissionCollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the BASELINEViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the BASELINEViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected EXO_JobPermissionCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {

        }

        #region Code Properties
        private readonly IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected DispatcherTimer delayedPermissionRefreshDispatcher;
        #endregion

        #region Loading Operations
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<Data.PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
            initializeCompulsoryViewProperties(loadPROJECT);
            initializeOptionalViewCollectionsOnRefresh = false;
            SubJobRegex = loadPROJECT.NUMBER + BluePrintsResources.Regex_SUBJOB;
            DisciplineRegex = BluePrintsResources.Regex_DISCIPLINE;

            tryCombineLocalUsers = true;
            delayedPermissionRefreshDispatcher = new DispatcherTimer();
            delayedPermissionRefreshDispatcher.Interval = new TimeSpan(0, 0, 0, 0, 1);
            delayedPermissionRefreshDispatcher.Tick += DelayedPermissionRefreshDispatcher_Tick;
            IgnoreCostGroupCostType = true;

            bluePrintsEntitiesUnitOfWork = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
        }

        private void DelayedPermissionRefreshDispatcher_Tick(object sender, EventArgs e)
        {
            delayedPermissionRefreshDispatcher.Stop();
            refreshPermissions();
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<ExoSubJobProjection>> specifyMainViewModelProjection()
        {
            return query => ExoQueries.GetExoSubJob(localPrimeroUnitOfWork, loadPROJECT, exoSTAFFS, loadPROJECT.OfficeNameForExo, COMMODITY_CODECollection, STOCK_ITEMSCollection);
        }

        protected override bool OnMainViewModelLoaded(IEnumerable<ExoSubJobProjection> entities)
        {
            MainViewModel.OnBeforeRowSaveIsContinue = onBeforeRowSaveIsContinue;
            return base.OnMainViewModelLoaded(entities);
        }

        private bool onBeforeRowSaveIsContinue(CellValueChangedEventArgs e, ExoSubJobProjection projection)
        {
            if (e.Column.FieldName == BindableBase.GetPropertyName(() => new ExoSubJobProjection().IsLineExistsInExo))
            {
                return false;
            }

            return true;
        }

        protected override void OnSelectedEntitiesChanged()
        {
            if(delayedPermissionRefreshDispatcher != null)
            {
                isPermissionLoading = true;
                this.RaisePropertyChanged(x => x.IsPermissionLoading);
                this.RaisePropertyChanged(x => x.IsPermissionGridEnabled);
                delayedPermissionRefreshDispatcher.Start();

            }
        }
        #endregion

        #region Events
        public override void ValidateCell(GridCellValidationEventArgs e)
        {
            if(MainViewModel != null)
            {
                if (e.Column.FieldName == BindableBase.GetPropertyName(() => new ExoSubJobProjection().IsLineExistsInExo))
                {
                    MainViewModel.ValidateCell(e);
                    e.Handled = true;
                }
            }
        }

        public bool CanUploadToExo()
        {
            return !IsLoading;
        }

        public void UploadToExo()
        {
            if(uploadToExo())
                OnSelectedEntitiesChanged();
        }

        private bool uploadToExo()
        {
            List<ErrorMessage> errorMessages;
            IEnumerable<ExoSubJobProjection> addedSubJobs = ExoMethods.CommitToExo(SelectedEntities, MessageBoxService, masterJob, copyLine, loadPROJECT, USERCollection, localPrimeroUnitOfWork, bluePrintsEntitiesUnitOfWork, BulkColumnEditDialogService, out errorMessages);

            if (errorMessages.Count > 0)
            {
                DialogCollectionViewModel<ErrorMessage> viewModel = DialogCollectionViewModel<ErrorMessage>.Create(errorMessages, "Errors");
                ErrorMessagesDialogService.ShowDialog(MessageButton.OKCancel, string.Empty, "ListErrorMessages", viewModel);
            }

            if (addedSubJobs.Count() > 0)
                return true;
            else
                return false;
        }

        public override string UnifiedValueValidation(ExoSubJobProjection projection, string field_name, object new_value, bool isPaste)
        {
            if (field_name == BindableBase.GetPropertyName(() => new ExoSubJobProjection().IsLineExistsInExo))
            {
                bool newValue = (bool)new_value;
                if (newValue && !projection.IsLineExistsInExo)
                {
                    if (!uploadToExo())
                        return "Error adding";
                    else
                    {
                        projection.IsLineExistsInExo = true;
                        ExoSubJobProjection sameSubJobProjection = Entities.Where(x => x.IsLineExistsInExo).FirstOrDefault(x => x.SubJobCode != null && x.SubJobId == projection.SubJobId);
                        if (sameSubJobProjection != null && sameSubJobProjection.AuthUserIds != null)
                            projection.AuthUserIds = new List<int>(sameSubJobProjection.AuthUserIds);

                        delayedPermissionRefreshDispatcher.Start();
                    }
                }
                else if (!newValue && projection.IsLineExistsInExo)
                {
                    JOBCOST_LINES line = localPrimeroUnitOfWork.JOBCOST_LINES.First(x => x.SEQNO == projection.LineId);
                    if (line != null)
                    {
                        if (line.ACTUAL_UNITCOST != null && line.ACTUAL_UNITCOST > 0)
                            return "Cannot remove line in EXO because it already has a budget, please contact a cost controller to remove the line";

                        ExoMethods.UpdateJOBCOST_LINES_AUDIT(bluePrintsEntitiesUnitOfWork, projection, line, true);
                        localPrimeroUnitOfWork.JOBCOST_LINES.Remove(line);
                        localPrimeroUnitOfWork.SaveChanges();
                    }

                    projection.ExoBudget = 0;
                    projection.SubJobTitle = string.Empty;
                    projection.LineId = null;
                    projection.IsLineExistsInExo = false;

                    delayedPermissionRefreshDispatcher.Start();
                }

                projection.Update();
            }

            return base.UnifiedValueValidation(projection, field_name, new_value, isPaste);
        }

        public void PermissionCellValueChanging(CellValueChangedEventArgs e)
        {
            //skip on new row
            if (e.RowHandle < 0)
            {
                e.Handled = true;
                base.CellValueChanging(e);
                return;
            }

            ExoSubJobAuth editingSubJobAuth = (ExoSubJobAuth)e.Row;
            //don't need to validate fieldname since only this field is changeable in role permission grid control

            bool newValue = (bool)e.Value;
            if (newValue)
            {
                foreach (ExoSubJobProjection selectedEntity in SelectedEntities.Where(x => x.IsLineExistsInExo && x.SubJobCode != null && x.SubJobId != null))
                {
                    if (editingSubJobAuth.User.ProjectLocaleExoId == null)
                        continue;

                    ExoMethods.findExistingOrAddResourceAllocation(localPrimeroUnitOfWork, (int)selectedEntity.SubJobId, (int)editingSubJobAuth.User.ProjectLocaleExoId);

                    //add authorised user to all entities with same subjob id
                    foreach (ExoSubJobProjection sameSubJobEntity in Entities.Where(x => x.SubJobCode != null && x.SubJobId == selectedEntity.SubJobId))
                    {
                        if (editingSubJobAuth.User.ProjectLocaleExoId != null && !sameSubJobEntity.AuthUserIds.Any(x => x == editingSubJobAuth.User.ProjectLocaleExoId))
                        {
                            sameSubJobEntity.AuthUserIds.Add((int)editingSubJobAuth.User.ProjectLocaleExoId);
                        }
                    }
                }

                e.Handled = true;
            }
            else
            {
                foreach (ExoSubJobProjection selectedEntity in SelectedEntities.Where(x => x.IsLineExistsInExo && x.SubJobCode != null && x.SubJobId != null))
                {
                    if (editingSubJobAuth.User.ProjectLocaleExoId != null)
                        ExoMethods.deleteResourceAllocation(localPrimeroUnitOfWork, (int)selectedEntity.SubJobId, (int)editingSubJobAuth.User.ProjectLocaleExoId);

                    //remove authorised user to all entities with same subjob id
                    foreach (ExoSubJobProjection sameSubJobEntity in Entities.Where(x => x.SubJobCode != null && x.SubJobId == selectedEntity.SubJobId))
                    {
                        if (editingSubJobAuth.User.ProjectLocaleExoId != null && sameSubJobEntity.AuthUserIds.Any(x => x == editingSubJobAuth.User.ProjectLocaleExoId))
                            sameSubJobEntity.AuthUserIds.Remove((int)editingSubJobAuth.User.ProjectLocaleExoId);
                    }
                }
            }

            //refreshPermissions();
            //base.CellValueChanging(e);
        }

        protected override void CellValueChangedImmediatePost(CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == (BindableBase.GetPropertyName(() => new ExoSubJobProjection().CommodityCode)) || e.Column.FieldName == (BindableBase.GetPropertyName(() => new ExoSubJobProjection().IsLineExistsInExo)))
            {
                TableView tableView = e.Source as TableView;
                if (tableView != null && e.RowHandle != GridControl.NewItemRowHandle)
                {
                    tableView.CommitEditing();
                }
            }
        }

        public bool IsPermissionGridEnabled
        {
            get
            {
                if (Entities == null || SelectedEntities.Count == 0)
                    return false;

                return SelectedEntities.Any(x => x.IsLineExistsInExo);
            }
        }

        protected virtual void refreshPermissions()
        {
            this.RaisePropertyChanged(x => x.IsPermissionGridEnabled);
            this.RaisePropertyChanged(x => x.Users);
            this.RaisePropertyChanged(x => x.BluePrintsUsers);
        }

        List<ExoSubJobAuth> orderedAuthUsers;
        public IEnumerable<ExoSubJobAuth> Users
        {
            get
            {
                if (MainViewModel == null || !IsPermissionGridEnabled)
                    return null;

                var permissions = new List<ExoSubJobAuth>();
                if (SelectedEntities == null && MainViewModel.Entities.Count > 0)
                    SelectedEntities.Add(MainViewModel.Entities.First());

                if (SelectedEntities == null || SelectedEntities.Count == 0)
                    return null;

                if (orderedAuthUsers == null)
                {
                    orderedAuthUsers = new List<ExoSubJobAuth>();
                    List<USER> projectLocaleUSERCollection = AllUSERCollection.ToList();
                    if (tryCombineLocalUsers)
                        projectLocaleUSERCollection.ForEach(x => x.ProjectLocale = loadPROJECT.OfficeNameForExo);

                    foreach (STAFF staff in exoSTAFFS)
                    {
                        ExoSubJobAuth displayUserAuth = new ExoSubJobAuth();
                        USER newUser = null;
                        if (tryCombineLocalUsers)
                            newUser = projectLocaleUSERCollection.FirstOrDefault(x => x.ProjectLocaleExoId == staff.STAFFNO);

                        if (newUser == null)
                        {
                            newUser = new USER();
                            newUser.ProjectLocale = loadPROJECT.OfficeNameForExo;
                        }
                        
                        if (!orderedAuthUsers.Any(x => x.User.ProjectLocaleExoId == staff.STAFFNO))
                        {
                            newUser.NAME = staff.NAME;
                            newUser.ProjectLocaleExoId = staff.STAFFNO;
                            newUser.TITLE = newUser.TITLE != null && newUser.TITLE != string.Empty ? newUser.TITLE : staff.JOBTITLE;
                            newUser.SecurityProfileID = staff.SECURITYPROFILEID;
                            displayUserAuth.User = newUser;

                            orderedAuthUsers.Add(displayUserAuth);
                        }
                    }
                }

                foreach (ExoSubJobAuth authorisation in orderedAuthUsers)
                {
                    if (SelectedEntities.All(x => x.AuthUserIds.Any(y => y == authorisation.User.ProjectLocaleExoId)))
                        authorisation.IsAssigned = true;
                    else if (SelectedEntities.Any(x => x.AuthUserIds.Any(y => y == authorisation.User.ProjectLocaleExoId)))
                        authorisation.IsAssigned = null;
                    else
                        authorisation.IsAssigned = false;

                    authorisation.ShouldAssign = false;
                }

                isPermissionLoading = false;
                this.RaisePropertyChanged(x => x.IsPermissionLoading);
                permissions.AddRange(orderedAuthUsers.OrderBy(x => x.User.Full_Name));
                return permissions;
            }
        }

        public virtual IEnumerable<ExoSubJobAuth> BluePrintsUsers
        {
            get
            {
                return new List<ExoSubJobAuth>();
            }
        }
#endregion

            #region View Properties
        protected bool isPermissionLoading;
        //if user clicks on an autofilter row and isPermissionLoading is true it won't be set to false ever and this can freeze up the view
        public bool IsPermissionLoading => !IsPermissionGridEnabled ? false : isPermissionLoading;

        public ExoSubJobAuth SelectedUser { get; set; }
        public override void ShowNotification()
        {
            if (AppNotificationService == null)
                return;

            INotification notification1 = AppNotificationService.CreatePredefinedNotification("Exo is connected to " + loadPROJECT.OfficeNameForExo, null, null, null);
            notification1.ShowAsync();
        }

        public override string ViewName
        {
            get
            {
                //return "BASELINE_ITEMSViewModelWrapper" + view_project_specific_affix;
                return "EXO_JobPermissionCollectionViewModelWrapper_v2";
            }
        }
        #endregion
    }
}