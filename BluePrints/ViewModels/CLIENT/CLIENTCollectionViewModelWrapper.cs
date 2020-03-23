using BaseModel.DataModel;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Data;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class CLIENTCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <CLIENT, CLIENT, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of CLIENTCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static CLIENTCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new CLIENTCollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the CLIENTCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the CLIENTCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected CLIENTCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void resolveParameters(object parameter)
        {
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.PROJECTS);
            loaderCollection.AddLoaderDescription<CLIENT_PROJECT, CLIENT_PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.CLIENT_PROJECTS);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.CLIENTS);
        }

        protected override Func<IRepositoryQuery<CLIENT>, IQueryable<CLIENT>> specifyMainViewModelProjection()
        {
            return query => populateClientProject(query);
        }

        private IQueryable<CLIENT> populateClientProject(IQueryable<CLIENT> query)
        {
            List<CLIENT> clients = query.ToList();
            //need to call ToList for tokenComboBoxEditSettings to work
            clients.ForEach(x => x.Projects = PROJECTCollection.Where(project => CLIENT_PROJECTCollection.Any(clientproject => clientproject.GUID_CLIENT == x.GUID && clientproject.GUID_PROJECT == project.GUID)).ToList());

            return clients.AsQueryable();
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<CLIENT> entities)
        {
            MainViewModel.OnAfterEntitySavedCallBack = onAfterEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #endregion

        #region Saving Behavior
        private void onAfterEntitySaved(CLIENT entity, CLIENT projection, bool isNewEntity)
        {
            save_project_assignments(entity);
        }

        private void save_project_assignments(CLIENT entity)
        {
            if(entity.Project_Assignments != null)
            {
                List<CLIENT_PROJECT> remove_projects = new List<CLIENT_PROJECT>();
                foreach (CLIENT_PROJECT assignment in CLIENT_PROJECTCollection.Where(x => x.GUID_CLIENT == entity.GUID))
                {
                    if (!entity.Project_Assignments.Any(x => x.GUID == assignment.GUID))
                        remove_projects.Add(assignment);
                }

                CLIENT_PROJECTCollectionViewModel.BaseBulkDelete(remove_projects);

                List<CLIENT_PROJECT> add_projects = new List<CLIENT_PROJECT>();
                foreach (PROJECT project in entity.Project_Assignments)
                {
                    if (!entity.CLIENT_PROJECT.Any(x => x.GUID == project.GUID))
                        add_projects.Add(new CLIENT_PROJECT() { GUID_PROJECT = project.GUID, GUID_CLIENT = entity.GUID });
                }

                CLIENT_PROJECTCollectionViewModel.BulkSave(add_projects);
            }
            else
            {
                List<CLIENT_PROJECT> remove_projects = new List<CLIENT_PROJECT>();
                foreach (CLIENT_PROJECT assignment in CLIENT_PROJECTCollection.Where(x => x.GUID_CLIENT == entity.GUID))
                {
                    remove_projects.Add(assignment);
                }

                CLIENT_PROJECTCollectionViewModel.BaseBulkDelete(remove_projects);
            }
        }

        public override string UnifiedRowValidation(CLIENT projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(CLIENT projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }
        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "CLIENTCollectionViewModelWrapper"; }
        }

        public IEnumerable<CLIENT_PROJECT> CLIENT_PROJECTCollection
        {
            get
            {
                var collection = GetEntities<CLIENT_PROJECT>();
                if (collection == null)
                    return new List<CLIENT_PROJECT>();

                return collection;
            }
        }

        public IEnumerable<PROJECT> PROJECTCollection
        {
            get
            {
                var collection = GetEntities<PROJECT>();
                if (collection == null)
                    return new List<PROJECT>();

                //need to call ToList for tokenComboBoxEditSettings to work
                return collection.OrderBy(x => x.NUMBER).ToList();
            }
        }

        public CollectionViewModel<CLIENT_PROJECT, CLIENT_PROJECT, Guid, IBluePrintsEntitiesUnitOfWork> CLIENT_PROJECTCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return
                    (CollectionViewModel<CLIENT_PROJECT, CLIENT_PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<CLIENT_PROJECT>();
            }
        }
        #endregion
    }
}