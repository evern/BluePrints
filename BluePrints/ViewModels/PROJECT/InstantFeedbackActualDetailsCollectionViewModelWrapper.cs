using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Loader;
using BaseModel.ViewModel.UndoRedo;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Filtering;
using BluePrints.Common.Misc;
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
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Core.ConditionalFormatting;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Printing;
using DevExpress.XtraEditors.DXErrorProvider;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single PROGRESS object view model.
    /// </summary>
    public partial class InstantFeedbackActualDetailsCollectionViewModelWrapper : BluePrintsEntitiesCollectionWrapper<X_JOB_TRANSACTIONS_DETAIL_V2, X_JOB_TRANSACTIONS_DETAIL_V2, int, IPrimeroEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of PROGRESS_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static InstantFeedbackActualDetailsCollectionViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new InstantFeedbackActualDetailsCollectionViewModelWrapper());
        }

        public bool IsCostsVisible { get; set; }
        public bool CanEditQuantity { get; set; }
        protected override string readOnlyMessage => "Cells are read only because you do not have authority to edit transactions";
        protected InstantFeedbackActualDetailsCollectionViewModelWrapper()
        {
            IsInstantFeedbackMode = true;
        }

        #region Database Operation
        Data.PROJECT loadPROJECT;
        bool loadAll;
        private readonly IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory;
        protected override void resolveParameters(object parameter)
        {
            if(parameter != null)
            {
                bool? isLoadAll = parameter as bool?;
                if(isLoadAll == null)
                {
                    loadPROJECT = (Data.PROJECT)parameter;
                    primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(loadPROJECT.OfficeNameForExo);
                }
                else
                {
                    loadAll = true;
                    primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
                }
            }
            else
                primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        }

        protected override void addEntitiesLoader()
        {
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(primeroUnitOfWorkFactory, x => x.X_JOB_TRANSACTIONS_DETAIL_V2);
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            IsPasteCellLevel = true;
        }

        protected override Func<IRepositoryQuery<X_JOB_TRANSACTIONS_DETAIL_V2>, IQueryable<X_JOB_TRANSACTIONS_DETAIL_V2>> specifyMainViewModelProjection()
        {
            if (loadAll)
                return query => query;
            else if(loadPROJECT == null)
                return query => query.Where(x => x.MASTER_JOBCODE == "X");
            else
                return query => query.Where(x => x.MASTER_JOBCODE == loadPROJECT.NUMBER);
        }

        public override void FullRefresh()
        {
            base.FullRefresh();
        }
#endregion

#region View Properties
        public override void OnAfterAuxiliaryEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, Guid senderKey, bool isBulkRefresh)
        {
            base.OnAfterAuxiliaryEntitiesChanged(key, changedType, messageType, sender, senderKey, isBulkRefresh);
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            //get { return "OffsiteDirectProgressViewModelWrapper" + view_project_specific_affix; }
            get { return "InstantFeedbackActualDetailsCollectionViewModelWrapper_v1" + view_project_specific_affix; }
        }

        private DevExpress.Mvvm.IDialogService DateFromToDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("DateFromToDialogService"); }
        }

        private string view_project_specific_affix
        {
            get
            {
                if (loadPROJECT == null)
                    return string.Empty;
                return loadPROJECT.GUID.ToString();
            }
        }
        #endregion

        public override string UnifiedValueValidation(X_JOB_TRANSACTIONS_DETAIL_V2 projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }

        public override string UnifiedRowValidation(X_JOB_TRANSACTIONS_DETAIL_V2 projection)
        {
            return string.Empty;
        }

        public override bool CanKeyboardPaste()
        {
            if (IsReadOnly)
                return false;

            return base.CanKeyboardPaste();
        }

        public override void PastingFromClipboard(PastingFromClipboardEventArgs e)
        {
            if (IsReadOnly)
                return;

            base.PastingFromClipboard(e);
        }
    }
}

