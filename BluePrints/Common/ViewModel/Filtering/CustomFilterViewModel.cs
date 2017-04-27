using DevExpress.Data.Filtering;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;

namespace BluePrints.Common.ViewModel.Filtering
{
    public class CustomFilterViewModel
    {
        public static CustomFilterViewModel Create(Type entityType, IEnumerable<string> hiddenProperties,
            IEnumerable<string> additionalProperties)
        {
            return
                ViewModelSource.Create(
                    () => new CustomFilterViewModel(entityType, hiddenProperties, additionalProperties));
        }

        protected CustomFilterViewModel(Type entityType, IEnumerable<string> hiddenProperties,
            IEnumerable<string> additionalProperties)
        {
            EntityType = entityType;
            HiddenProperties = hiddenProperties;
            AdditionalProperties = additionalProperties;
        }

        public Type EntityType { get; private set; }
        public IEnumerable<string> HiddenProperties { get; private set; }
        public IEnumerable<string> AdditionalProperties { get; private set; }
        public virtual bool Save { get; set; }
        public virtual CriteriaOperator FilterCriteria { get; set; }
        public virtual string FilterName { get; set; }
    }
}