using BaseModel.Misc;
using BluePrints.Common.Projections;
using BluePrints.Data;
using System;
using System.Linq;
using System.Collections.Generic;
using BluePrints.Common.ViewModel.Reporting;
using DevExpress.Mvvm;
using System.ComponentModel.DataAnnotations.Schema;

namespace BluePrints.Common.Base
{
    /// <summary>
    /// Provides mapping back to key naming convention for projection
    /// Projection key property name must be the same with entity key name for repository results to map back to projections
    /// </summary>
    /// <typeparam name="TEntity">Entity with Guid typed key</typeparam>
    public abstract class BluePrintsEntityBase : BindableBase, ICanUpdate
    {
        [NotMapped]
        public bool NewEntityFromView { get; set; }

        public void Update()
        {
            RaisePropertiesChanged();
        }
    }
}
