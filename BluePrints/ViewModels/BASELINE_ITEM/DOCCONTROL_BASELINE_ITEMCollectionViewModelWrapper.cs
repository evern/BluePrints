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
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }


        public bool CanApproveSelectedInternalNumbers => SelectedEntities.Count() > 0;
        public void ApproveSelectedInternalNumbers()
        {
            if (!LoginCredentials.hasPermission(PermissionResources.UnapproveInternalNumbers))
            {
                MessageBoxService.ShowMessage("You do not have the authority to unapprove internal numbers", "Unauthorised");
                return;
            }

            if (MessageBoxService.ShowMessage("This will lock selected internal numbers, are you sure you want to continue?", "Confirmation", MessageButton.OKCancel) == MessageResult.OK)
            {
                foreach (BASELINE_ITEMProgress entity in SelectedEntities)
                {
                    if(entity.Entity.Entity.INTERNALNUM_STATUS == DocumentNumberStatus.Awaiting)
                        entity.Entity.Entity.INTERNALNUM_STATUS = DocumentNumberStatus.Approved;
                }

                MainViewModel.SimpleSaveAll();
                GridControlService.RefreshData();
            };
        }

        public bool CanApproveSelectedClientNumbers => SelectedEntities.Count() > 0;
        public void ApproveSelectedClientNumbers()
        {
            if (!LoginCredentials.hasPermission(PermissionResources.UnapproveInternalNumbers))
            {
                MessageBoxService.ShowMessage("You do not have the authority to unapprove client numbers", "Unauthorised");
                return;
            }

            if (MessageBoxService.ShowMessage("This will lock selected client numbers, are you sure you want to continue?", "Confirmation", MessageButton.OKCancel) == MessageResult.OK)
            {
                foreach (BASELINE_ITEMProgress entity in SelectedEntities)
                {
                    if (entity.Entity.Entity.CLIENTNUM_STATUS == DocumentNumberStatus.Awaiting)
                        entity.Entity.Entity.CLIENTNUM_STATUS = DocumentNumberStatus.Approved;
                }

                MainViewModel.SimpleSaveAll();
                GridControlService.RefreshData();
            };
        }
    }
}