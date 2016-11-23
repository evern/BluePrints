using DevExpress.Data.Filtering;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel.Filtering
{
    public class FilterItem
    {
        public static FilterItem Create(int entitiesCount, string name, CriteriaOperator filterCriteria, string imageUri, bool showEntityCount)
        {
            return ViewModelSource.Create(() => new FilterItem(entitiesCount, name, filterCriteria, imageUri, showEntityCount));
        }

        protected FilterItem(int entitiesCount, string name, CriteriaOperator filterCriteria, string imageUri, bool showEntityCount)
        {
            this.Name = name;
            this.FilterCriteria = filterCriteria;
            this.ImageUri = imageUri;
            this.ShowEntityCount = showEntityCount;
            Update(entitiesCount);
        }

        bool ShowEntityCount { get; set; }

        public virtual string Name { get; set; }

        public virtual CriteriaOperator FilterCriteria { get; set; }

        public virtual int EntitiesCount { get; protected set; }

        public virtual string DisplayText { get; protected set; }

        public virtual string ImageUri { get; protected set; }

        public virtual bool IsSelected { get; set; }

        public void Update(int entitiesCount)
        {
            this.EntitiesCount = entitiesCount;
            if (ShowEntityCount)
                DisplayText = string.Format("{0} ({1})", Name, entitiesCount);
            else
                DisplayText = Name;
        }

        public FilterItem Clone()
        {
            return FilterItem.Create(EntitiesCount, Name, FilterCriteria, ImageUri, ShowEntityCount);
        }
        public FilterItem Clone(string name, string imageUri)
        {
            return FilterItem.Create(EntitiesCount, name, FilterCriteria, imageUri, ShowEntityCount);
        }

        protected virtual void OnNameChanged()
        {
            Update(EntitiesCount);
        }
    }
}
