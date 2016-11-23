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
    public class PROJECTRATEDetailsCollectionViewModel : DetailsFilterableSingleObjectViewModel<PROJECT, RATE, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Initializes a new instance of the PROJECTViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected PROJECTRATEDetailsCollectionViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
            : base(unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.PROJECTS, x => x.NUMBER)
        {
        }

        /// <summary>
        /// The view model for the PROJECTRATES detail collection.
        /// </summary>
        public CollectionViewModel<RATE, Guid, IBluePrintsEntitiesUnitOfWork> PROJECTRATESDetails
        {
            get { return GetDetailsCollectionViewModel((PROJECTViewModel x) => x.PROJECTRATESDetails, x => x.RATES, x => x.GUID_PROJECT, (x, key) => { x.GUID_PROJECT = key; }); }
        }

        /// <summary>
        /// The view model that contains a look-up collection of DEPARTMENTS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<DEPARTMENT> LookUpDEPARTMENTS
        {
            get { return GetLookUpEntitiesViewModel((PROJECTViewModel x) => x.LookUpDEPARTMENTS, x => x.DEPARTMENTS); }
        }

        /// <summary>
        /// The view model that contains a look-up collection of DISCIPLINES for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<DISCIPLINE> LookUpDISCIPLINES
        {
            get { return GetLookUpEntitiesViewModel((PROJECTViewModel x) => x.LookUpDISCIPLINES, x => x.DISCIPLINES); }
        }
    }

}
