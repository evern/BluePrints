using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Dialogs;
using BluePrints.Common.Misc;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BluePrints.Common.ViewModel
{
    public class VariationApprovalViewModel<TEntity> : DialogCollectionViewModel<VariationApprovalAction<TEntity>>
        where TEntity : class, IDeliverable, ISupportVariationRevision, new()
    {
        public static VariationApprovalViewModel<TEntity> CreateViewModel(IEnumerable<VariationApprovalAction<TEntity>> enumerableObjects)
        {
            return ViewModelSource.Create(() => new VariationApprovalViewModel<TEntity>(enumerableObjects));
        }

        public VariationApprovalViewModel(IEnumerable<VariationApprovalAction<TEntity>> enumerableObjects)
            : base(enumerableObjects)
        {
            selectedDeliverables = new ObservableCollection<VariationApprovalAction<TEntity>>();
        }

        ObservableCollection<VariationApprovalAction<TEntity>> selectedDeliverables { get; set; }
        public ObservableCollection<VariationApprovalAction<TEntity>> SelectedDeliverables
        {
            get { return selectedDeliverables; }
            set { selectedDeliverables = value; }
        }

        public bool CanUncheckEarnedReduction(object button)
        {
            return SelectedDeliverables.Count > 0;
        }

        public void UncheckEarnedReduction(object button)
        {
            BulkEditEarnedReduction(button, false);
        }

        public bool CanCheckEarnedReduction(object button)
        {
            return CanUncheckEarnedReduction(button);
        }

        public void CheckEarnedReduction(object button)
        {
            BulkEditEarnedReduction(button, true);
        }

        public void BulkEditEarnedReduction(object button, bool isChecked)
        {
            GridMenuInfo info = GridPopupMenuBase.GetGridMenuInfo((DependencyObject)button) as GridMenuInfo;
            bool valueToFill = isChecked;

            foreach(VariationApprovalAction<TEntity> selectedDeliverable in SelectedDeliverables)
            {
                selectedDeliverable.ReduceEarned = valueToFill;
                selectedDeliverable.Update();
            }
        }
    }
}
