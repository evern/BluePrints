using BaseModel.DataModel;
using BaseModel.DataModel.DesignTime;
using BaseModel.DataModel.EntityFramework;
using BluePrints.Data;
using DevExpress.Mvvm;

namespace BluePrints.BluePrintsEntitiesDataModel
{
    /// <summary>
    /// Provides methods to obtain the relevant IUnitOfWorkFactory.
    /// </summary>
    public static class BluePrintsEntitiesUnitOfWorkSource
    {
        /// <summary>
        /// Returns the IUnitOfWorkFactory implementation based on the current mode (run-time or design-time).
        /// </summary>
        public static IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> GetUnitOfWorkFactory()
        {
            return GetUnitOfWorkFactory(ViewModelBase.IsInDesignMode);
        }

        /// <summary>
        /// Returns the IUnitOfWorkFactory implementation based on the given mode (run-time or design-time).
        /// </summary>
        /// <param name="isInDesignTime">Used to determine which implementation of IUnitOfWorkFactory should be returned.</param>
        public static IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> GetUnitOfWorkFactory(bool isInDesignTime)
        {
            if (isInDesignTime)
                return
                    new DesignTimeUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork>(
                        () => new BluePrintsEntitiesDesignTimeUnitOfWork());
            return
                new DbUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork>(
                    () => new BluePrintsEntitiesUnitOfWork(() => new BluePrintsEntities()));
        }
    }
}