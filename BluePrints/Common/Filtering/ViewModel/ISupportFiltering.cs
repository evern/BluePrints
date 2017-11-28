using BaseModel.ViewModel.Services;
using DevExpress.Data.Filtering;
using System;
using System.Linq.Expressions;

namespace BluePrints.Common.Filtering
{
    public interface ISupportFiltering<TEntity> where TEntity : class {
        IGridControlService GridControlService { get; }
    }
}
