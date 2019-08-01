using BaseModel.DataModel;
using BaseModel.DataModel.DesignTime;
using BaseModel.DataModel.EntityFramework;
using DevExpress.Mvvm;

namespace BluePrints.PrimeroData.PrimeroEntitiesDataModel
{
    /// <summary>
    /// Provides methods to obtain the relevant IUnitOfWorkFactory.
    /// </summary>
    public static class PrimeroEntitiesUnitOfWorkSource
    {
        /// <summary>
        /// Returns the IUnitOfWorkFactory implementation based on the current mode (run-time or design-time).
        /// </summary>
        public static IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> GetUnitOfWorkFactory(bool isRemote = false)
        {
            return GetUnitOfWorkFactory(ViewModelBase.IsInDesignMode, isRemote);
        }

        /// <summary>
        /// Returns the IUnitOfWorkFactory implementation based on the given mode (run-time or design-time).
        /// </summary>
        /// <param name="isInDesignTime">Used to determine which implementation of IUnitOfWorkFactory should be returned.</param>
        public static IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> GetUnitOfWorkFactory(bool isInDesignTime, bool isRemote)
        {
            if(isRemote)
                return
                    new DbUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork>(
                        () => new PrimeroEntitiesUnitOfWork(() => new PGAEntities()));
            else if (isInDesignTime)
                return
                    new DesignTimeUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork>(
                        () => new PrimeroEntitiesDesignTimeUnitOfWork());
            else
                return
                    new DbUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork>(
                        () => new PrimeroEntitiesUnitOfWork(() => new PrimeroEntities()));
        }
    }
}