using System;
using System.Linq.Expressions;

namespace BluePrints.Common.ViewModel.Filtering
{
    public interface ISupportFiltering<TEntity> where TEntity : class
    {
        Expression<Func<TEntity, bool>> FilterExpression { get; set; }
    }
}