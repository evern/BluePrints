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
        public static IUnitOfWorkFactory<IP6EntitiesUnitOfWork> GetUnitOfWorkFactory()
        {
            return GetUnitOfWorkFactory(ViewModelBase.IsInDesignMode);
        }

        /// <summary>
        /// Returns the IUnitOfWorkFactory implementation based on the given mode (run-time or design-time).
        /// </summary>
        /// <param name="isInDesignTime">Used to determine which implementation of IUnitOfWorkFactory should be returned.</param>
        public static IUnitOfWorkFactory<IP6EntitiesUnitOfWork> GetUnitOfWorkFactory(bool isInDesignTime)
        {
            if (isInDesignTime)
                return new DesignTimeUnitOfWorkFactory<IP6EntitiesUnitOfWork>(() => new P6EntitiesDesignTimeUnitOfWork());
            return new DbUnitOfWorkFactory<IP6EntitiesUnitOfWork>(() => new P6EntitiesUnitOfWork(() => new P6Entities()));
        }
    }
}