using BaseModel.DataModel;
using BaseModel.DataModel.DesignTime;
using BaseModel.DataModel.EntityFramework;
using BluePrints.P6Data;
using DevExpress.Mvvm;

namespace BluePrints.P6EntitiesDataModel
{
    /// <summary>
    /// Provides methods to obtain the relevant IUnitOfWorkFactory.
    /// </summary>
    public static class P6EntitiesUnitOfWorkSource
    {
        /// <summary>
        /// Returns the IUnitOfWorkFactory implementation based on the current mode (run-time or design-time).
        /// </summary>
        public static IUnitOfWorkFactory<IP6EntitiesUnitOfWork> GetUnitOfWorkFactory(IP6EntitiesUnitOfWork p6EntitiesUnitOfWork = null)
        {
            return GetUnitOfWorkFactory(ViewModelBase.IsInDesignMode, p6EntitiesUnitOfWork);
        }

        /// <summary>
        /// Returns the IUnitOfWorkFactory implementation based on the given mode (run-time or design-time).
        /// </summary>
        /// <param name="isInDesignTime">Used to determine which implementation of IUnitOfWorkFactory should be returned.</param>
        public static IUnitOfWorkFactory<IP6EntitiesUnitOfWork> GetUnitOfWorkFactory(bool isInDesignTime, bool isNew)
        {
            if (isNew)
                return
                    new DbUnitOfWorkFactory<IP6EntitiesUnitOfWork>(
                        () => new P6EntitiesUnitOfWork(() => new P6NewEntities()));
            else
                return
                    new DbUnitOfWorkFactory<IP6EntitiesUnitOfWork>(
                        () => new P6EntitiesUnitOfWork(() => new P6Entities()));
        }

        /// <summary>
        /// Returns the IUnitOfWorkFactory implementation based on the given mode (run-time or design-time).
        /// </summary>
        /// <param name="isInDesignTime">Used to determine which implementation of IUnitOfWorkFactory should be returned.</param>
        public static IUnitOfWorkFactory<IP6EntitiesUnitOfWork> GetUnitOfWorkFactory(bool isInDesignTime, IP6EntitiesUnitOfWork p6EntitiesUnitOfWork = null)
        {
            if (isInDesignTime)
                return new DesignTimeUnitOfWorkFactory<IP6EntitiesUnitOfWork>(() => new P6EntitiesDesignTimeUnitOfWork());

            if(p6EntitiesUnitOfWork != null)
                return new DbUnitOfWorkFactory<IP6EntitiesUnitOfWork>(() => p6EntitiesUnitOfWork);
            else
                return new DbUnitOfWorkFactory<IP6EntitiesUnitOfWork>(() => new P6EntitiesUnitOfWork(() => new P6Entities()));
        }
    }
}