using System;
using System.Linq;
using System.Data;
using System.Linq.Expressions;
using System.Collections.Generic;
using BluePrints.Common.Utils;
using BluePrints.Common.DataModel;
using BluePrints.Common.DataModel.DesignTime;
using BluePrints.Common.DataModel.EntityFramework;
using BluePrints.PrimeroData;
using DevExpress.Mvvm;
using System.Collections;
using System.ComponentModel;
using DevExpress.Data.Linq;
using DevExpress.Data.Linq.Helpers;
using DevExpress.Data.Async.Helpers;

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
        public static IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> GetUnitOfWorkFactory()
        {
            return GetUnitOfWorkFactory(ViewModelBase.IsInDesignMode);
        }

        /// <summary>
        /// Returns the IUnitOfWorkFactory implementation based on the given mode (run-time or design-time).
        /// </summary>
        /// <param name="isInDesignTime">Used to determine which implementation of IUnitOfWorkFactory should be returned.</param>
        public static IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> GetUnitOfWorkFactory(bool isInDesignTime)
        {
            if (isInDesignTime)
                return
                    new DesignTimeUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork>(
                        () => new PrimeroEntitiesDesignTimeUnitOfWork());
            return
                new DbUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork>(
                    () => new PrimeroEntitiesUnitOfWork(() => new PrimeroEntities()));
        }
    }
}