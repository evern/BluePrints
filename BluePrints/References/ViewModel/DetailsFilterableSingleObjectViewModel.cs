using BluePrints.Common.DataModel;
using BluePrints.Common.ViewModel.Filtering;
using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel
{
    public class DetailsFilterableSingleObjectViewModel<TEntity, TFilterEntity, TPrimaryKey, TUnitOfWork> :
        SingleObjectViewModel<TEntity, TPrimaryKey, TUnitOfWork>, ISupportFiltering<TFilterEntity>
        where TEntity : class
        where TFilterEntity : class
        where TUnitOfWork : IUnitOfWork
    {
        /// <summary>
        /// Initializes a new instance of the SingleObjectViewModel class.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create the unit of work instance.</param>
        /// <param name="getRepositoryFunc">A function that returns the repository representing entities of a given type.</param>
        /// <param name="getEntityDisplayNameFunc">An optional parameter that provides a function to obtain the display text for a given entity. If ommited, the primary key value is used as a display text.</param>
        protected DetailsFilterableSingleObjectViewModel(IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory,
            Func<TUnitOfWork, IRepository<TEntity, TPrimaryKey>> getRepositoryFunc,
            Func<TEntity, object> getEntityDisplayNameFunc = null)
            : base(unitOfWorkFactory, getRepositoryFunc, getEntityDisplayNameFunc)
        {
            FilterDetailsCollectionPropertyInfo =
                GetType()
                    .GetProperties()
                    .FirstOrDefault(
                        prop =>
                            prop.PropertyType == typeof(CollectionViewModel<TFilterEntity, TPrimaryKey, TUnitOfWork>));
        }

        #region ISupportFiltering

        public void CreateCustomFilter()
        {
            Messenger.Default.Send(new CreateCustomFilterMessage<TFilterEntity>());
        }

        Expression<Func<TFilterEntity, bool>> ISupportFiltering<TFilterEntity>.FilterExpression
        {
            get { return FilterExpression; }
            set { FilterExpression = value; }
        }

        /// <summary>
        /// The lambda expression used to filter which entities will be loaded locally from the unit of work.
        /// Since ReadOnlyCollectionViewModelBase is a POCO view model, this property will raise INotifyPropertyChanged.PropertyEvent when modified so it can be used as a binding source in views.
        /// </summary>
        public virtual Expression<Func<TFilterEntity, bool>> FilterExpression { get; set; }

        private PropertyInfo FilterDetailsCollectionPropertyInfo { get; set; }

        protected virtual void OnFilterExpressionChanged()
        {
            if (FilterDetailsCollectionPropertyInfo == null)
                return;

            var viewModel = FilterDetailsCollectionPropertyInfo.GetValue(this) as ISupportFiltering<TFilterEntity>;
            if (viewModel != null)
                viewModel.FilterExpression = FilterExpression;
        }

        #endregion
    }
}