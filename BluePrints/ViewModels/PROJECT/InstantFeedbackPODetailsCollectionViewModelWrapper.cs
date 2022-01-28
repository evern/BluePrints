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
using BluePrints.Common.Utils;
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
    public partial class InstantFeedbackPODetailsCollectionViewModelWrapper : BluePrintsEntitiesCollectionWrapper<X_PURCHORD_LINE_DETAILS_VIEW_V1, X_PURCHORD_LINE_DETAILS_VIEW_V1, int, IPrimeroEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of PROGRESS_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static InstantFeedbackPODetailsCollectionViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new InstantFeedbackPODetailsCollectionViewModelWrapper());
        }

        public bool IsCostsVisible { get; set; }
        public bool CanEditQuantity { get; set; }
        protected override string readOnlyMessage => "Cells are read only because you do not have authority to edit transactions";
        protected InstantFeedbackPODetailsCollectionViewModelWrapper()
        {
            IsInstantFeedbackMode = true;
        }

        #region Database Operation
        private IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected override void resolveParameters(object parameter)
        {
        }

        protected override void addEntitiesLoader()
        {
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(primeroUnitOfWorkFactory, x => x.X_PURCHORD_LINE_DETAILS_VIEW_V1);
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            IsPasteCellLevel = true;
        }

        protected override Func<IRepositoryQuery<X_PURCHORD_LINE_DETAILS_VIEW_V1>, IQueryable<X_PURCHORD_LINE_DETAILS_VIEW_V1>> specifyMainViewModelProjection()
        {
            return query => query.OrderByDescending(x => x.ORDERDATE);
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
            get { return "InstantFeedbackPODetailsCollectionViewModelWrapper_v1"; }
        }
        #endregion

        public override string UnifiedValueValidation(X_PURCHORD_LINE_DETAILS_VIEW_V1 projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }

        public override string UnifiedRowValidation(X_PURCHORD_LINE_DETAILS_VIEW_V1 projection)
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

