using BluePrints.Common.ViewModel.Reporting;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel
{
    public interface ICollectionViewModel<TProjection>
        where TProjection : class
    {
        void Save(TProjection entity);
        void Delete(TProjection entity);
        void CleanUpCallBacks();
    }

    public interface IHaveGUID
    {
        Guid GUID { get; set; }
    }

    public interface IHaveSummary : IHaveStats
    {
        void BuildStats();
        void RecalculateStats(bool isCosts);
    }

    public interface IHaveStats
    {
        ProgressStats Stats { get; set; }
    }

    public interface ISupportViewRestoration
    {
        Action StoreActiveCell { get; set; }
        Action RestoreActiveCell { get; set; }

        //Raise Properties changed doesn't refresh column data, call this method instead
        Action ForceGridRefresh { get; set; }
    }

    /// <summary>
    /// The interface for supporting children document other than using TEntity type name.
    /// </summary>
    public interface ISupportCustomDocumentTypeNameAndParameter
    {
        string GetCustomDocumentTypeName();
        object GetCustomDocumentParameter();
        string GetCustomDocumentTitle();
        bool IsCustomModeEnabled();
    }

    /// <summary>
    /// The base interface for view models representing a single entity.
    /// </summary>
    /// <typeparam name="TEntity">An entity type.</typeparam>
    /// <typeparam name="TPrimaryKey">An entity primary key type.</typeparam>
    public interface ISingleObjectViewModel<TEntity, TPrimaryKey>
    {
        /// <summary>
        /// The entity represented by a view model.
        /// </summary>
        TEntity Entity { get; }

        /// <summary>
        /// The entity primary key value.
        /// </summary>
        TPrimaryKey PrimaryKey { get; }
    }
}
