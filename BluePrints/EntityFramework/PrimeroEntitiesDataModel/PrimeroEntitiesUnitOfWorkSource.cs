using BaseModel.DataModel;
using BaseModel.DataModel.DesignTime;
using BaseModel.DataModel.EntityFramework;
using BluePrints.Common;
using BluePrints.Common.Resources;
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
        public static IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> GetUnitOfWorkFactory(string officeName = "")
        {
            return GetUnitOfWorkFactory(ViewModelBase.IsInDesignMode, getDatabaseLocaleFromOfficeName(officeName));
        }

        private static DatabaseLocale getDatabaseLocaleFromOfficeName(string officeName)
        {
            string officeNameUpper = officeName.ToUpper();
            if (officeNameUpper == BluePrintsResources.OfficePerth)
                return DatabaseLocale.Perth;
            else if (officeNameUpper == BluePrintsResources.OfficeMontreal)
                return DatabaseLocale.Montreal;
            else if (officeNameUpper == BluePrintsResources.OfficeUSA)
                return DatabaseLocale.USA;
            else
                return DatabaseLocale.Default;
        }

        /// <summary>
        /// Returns the IUnitOfWorkFactory implementation based on the given mode (run-time or design-time).
        /// </summary>
        /// <param name="isInDesignTime">Used to determine which implementation of IUnitOfWorkFactory should be returned.</param>
        public static IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> GetUnitOfWorkFactory(bool isInDesignTime, DatabaseLocale dbLocale)
        {
            if (dbLocale == DatabaseLocale.Perth)
                return
                    new DbUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork>(
                        () => new PrimeroEntitiesUnitOfWork(() => new PrimeroEntities()));
            else if (dbLocale == DatabaseLocale.Montreal)
                return
                    new DbUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork>(
                        () => new PrimeroEntitiesUnitOfWork(() => new PGAEntities()));
            else if (dbLocale == DatabaseLocale.USA)
                return
                    new DbUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork>(
                        () => new PrimeroEntitiesUnitOfWork(() => new PUSAEntities()));
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