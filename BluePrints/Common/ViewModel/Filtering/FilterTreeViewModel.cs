using BluePrints.Data.Helpers;
using DevExpress.Data.Filtering;
using DevExpress.Data.Utils;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace BluePrints.Common.ViewModel.Filtering
{
    public class FilterTreeViewModel<TEntity, TFilterEntity, TPrimaryKey> : IFilterTreeViewModel
        where TEntity : class
        where TFilterEntity : class
    {
        static FilterTreeViewModel()
        {
            var enums = typeof(ProjectType).Assembly.GetTypes().Where(t => t.IsEnum);
            foreach (var e in enums)
                EnumProcessingHelper.RegisterEnum(e);
        }

        public static FilterTreeViewModel<TEntity, TFilterEntity, TPrimaryKey> Create(
            IFilterTreeModelPageSpecificSettings<TFilterEntity> settings, IQueryable<TEntity> entities,
            Action<object, Action> registerEntityChangedMessageHandler, bool createContextFilter = false)
        {
            return
                ViewModelSource.Create(
                    () =>
                        new FilterTreeViewModel<TEntity, TFilterEntity, TPrimaryKey>(settings, entities,
                            registerEntityChangedMessageHandler, createContextFilter));
        }

        private readonly IQueryable<TEntity> entities;
        private readonly IFilterTreeModelPageSpecificSettings<TFilterEntity> settings;
        private object viewModel;

        protected FilterTreeViewModel(IFilterTreeModelPageSpecificSettings<TFilterEntity> settings,
            IQueryable<TEntity> entities, Action<object, Action> registerEntityChangedMessageHandler,
            bool createContextFilter = false)
        {
            this.settings = settings;
            this.entities = entities;
            StaticFilters = CreateFilterItems(settings.StaticFilters, false);
            CustomFilters = CreateFilterItems(settings.CustomFilters, true);
            if (createContextFilter)
                ContextFilters = CreateContextFilterItems();
            SelectedItem = StaticFilters.FirstOrDefault();
            if (SelectedItem != null)
                SelectedItem.IsSelected = true;
            registerEntityChangedMessageHandler(this, () => UpdateFilters());
            Messenger.Default.Register<CreateCustomFilterMessage<TEntity>>(this, message => CreateCustomFilter());
            UpdateFilters();
            StaticCategoryName = settings.StaticFiltersTitle;
            ContextCategoryName = typeof(TFilterEntity).Name;
            CustomCategoryName = "CUSTOM FILTERS";
                //Instead of putting it in xaml, use binding to ensure that code behind indeed exists to show anything in view
        }

        public virtual ObservableCollection<FilterItem> StaticFilters { get; protected set; }
        public virtual ObservableCollection<FilterItem> CustomFilters { get; protected set; }
        public virtual ObservableCollection<FilterItem> ContextFilters { get; protected set; }

        /// <summary>
        /// Private member to avoid SelectedItem getting overridded when module changes because of TreeView's TwoWay binding
        /// </summary>
        private FilterItem lastSelectedItem { get; set; }

        public virtual FilterItem SelectedItem { get; set; }

        public virtual FilterItem ActiveFilterItem { get; set; }
        public Action NavigateAction { get; set; }
        public string StaticCategoryName { get; private set; }
        public string ContextCategoryName { get; private set; }
        public string CustomCategoryName { get; private set; }

        public bool AllowStaticFilters
        {
            get { return StaticFilters != null && StaticFilters.Count > 0; }
        }

        public bool AllowCustomFilters
        {
            get { return settings.CustomFilters != null; }
        }

        public bool AllowContextFilters
        {
            get { return ContextFilters != null; }
        }

        public void DeleteCustomFilter(FilterItem filterItem)
        {
            CustomFilters.Remove(filterItem);
            SaveCustomFilters();
        }

        public void DuplicateFilter(FilterItem filterItem)
        {
            var newItem = filterItem.Clone("Copy of " + filterItem.Name, null);
            CustomFilters.Add(newItem);
            SaveCustomFilters();
        }

        public void ResetCustomFilters()
        {
            if (CustomFilters.Contains(SelectedItem))
                SelectedItem = null;
            settings.CustomFilters = new FilterInfoList();
            CustomFilters.Clear();
            settings.SaveSettings();
        }

        public void ModifyCustomFilter(FilterItem existing)
        {
            var clone = existing.Clone();
            var filterViewModel = CreateCustomFilterViewModel(clone, true);
            ShowFilter(clone, filterViewModel, () =>
            {
                existing.FilterCriteria = clone.FilterCriteria;
                existing.Name = clone.Name;
                SaveCustomFilters();
                if (existing == SelectedItem)
                    OnSelectedItemChanged();
            });
        }

        public void ResetToAll()
        {
            SelectedItem = StaticFilters[0];
        }

        public void CreateCustomFilter()
        {
            var filterItem = CreateFilterItem(string.Empty, null, null, true);
            var filterViewModel = CreateCustomFilterViewModel(filterItem, true);
            ShowFilter(filterItem, filterViewModel, () => AddNewCustomFilter(filterItem));
        }

        protected virtual void OnSelectedItemChanged()
        {
            if (SelectedItem == null)
                SelectedItem = lastSelectedItem;

            lastSelectedItem = SelectedItem.Clone();
            ActiveFilterItem = lastSelectedItem;
            UpdateFilterExpression();
            if (NavigateAction != null)
                NavigateAction();
        }

        void IFilterTreeViewModel.SetViewModel(object viewModel, object selectedFilterName)
        {
            this.viewModel = viewModel;
            var viewModelContainer = viewModel as IFilterTreeViewModelContainer<TEntity, TFilterEntity, TPrimaryKey>;
            if (viewModelContainer != null)
                viewModelContainer.FilterTreeViewModel = this;
            if (selectedFilterName != null)
            {
                var contextFilterItem =
                    ContextFilters.FirstOrDefault(x => x.Name == selectedFilterName.ToString());
                if (contextFilterItem != null)
                {
                    SelectedItem = contextFilterItem;
                    SelectedItem.IsSelected = true;
                }
            }

            UpdateFilterExpression();
        }

        private void UpdateFilterExpression()
        {
            var viewModel = this.viewModel as ISupportFiltering<TEntity>;
            if (viewModel != null)
                viewModel.FilterExpression = ActiveFilterItem == null
                    ? null
                    : GetWhereExpression(ActiveFilterItem.FilterCriteria);
        }

        private ObservableCollection<FilterItem> CreateFilterItems(IEnumerable<FilterInfo> filters, bool showEntityCount)
        {
            if (filters == null)
                return new ObservableCollection<FilterItem>();
            return
                new ObservableCollection<FilterItem>(
                    filters.Select(
                        x =>
                            CreateFilterItem(x.Name, CriteriaOperator.Parse(x.FilterCriteria), x.ImageUri,
                                showEntityCount)));
        }

        private ObservableCollection<FilterItem> CreateContextFilterItems()
        {
            var filterValuePropertyInfo = DataUtils.GetFilterValuePropertyInfo(typeof(TEntity));
            var filterNamePropertyInfo = DataUtils.GetFilterNamePropertyInfo(typeof(TEntity));

            if (filterValuePropertyInfo == null || filterNamePropertyInfo == null)
                return null;

            var baseFilterNamePropertyInfo = filterNamePropertyInfo.PropertyType == typeof(string)
                ? null
                : DataUtils.GetFilterNamePropertyInfo(filterNamePropertyInfo.PropertyType);
            var baseFilterKeyPropertyInfo = baseFilterNamePropertyInfo == null
                ? null
                : DataUtils.GetKeyPropertyInfo(typeof(TFilterEntity));

            if (filterNamePropertyInfo != null)
            {
                var filterInfos = new FilterInfoList();

                foreach (var filterEntity in settings.FilterEntities)
                {
                    string filterValue;
                    if (baseFilterNamePropertyInfo != null && baseFilterNamePropertyInfo.PropertyType == typeof(string) &&
                        baseFilterKeyPropertyInfo != null)
                    {
                        var currentBaseFilterNameValue = baseFilterNamePropertyInfo.GetValue(filterEntity);
                        filterValue = baseFilterKeyPropertyInfo.GetValue(filterEntity).ToString();

                        var filterCriteria = "[" + filterValuePropertyInfo.Name + "] = '" + filterValue + "'";
                        filterInfos.Add(new FilterInfo()
                        {
                            Name = (string) currentBaseFilterNameValue,
                            FilterCriteria = filterCriteria,
                            ImageUri = null
                        });
                    }
                    else if (filterNamePropertyInfo.PropertyType == typeof(string))
                    {
                        filterValue = filterValuePropertyInfo.GetValue(filterEntity).ToString();
                        var filterCriteria = "[" + filterValuePropertyInfo.Name + "] = '" + filterValue + "'";
                        filterInfos.Add(new FilterInfo()
                        {
                            Name = (string) filterNamePropertyInfo.GetValue(filterEntity),
                            FilterCriteria = filterCriteria,
                            ImageUri = null
                        });
                    }
                    else
                    {
                        return null;
                    }
                }

                return CreateFilterItems(filterInfos, true);
            }
            else
            {
                return null;
            }
        }


        private const string NewFilterName = @"New Filter";

        private void AddNewCustomFilter(FilterItem filterItem)
        {
            if (string.IsNullOrEmpty(filterItem.Name))
            {
                var prevIndex =
                    CustomFilters.Select(fi => Regex.Match(fi.Name, NewFilterName + @" (?<index>\d+)"))
                        .Where(m => m.Success)
                        .Select(m => int.Parse(m.Groups["index"].Value))
                        .DefaultIfEmpty(0)
                        .Max();
                filterItem.Name = NewFilterName + " " + (prevIndex + 1);
            }
            else
            {
                var existing = CustomFilters.FirstOrDefault(fi => fi.Name == filterItem.Name);
                if (existing != null)
                    CustomFilters.Remove(existing);
            }
            CustomFilters.Add(filterItem);
            SaveCustomFilters();
        }

        private void SaveCustomFilters()
        {
            settings.CustomFilters = SaveToSettings(CustomFilters);
            settings.SaveSettings();
        }

        private FilterInfoList SaveToSettings(ObservableCollection<FilterItem> filters)
        {
            return
                new FilterInfoList(
                    filters.Select(
                        fi =>
                            new FilterInfo
                            {
                                Name = fi.Name,
                                FilterCriteria = CriteriaOperator.ToString(fi.FilterCriteria)
                            }));
        }

        private void UpdateFilters()
        {
            IEnumerable<FilterItem> updateFilters = CustomFilters;
            if (ContextFilters != null)
                updateFilters.Concat(ContextFilters);

            foreach (var item in updateFilters)
                item.Update(GetEntityCount(item.FilterCriteria));
        }

        private void ShowFilter(FilterItem filterItem, CustomFilterViewModel filterViewModel, Action onSave)
        {
            if (
                FilterDialogService.ShowDialog(MessageButton.OKCancel, "Create Custom Filter", "CustomFilterView",
                    filterViewModel) != MessageResult.OK)
                return;
            filterItem.FilterCriteria = filterViewModel.FilterCriteria;
            filterItem.Name = filterViewModel.FilterName;
            ActiveFilterItem = filterItem;
            if (filterViewModel.Save)
            {
                onSave();
                UpdateFilters();
            }
        }

        private CustomFilterViewModel CreateCustomFilterViewModel(FilterItem existing, bool save)
        {
            var viewModel = CustomFilterViewModel.Create(typeof(TEntity), settings.HiddenFilterProperties,
                settings.AdditionalFilterProperties);
            viewModel.FilterCriteria = existing.FilterCriteria;
            viewModel.FilterName = existing.Name;
            viewModel.Save = save;
            viewModel.SetParentViewModel(this);
            return viewModel;
        }

        private FilterItem CreateFilterItem(string name, CriteriaOperator filterCriteria, string imageUri, bool showEntityCount)
        {
            return FilterItem.Create(GetEntityCount(filterCriteria), name, filterCriteria, imageUri, showEntityCount);
        }

        private int GetEntityCount(CriteriaOperator criteria)
        {
            return entities.Where(GetWhereExpression(criteria)).Count();
        }

        private Expression<Func<TEntity, bool>> GetWhereExpression(CriteriaOperator criteria)
        {
            return this.IsInDesignMode()
                ? CriteriaOperatorToExpressionConverter.GetLinqToObjectsWhere<TEntity>(criteria)
                : CriteriaOperatorToExpressionConverter.GetGenericWhere<TEntity>(criteria);
        }

        private IDialogService FilterDialogService
        {
            get { return this.GetRequiredService<IDialogService>("FilterDialogService"); }
        }
    }

    public interface IFilterTreeViewModelContainer<TEntity, TFilterEntity, TPrimaryKey>
        where TEntity : class
        where TFilterEntity : class
    {
        FilterTreeViewModel<TEntity, TFilterEntity, TPrimaryKey> FilterTreeViewModel { get; set; }
    }

    public class CreateCustomFilterMessage<TEntity> where TEntity : class
    {
    }

    public interface IFilterTreeViewModel
    {
        void SetViewModel(object content, object selectedFilterName);
        Action NavigateAction { get; set; }
    }
}