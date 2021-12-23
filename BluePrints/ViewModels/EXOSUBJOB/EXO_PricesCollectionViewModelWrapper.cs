using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Mvvm;
using DevExpress.Xpf.Grid;

namespace BluePrints.ViewModels
{
    public class EXO_PricesCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <DR_PRICES, DR_PRICES, int, IPrimeroEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of EXO_PricePolicyCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static EXO_PricesCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new EXO_PricesCollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// Initializes a new instance of the EXO_PricePolicyCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the EXO_PricePolicyCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected EXO_PricesCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        private IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory;
        private PROJECT loadPROJECT;
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();

            primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(loadPROJECT.OfficeNameForExo);
        }

        DR_PRICE_POLICY projectDR_PRICE_POLICY { get; set; }
        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<PrimeroData.STOCK_ITEMS, PrimeroData.STOCK_ITEMS, string, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.STOCK_ITEMS);
            loaderCollection.AddLoaderDescription(primeroUnitOfWorkFactory, x => x.DR_PRICE_POLICY, DR_PRICE_POLICYProjectionFunc, x => assignPricePolicy(x));
        }

        protected virtual Func<IRepositoryQuery<DR_PRICE_POLICY>, IQueryable<DR_PRICE_POLICY>> DR_PRICE_POLICYProjectionFunc()
        {
            return query => query.Where(x => x.POLICY_REF == loadPROJECT.NUMBER);
        }

        private void assignPricePolicy(DR_PRICE_POLICY DR_PRICE_POLICY)
        {
            if (DR_PRICE_POLICY == null)
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("There is no Policy Ref of " + loadPROJECT.NUMBER + " created yet, please create a price policy on this project with Policy Ref: " + loadPROJECT.NUMBER)));

            projectDR_PRICE_POLICY = DR_PRICE_POLICY;
        }
        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(primeroUnitOfWorkFactory, x => x.DR_PRICES);
        }

        protected override Func<IRepositoryQuery<DR_PRICES>, IQueryable<DR_PRICES>> specifyMainViewModelProjection()
        {
            return query => DR_PRICESQuery(query);
        }

        private IQueryable<DR_PRICES> DR_PRICESQuery(IQueryable<DR_PRICES> DR_PRICES)
        {
            List<DR_PRICES> projectDR_PRICES = DR_PRICES.Where(x => x.POLICY_HDR == projectDR_PRICE_POLICY.POLICY_HDR).ToList();
            projectDR_PRICES.ForEach(x => assignEntityStockItems(x));
            return projectDR_PRICES.AsQueryable();
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<DR_PRICES> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #endregion
        #region Collection Call Backs
        public override void UnifiedNewRowInitializationFromView(DR_PRICES projection)
        {
            projection.POLICY_HDR = projectDR_PRICE_POLICY.POLICY_HDR;
            projection.STARTDATE = projectDR_PRICE_POLICY.START_DATE;
            projection.STOPDATE = projectDR_PRICE_POLICY.END_DATE;
            projection.MINQTY = 0;
            projection.ACCGROUP = -1;
            projection.FREIGHT_FREE = "N";
            projection.SELL_PRICE_BANDNO = -1;
            projection.MASTER_JOBNO = 0;
            projection.JOBNO = 0;
            projection.CAMPAIGN_WAVE_SEQNO = -1;
            base.UnifiedNewRowInitializationFromView(projection);
        }

        public override void CellValueChanged(CellValueChangedEventArgs e)
        {
            if(e.Column.FieldName == BindableBase.GetPropertyName(() => new DR_PRICES().STOCKCODE))
            {
                DR_PRICES entity = (DR_PRICES)e.Row;
                assignEntityStockItems(entity);
                entity.Update();
            }

            base.CellValueChanged(e);
        }

        private void assignEntityStockItems(DR_PRICES entity)
        {
            STOCK_ITEMS selectedSTOCK_ITEMS = STOCK_ITEMSCollection.FirstOrDefault(x => x.STOCKCODE == entity.STOCKCODE);
            entity.AssignSTOCK_ITEMS(selectedSTOCK_ITEMS);
        }

        public override string UnifiedValueValidation(DR_PRICES projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }

        public override string UnifiedRowValidation(DR_PRICES projection)
        {
            return string.Empty;
        }

        #endregion

        #region View Properties
        public IEnumerable<PrimeroData.STOCK_ITEMS> STOCK_ITEMSCollection
        {
            get
            {
                var collection = GetEntities<PrimeroData.STOCK_ITEMS>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.STOCKCODE);
                return collection;
            }
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "EXO_PricePolicyCollectionViewModelWrapper_v2"; }
        }
        #endregion
    }
}