using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Helpers;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Loader;
using BaseModel.ViewModel.UndoRedo;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Filtering;
using BluePrints.Common.Projections;
using BluePrints.Common.Reports;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.Reports;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.DragDrop;
using DevExpress.Xpf.Printing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Linq.Expressions;
using DevExpress.Data.Filtering;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using BluePrints.PrimeroData;
using BluePrints.Common.Misc;
using System.Net.Mail;
using Microsoft.Exchange.WebServices.Data;
using BluePrints.P6EntitiesDataModel;
using System.ComponentModel;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single BASELINE object view model.
    /// </summary>
    public partial class DOCCONTROL_BASELINE_ITEMCollectionViewModelWrapper : BASELINE_ITEMCollectionViewModelWrapper
    {
        protected override void resolveParameters(object parameter)
        {
            InternalNumberMode = DeliverableInternalNumberMode.AlwaysEditable;
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.SUBJOBS, SUBJOBProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES, DOCTYPEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DELIVERABLES_STATUSES, DELIVERABLES_STATUSProjectionFunc);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);
            loaderCollection.AddLoaderDescription<OFFICE, OFFICE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.OFFICES);
        }

        protected override Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.STATUS == ProjectStatus.Active);
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM_WORK>, IQueryable<BASELINE_ITEM_WORK>> BASELINE_ITEM_WORKProjectionFunc()
        {
            return query => query;
        }

        protected override Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> BASELINEProjectionFunc()
        {
            return query => query.Where(x => x.STATUS == BaselineStatus.Live && x.PROJECT.STATUS == ProjectStatus.Active);
        }

        protected override Func<IRepositoryQuery<REGISTER_HOLD>, IQueryable<REGISTER_HOLD>> REGISTER_HOLDProjectionFunc()
        {
            return query => query.Where(x => x.PROJECT.STATUS == ProjectStatus.Active);
        }

        protected override Func<IRepositoryQuery<SUBJOB>, IQueryable<SUBJOB>> SUBJOBProjectionFunc()
        {
            return query => query.Where(x => x.PROJECT.STATUS == ProjectStatus.Active);
        }

        protected override Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> WORKPACKProjectionFunc()
        {
            return query => query;
        }

        protected override Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.PROJECT.STATUS == ProjectStatus.Active);
        }

        protected override Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.PROJECT.STATUS == ProjectStatus.Active);
        }

        protected override Func<IRepositoryQuery<DELIVERABLES_STATUS>, IQueryable<DELIVERABLES_STATUS>> DELIVERABLES_STATUSProjectionFunc()
        {
            return query => query.Where(x => x.PROJECT.STATUS == ProjectStatus.Active);
        }

        protected override Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            return query => query.Where(x => x.PROJECT.STATUS == ProjectStatus.Active && x.TYPE == PhaseType.Design && x.STATUS == ProgressStatus.Live);
        }

        protected override Func<IRepositoryQuery<PROGRESS_ITEM>, IQueryable<PROGRESS_ITEM>> PROGRESS_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.PROGRESS.STATUS == ProgressStatus.Live && x.PROGRESS.PROJECT.STATUS == ProjectStatus.Active);
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEMProgress>> specifyMainViewModelProjection()
        {
            return query => ProgressQueries.DocControlProgressItemTransformation(query, PROGRESS_ITEMCollection);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS);
        }

        List<BASELINE> internalNumberBaselines = new List<BASELINE>();
        List<BASELINE> clientNumberBaselines = new List<BASELINE>();
        public bool CanApproveSelectedInternalNumbers => SelectedEntities.Count() > 0;
        public void ApproveSelectedInternalNumbers()
        {
            if (LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_DesignDeliverables_InternalNumbersApproval)) == LoginCredentials.PermissionStatus.None)
            {
                MessageBoxService.ShowMessage("You do not have the authority to approve internal numbers", "Unauthorised");
                return;
            }

            if (MessageBoxService.ShowMessage("This will lock selected internal numbers, are you sure you want to continue?", "Confirmation", MessageButton.OKCancel) == MessageResult.OK)
            {
                foreach (BASELINE_ITEMProgress entity in SelectedEntities)
                {
                    if(entity.Entity.Entity.INTERNALNUM_STATUS == DocumentNumberStatus.Awaiting)
                        entity.Entity.Entity.INTERNALNUM_STATUS = DocumentNumberStatus.Approved;

                    BASELINE entityBASELINE = entity.Entity.Entity.BASELINE;
                    if (!internalNumberBaselines.Any(x => x.GUID_PROJECT == entityBASELINE.GUID_PROJECT))
                        internalNumberBaselines.Add(entityBASELINE);
                }

                MainViewModel.SaveChangesDirectly();
                GridControlService.RefreshData();
            };
        }

        public bool CanApproveSelectedClientNumbers => SelectedEntities.Count() > 0;
        public void ApproveSelectedClientNumbers()
        {
            if (LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_DesignDeliverables_InternalNumbersApproval)) == LoginCredentials.PermissionStatus.None)
            {
                MessageBoxService.ShowMessage("You do not have the authority to approve client numbers", "Unauthorised");
                return;
            }

            if (MessageBoxService.ShowMessage("This will lock selected client numbers, are you sure you want to continue?", "Confirmation", MessageButton.OKCancel) == MessageResult.OK)
            {
                foreach (BASELINE_ITEMProgress entity in SelectedEntities)
                {
                    if (entity.Entity.Entity.CLIENTNUM_STATUS == DocumentNumberStatus.Awaiting)
                        entity.Entity.Entity.CLIENTNUM_STATUS = DocumentNumberStatus.Approved;
                    
                    BASELINE entityBASELINE = entity.Entity.Entity.BASELINE;
                    if (!clientNumberBaselines.Any(x => x.GUID_PROJECT == entityBASELINE.GUID_PROJECT))
                        clientNumberBaselines.Add(entityBASELINE);
                }

                MainViewModel.SaveChangesDirectly();
                GridControlService.RefreshData();
            };
        }

        public void SendEmail()
        {
            List<EmailReport> emailReportList = new List<EmailReport>();
            foreach (BASELINE internalNumberBaseline in internalNumberBaselines)
            {
                USER user = USERCollection.FirstOrDefault(x => x.GUID == internalNumberBaseline.FIN_INTERNALNUM_BY);
                if (user != null)
                {
                    emailReportList.Add(new EmailReport() { Number = internalNumberBaseline.PROJECT.NUMBER, Sent = user.NAME, Type = "Internal Number" });
                    EmailService.SendEmailFromDocControl(LoginCredentials.CurrentUser.NAME, "Deliverable(s) internal number in project " + internalNumberBaseline.PROJECT.NUMBER + " has been approved!", "Deliverable Internal Numbers Approved for " + internalNumberBaseline.PROJECT.NUMBER, user.NAME + "@primero.com.au");
                }
            }

            foreach (BASELINE clientNumberBaselines in clientNumberBaselines)
            {
                USER user = USERCollection.FirstOrDefault(x => x.GUID == clientNumberBaselines.FIN_INTERNALNUM_BY);
                if (user != null)
                {
                    emailReportList.Add(new EmailReport() { Number = clientNumberBaselines.PROJECT.NUMBER, Sent = user.NAME, Type = "Client Number" });
                    EmailService.SendEmailFromDocControl(LoginCredentials.CurrentUser.NAME, "Deliverable(s) client number in project " + clientNumberBaselines.PROJECT.NUMBER + " has been approved!", "Deliverable Client Numbers Approved for " + clientNumberBaselines.PROJECT.NUMBER, user.NAME + "@primero.com.au");
                }
            }

            if (emailReportList.Count == 0)
            {
                MessageBox.Show("Nothing to send because no approval has been made");
                return;
            }

            DialogCollectionViewModel<EmailReport> viewModel = DialogCollectionViewModel<EmailReport>.Create(emailReportList);
            ReportDialogService.ShowDialog(MessageButton.OK, "Email Report", "EmailSentReport", viewModel);
            emailReportList.Clear();
        }

        public override string ViewName => "DOCCONTROL_BASELINE_ITEMCollection_v2";

        private DevExpress.Mvvm.IDialogService ReportDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("ReportDialogService"); }
        }
    }

    public class EmailReport
    {
        public string Number { get; set; }
        public string Sent { get; set; }
        public string Type { get; set; }
    }
}