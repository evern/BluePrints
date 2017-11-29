using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using DevExpress.Data.Filtering;
using DevExpress.Data.Utils;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.ViewModel.Reporting;

namespace BluePrints.Common.Filtering
{
    public class FilterTreeViewModel<TEntity, TPrimaryKey>
        where TEntity : class, new()
    {
        static FilterTreeViewModel() {
            //var enums = typeof(EmployeeStatus).Assembly.GetTypes().Where(t => t.IsEnum);
            //foreach(Type e in enums)
            //    EnumProcessingHelper.RegisterEnum(e);
        }

        public static FilterTreeViewModel<TEntity, TPrimaryKey> Create(IFilterTreeModelPageSpecificSettings settings, ISupportFiltering<TEntity> viewModel, IEnumerable<TEntity> entities) {
            return ViewModelSource.Create(() => new FilterTreeViewModel<TEntity, TPrimaryKey>(settings, viewModel, entities));
        }

        readonly IQueryable<TEntity> entities;
        readonly IFilterTreeModelPageSpecificSettings settings;
        ISupportFiltering<TEntity> viewModel;

        protected FilterTreeViewModel(IFilterTreeModelPageSpecificSettings settings, ISupportFiltering<TEntity> viewModel, IEnumerable<TEntity> entities) {
            this.settings = settings;
            this.viewModel = viewModel;
            this.entities = entities.AsQueryable();
            StaticFilters = CreateFilterItems(settings.StaticFilters);
            CustomFilters = CreateFilterItems(settings.CustomFilters);
            SelectedItem = StaticFilters.FirstOrDefault();
            AllowFiltersContextMenu = true;

            Messenger.Default.Register<EntityMessage<TEntity, TPrimaryKey>>(this, message => UpdateFilters());
            Messenger.Default.Register<CreateCustomFilterMessage<TEntity>>(this, message => CreateCustomFilter());
            UpdateFilters();
            StaticCategoryName = settings.StaticFiltersTitle;
            CustomCategoryName = settings.CustomFiltersTitle;
        }

        public virtual ObservableCollection<FilterItem> StaticFilters { get; protected set; }
        public virtual ObservableCollection<FilterItem> CustomFilters { get; protected set; }
        public virtual FilterItem SelectedItem { get; set; }
        public virtual FilterItem ActiveFilterItem { get; set; }
        public Action NavigateAction { get; set; }
        public string StaticCategoryName { get; private set; }
        public string CustomCategoryName { get; private set; }
        public bool AllowCustomFilters { get { return settings.CustomFilters != null; } }
        public bool IsExpanded { get; set; }

        public bool AllowFiltersContextMenu {
            get {
                return AllowCustomFilters && allowFiltersContextMenu;
            }
            set {
                allowFiltersContextMenu = value;
            }
        }
        bool allowFiltersContextMenu;

        public void DeleteCustomFilter(FilterItem filterItem) {
            CustomFilters.Remove(filterItem);
            SaveCustomFilters();
        }

        public void DuplicateFilter(FilterItem filterItem) {
            var newItem = filterItem.Clone("Copy of " + filterItem.Name, null);
            CustomFilters.Add(newItem);
            SaveCustomFilters();
        }

        public void ResetCustomFilters() {
            if(CustomFilters.Contains(SelectedItem))
                SelectedItem = null;
            settings.CustomFilters = new FilterInfoList();
            CustomFilters.Clear();
            settings.SaveSettings();
        }

        public void ModifyCustomFilter(FilterItem existing) {
            FilterItem clone = existing.Clone();
            var filterViewModel = CreateCustomFilterViewModel(clone, true);
            ShowFilter(clone, filterViewModel, () =>
            {
                existing.FilterCriteria = clone.FilterCriteria;
                existing.Name = clone.Name;
                SaveCustomFilters();
                if(existing == SelectedItem)
                    OnSelectedItemChanged();
            });
        }
        public void ModifyCustomFilterCriteria(FilterItem existing, CriteriaOperator criteria) {
            if(!ReferenceEquals(existing.FilterCriteria, null) && !ReferenceEquals(criteria, null) && existing.FilterCriteria.ToString() == criteria.ToString())
                return;
            existing.FilterCriteria = criteria;
            SaveCustomFilters();
            if(existing == SelectedItem)
                OnSelectedItemChanged();
            UpdateFilters();
        }
        public void ResetToAll() {
            SelectedItem = StaticFilters[0];
        }

        public void CreateCustomFilter() {
            FilterItem filterItem = CreateFilterItem(string.Empty, null, null);
            var filterViewModel = CreateCustomFilterViewModel(filterItem, true);
            ShowFilter(filterItem, filterViewModel, () => AddNewCustomFilter(filterItem));
        }
        public void AddCustomFilter(string name, CriteriaOperator filterCriteria, string imageUri = null) {
            AddNewCustomFilter(CreateFilterItem(name, filterCriteria, imageUri));
        }

        public void ClearFilters()
        {
            SelectedItem = null;
            viewModel.GridControlService.ClearFilterCriteria();
        }

        protected virtual void OnSelectedItemChanged() {
            if (SelectedItem != null)
                ActiveFilterItem = SelectedItem.Clone();
            else
                ActiveFilterItem = null;

            UpdateFilterExpression();
            NavigateCore();
        }
        public virtual void Navigate() {
            NavigateCore();
        }
        void NavigateCore() {
            if(NavigateAction != null)
                NavigateAction();
        }

        void UpdateFilterExpression() {
            viewModel.GridControlService.SetFilterCriteria(ActiveFilterItem == null ? null : ActiveFilterItem.FilterCriteria);
        }

        ObservableCollection<FilterItem> CreateFilterItems(IEnumerable<FilterInfo> filters) {
            if(filters == null)
                return new ObservableCollection<FilterItem>();
            return new ObservableCollection<FilterItem>(filters.Select(x => CreateFilterItem(x.Name, CriteriaOperator.Parse(x.FilterCriteria), x.ImageUri)));
        }

        const string NewFilterName = @"New Filter";

        void AddNewCustomFilter(FilterItem filterItem) {
            if(string.IsNullOrEmpty(filterItem.Name)) {
                int prevIndex = CustomFilters.Select(fi => Regex.Match(fi.Name, NewFilterName + @" (?<index>\d+)")).Where(m => m.Success).Select(m => int.Parse(m.Groups["index"].Value)).DefaultIfEmpty(0).Max();
                filterItem.Name = NewFilterName + " " + (prevIndex + 1);
            } else {
                var existing = CustomFilters.FirstOrDefault(fi => fi.Name == filterItem.Name);
                if(existing != null)
                    CustomFilters.Remove(existing);
            }
            CustomFilters.Add(filterItem);
            SaveCustomFilters();
        }

        void SaveCustomFilters() {
            settings.CustomFilters = SaveToSettings(CustomFilters);
            settings.SaveSettings();
        }

        FilterInfoList SaveToSettings(ObservableCollection<FilterItem> filters) {
            return new FilterInfoList(filters.Select(fi => new FilterInfo { Name = fi.Name, FilterCriteria = CriteriaOperator.ToString(fi.FilterCriteria) }));
        }

        void UpdateFilters() {
            foreach(var item in StaticFilters.Concat(CustomFilters)) {
                item.Update(GetEntityCount(item.FilterCriteria));
            }
        }

        void ShowFilter(FilterItem filterItem, CustomFilterViewModel filterViewModel, Action onSave) {
            if(FilterDialogService.ShowDialog(MessageButton.OKCancel, "Create Custom Filter", "CustomFilterView", filterViewModel) != MessageResult.OK)
                return;
            filterItem.FilterCriteria = viewModel.GridControlService.GetFilterCriteria();
            filterItem.Name = filterViewModel.FilterName;
            ActiveFilterItem = filterItem;
            onSave();
            UpdateFilters();
        }

        CustomFilterViewModel CreateCustomFilterViewModel(FilterItem existing, bool save) {
            var viewModel = CustomFilterViewModel.Create(typeof(TEntity), settings.HiddenFilterProperties, settings.AdditionalFilterProperties);
            viewModel.FilterName = existing.Name;
            viewModel.SetParentViewModel(this);
            return viewModel;
        }

        FilterItem CreateFilterItem(string name, CriteriaOperator filterCriteria, string imageUri) {
            return FilterItem.Create(GetEntityCount(filterCriteria), name, filterCriteria, imageUri);
        }

        int GetEntityCount(CriteriaOperator criteria) {
            Expression<Func<TEntity, bool>> whereExpression = GetWhereExpression(criteria);
            if (whereExpression != null)
                return entities.Where(whereExpression).Count();
            else
                return 0;
        }

        Expression<Func<TEntity, bool>> GetWhereExpression(CriteriaOperator criteria) {
            try
            {
                var caseInsensitiveCriteria = DevExpress.Data.Helpers.StringsTolowerCloningHelper.Process(criteria);
                var nullSafeCriteria = nullOrEmptyConversion(caseInsensitiveCriteria);
                return this.IsInDesignMode()
                    ? CriteriaOperatorToExpressionConverter.GetLinqToObjectsWhere<TEntity>(nullSafeCriteria)
                    : CriteriaOperatorToExpressionConverter.GetGenericWhere<TEntity>(nullSafeCriteria);
            }
            catch
            {
                return null;
            }
        }

        CriteriaOperator nullOrEmptyConversion(CriteriaOperator criteria)
        {
            string currentCriteria = criteria.ToString();
            if (!currentCriteria.ToUpper().Contains("NULL"))
                return criteria;
            else
            {
                List<string> operatorArr = currentCriteria.Split(new string[] { " And " }, StringSplitOptions.None).ToList();
                string newOperator = string.Empty;
                foreach (string op in operatorArr)
                {
                    if(op.ToUpper().Contains("NULL"))
                    {
                        string propertyName = findPropertyName(op);
                        if(propertyName != string.Empty)
                        {
                            if (op.ToUpper().Contains("NOT"))
                                newOperator += "[" + propertyName + "] Is Not Null And ";
                            else
                                newOperator += "[" + propertyName + "] Is Null And ";
                        }
                    }
                }

                if (newOperator.Contains(" And "))
                    return CriteriaOperator.Parse(newOperator.Substring(0, newOperator.Length - 5));
                else
                    return string.Empty;
            }
        }

        string findPropertyName(string operatorString)
        {
            string mystr = operatorString;

            List<string> myvariables = new List<string>();
            while (mystr.Contains("["))
            {
                return mystr.Split('[', ']')[1];
            };

            return string.Empty;
        }

        IDialogService FilterDialogService { get { return this.GetRequiredService<IDialogService>("FilterDialogService"); } }
    }

    public class CreateCustomFilterMessage<TEntity> where TEntity : class {
    }
}
