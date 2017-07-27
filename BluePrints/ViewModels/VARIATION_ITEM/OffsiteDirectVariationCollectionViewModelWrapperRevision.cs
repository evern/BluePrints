using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single VARIATION object view model.
    /// </summary>
    public partial class OffsiteDirectVariationCollectionViewModelWrapperRevision :
        BluePrintsEntitiesCollectionWrapper
        <BASELINE_ITEM, BASELINE_ITEMVariation, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        //Used by variation to generate new baseline
        public Func<object> OnEntitiesLoadedParameterCallBack;
        public Action<IEnumerable<BASELINE_ITEMVariation>, object> OnEntitiesLoadedWithParameterCallBack;

        /// <summary>
        /// Creates a new instance of VARIATION_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static OffsiteDirectVariationCollectionViewModelWrapperRevision Create()
        {
            return ViewModelSource.Create(() => new OffsiteDirectVariationCollectionViewModelWrapperRevision());
        }

        BASELINE_ITEMCollectionViewModelWrapper baseline_itemCollectionViewModelWrapper;


    }
}