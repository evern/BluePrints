using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.ViewModels
{
    public class PROJECTPROGRESSDetailsCollectionViewModel : DetailsFilterableSingleObjectViewModel<PROJECT, PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Initializes a new instance of the PROJECTViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected PROJECTPROGRESSDetailsCollectionViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
            : base(unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.PROJECTS, x => x.NUMBER)
        {
        }

        ISupportCustomDocumentPROGRESSCollectionViewModel collectionViewModel;
        /// <summary>
        /// The view model for the PROJECTPROGRESSES detail collection.
        /// </summary>
        public ISupportCustomDocumentPROGRESSCollectionViewModel PROJECTPROGRESSESDetails
        {
            get
            {
                if(collectionViewModel == null && this.PrimaryKey != Guid.Empty)
                {
                    collectionViewModel = ISupportCustomDocumentPROGRESSCollectionViewModel.Create(this.Entity, query => query.Where(x => x.GUID_PROJECT == this.Entity.GUID));
                    collectionViewModel.OnBeforeEntitySavedCallBack = this.OnBeforeEntitySaved;
                    collectionViewModel.SetParentViewModel(this);
                }

                return collectionViewModel;
            }
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            this.RaisePropertyChanged(x => x.PROJECTPROGRESSESDetails);
        }

        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public void OnBeforeEntitySaved(PROGRESS entity)
        {
            entity.GUID_PROJECT = this.Entity.GUID;
            entity.PROGRESS_START = entity.PROGRESS_START.Date;
            entity.DATA_DATE = entity.DATA_DATE.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
        }

        protected override void OnDestroy()
        {
            collectionViewModel.OnBeforeEntitySavedCallBack = null;
            collectionViewModel.SetParentViewModel(null);
            collectionViewModel.OnDestroy();
            collectionViewModel = null;

            base.OnDestroy();
        }
    }



    public class ISupportCustomDocumentPROGRESSCollectionViewModel : CollectionViewModel<PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork>, ISupportCustomDocumentTypeNameAndParameter
    {
        /// <summary>
        /// Creates a new instance of PROGRESSCollectionViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static ISupportCustomDocumentPROGRESSCollectionViewModel Create(PROJECT project, Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> projection = null)
        {
            return ViewModelSource.Create(() => new ISupportCustomDocumentPROGRESSCollectionViewModel(project, projection));
        }

        PROJECT thisPROJECT;
        /// <summary>
        /// Initializes a new instance of the PROGRESSCollectionViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROGRESSCollectionViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected ISupportCustomDocumentPROGRESSCollectionViewModel(PROJECT project, Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> projection = null)
            : base(BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.PROGRESSES, projection)
        {
            thisPROJECT = project;
        }

        #region ISupportCustomDocumentTypeNameAndParameter
        public string GetCustomDocumentTypeName()
        {
            return "PROGRESS_ITEMCollectionView";
        }

        public object GetCustomDocumentParameter()
        {
            return new OptionalEntitiesParameter<PROJECT, PROGRESS>(null, SelectedEntity);
        }

        public string GetCustomDocumentTitle()
        {
            return "[" + thisPROJECT.NUMBER + "] PROGRESS";
        }

        public bool IsCustomModeEnabled()
        {
            return true;
        }
        #endregion
    }
}
