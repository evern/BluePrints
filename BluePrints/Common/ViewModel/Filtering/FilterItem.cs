using DevExpress.Data.Filtering;
using DevExpress.Mvvm.POCO;

namespace BluePrints.Common.ViewModel.Filtering
{
    public class FilterItem
    {
        public static FilterItem Create(int entitiesCount, string name, CriteriaOperator filterCriteria, string imageUri,
            bool showEntityCount)
        {
            return
                ViewModelSource.Create(
                    () => new FilterItem(entitiesCount, name, filterCriteria, imageUri, showEntityCount));
        }

        protected FilterItem(int entitiesCount, string name, CriteriaOperator filterCriteria, string imageUri,
            bool showEntityCount)
        {
            Name = name;
            FilterCriteria = filterCriteria;
            ImageUri = imageUri;
            ShowEntityCount = showEntityCount;
            Update(entitiesCount);
        }

        private bool ShowEntityCount { get; set; }

        public virtual string Name { get; set; }

        public virtual CriteriaOperator FilterCriteria { get; set; }

        public virtual int EntitiesCount { get; protected set; }

        public virtual string DisplayText { get; protected set; }

        public virtual string ImageUri { get; protected set; }

        public virtual bool IsSelected { get; set; }

        public void Update(int entitiesCount)
        {
            EntitiesCount = entitiesCount;
            if (ShowEntityCount)
                DisplayText = string.Format("{0} ({1})", Name, entitiesCount);
            else
                DisplayText = Name;
        }

        public FilterItem Clone()
        {
            return Create(EntitiesCount, Name, FilterCriteria, ImageUri, ShowEntityCount);
        }

        public FilterItem Clone(string name, string imageUri)
        {
            return Create(EntitiesCount, name, FilterCriteria, imageUri, ShowEntityCount);
        }

        protected virtual void OnNameChanged()
        {
            Update(EntitiesCount);
        }
    }
}