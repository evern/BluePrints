using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Dialogs;
using BluePrints.Common.Misc;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
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
        }
    }
}
