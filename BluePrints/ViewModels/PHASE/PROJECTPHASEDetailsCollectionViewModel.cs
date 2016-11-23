using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.ViewModels
{
    public class PROJECTPHASEDetailsCollectionViewModel : DetailsFilterableSingleObjectViewModel<PROJECT, PHASE, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Initializes a new instance of the PROJECTViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected PROJECTPHASEDetailsCollectionViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
            : base(unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.PROJECTS, x => x.NUMBER)
        {
        }

        /// <summary>
        /// The view model for the PROJECTPHASES detail collection.
        /// </summary>
        public CollectionViewModel<PHASE, Guid, IBluePrintsEntitiesUnitOfWork> PROJECTPHASESDetails
        {
            get { return GetDetailsCollectionViewModel((PROJECTViewModel x) => x.PROJECTPHASESDetails, x => x.PHASES, x => x.GUID_PROJECT, (x, key) => { x.GUID_PROJECT = key; }); }
        }
    }
}
