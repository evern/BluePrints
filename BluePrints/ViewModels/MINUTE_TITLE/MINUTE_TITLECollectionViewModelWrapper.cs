using System;
using System.Linq;
using DevExpress.Mvvm.POCO;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Data;
using BaseModel.ViewModel.Base;
using BaseModel.DataModel;
using BaseModel.ViewModel.Loader;
using System.Collections.Generic;
using System.Windows.Threading;
using System.ComponentModel;
using BaseModel.Misc;
using DevExpress.Xpf.Grid.TreeList;
using DevExpress.Mvvm;
using DevExpress.Xpf.Grid;
using System.Threading;
using BaseModel.ViewModel.Document;
using BluePrints.Common.Resources;
using System.Globalization;
using BluePrints.Common.Projections;
using BaseModel.Data.Helpers;
using BluePrints.Common;
using BluePrints.Common.Base;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the MINUTE_TITLE collection view model.
    /// </summary>
    public partial class MINUTE_TITLECollectionViewModelWrapper :
        BluePrintsEntitiesTreeCollectionWrapper
        <MINUTE_TITLE, MINUTE_TITLE, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of MINUTE_TITLECollectionViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static MINUTE_TITLECollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new MINUTE_TITLECollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the MINUTE_TITLECollectionViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the MINUTE_TITLECollectionViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected MINUTE_TITLECollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> BluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        public MEETING_TYPE loadMEETING_TYPE { get; set; }
        protected override void resolveParameters(object parameter)
        {
            var MEETING_TYPEParameter = (EntitiesParameter<MEETING_TYPE>)parameter;
            loadMEETING_TYPE = MEETING_TYPEParameter.GetEntity();
        }

        protected override void addEntitiesLoader()
        {
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(BluePrintsUnitOfWorkFactory, x => x.MINUTE_TITLES);
        }

        protected override Func<IRepositoryQuery<MINUTE_TITLE>, IQueryable<MINUTE_TITLE>>
            specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_MEETING_TYPE == loadMEETING_TYPE.GUID);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<MINUTE_TITLE> entities)
        {
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #endregion

        #region View Behavior
        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(MINUTE_TITLE projection, out bool isNew)
        {
            projection.GUID_MEETING_TYPE = loadMEETING_TYPE.GUID;
            projection.NUMBER = ((projection.SortOrder * 1.0) / 10).ToString();
            return base.OnBeforeProjectionSaveIsContinue(projection, out isNew);
        }

        /// <summary>
        /// Save expanded state before closing
        /// </summary>
        protected override void OnClose(CancelEventArgs e)
        {
            MainViewModel.BaseBulkSave(MainViewModel.Entities);
            base.OnClose(e);
        }
        #endregion

        public override string UnifiedRowValidation(MINUTE_TITLE projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(MINUTE_TITLE projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "MINUTE_TITLEViewModelWrapper"; }
        }

        #endregion
    }
}
