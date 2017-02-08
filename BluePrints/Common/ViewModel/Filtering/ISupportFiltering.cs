using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel.Filtering
{
    public interface ISupportFiltering<TEntity> where TEntity : class
    {
        Expression<Func<TEntity, bool>> FilterExpression { get; set; }
    }
}