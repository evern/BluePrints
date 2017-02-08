using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;

namespace BluePrints.Common.ViewModel.Filtering
{
    public class FilterTreeModelPageSpecificSettings<TSettings, TFilterEntities> :
        IFilterTreeModelPageSpecificSettings<TFilterEntities>
        where TSettings : ApplicationSettingsBase
        where TFilterEntities : class
    {
        private readonly string staticFiltersTitle;
        private readonly TSettings settings;
        private readonly PropertyDescriptor customFiltersProperty;
        private readonly PropertyDescriptor staticFiltersProperty;
        private readonly IEnumerable<string> hiddenFilterProperties;
        private readonly IEnumerable<string> additionalFilterProperties;
        private readonly IQueryable<TFilterEntities> filterEntities;

        public FilterTreeModelPageSpecificSettings(TSettings settings, IQueryable<TFilterEntities> filterEntities,
            string staticFiltersTitle,
            Expression<Func<TSettings, FilterInfoList>> getStaticFiltersExpression,
            Expression<Func<TSettings, FilterInfoList>> getCustomFiltersExpression,
            IEnumerable<string> hiddenFilterProperties = null, IEnumerable<string> additionalFilterProperties = null)
        {
            this.settings = settings;
            this.filterEntities = filterEntities;
            this.staticFiltersTitle = staticFiltersTitle;
            staticFiltersProperty = GetProperty(getStaticFiltersExpression);
            customFiltersProperty = GetProperty(getCustomFiltersExpression);
            this.hiddenFilterProperties = hiddenFilterProperties;
            this.additionalFilterProperties = additionalFilterProperties;
        }

        FilterInfoList IFilterTreeModelPageSpecificSettings<TFilterEntities>.CustomFilters
        {
            get { return GetFilters(customFiltersProperty); }
            set { SetFilters(customFiltersProperty, value); }
        }

        FilterInfoList IFilterTreeModelPageSpecificSettings<TFilterEntities>.StaticFilters
        {
            get { return GetFilters(staticFiltersProperty); }
            set { SetFilters(staticFiltersProperty, value); }
        }

        IQueryable<TFilterEntities> IFilterTreeModelPageSpecificSettings<TFilterEntities>.FilterEntities
        {
            get { return filterEntities; }
        }

        string IFilterTreeModelPageSpecificSettings<TFilterEntities>.StaticFiltersTitle
        {
            get { return staticFiltersTitle; }
        }

        IEnumerable<string> IFilterTreeModelPageSpecificSettings<TFilterEntities>.HiddenFilterProperties
        {
            get { return hiddenFilterProperties; }
        }

        IEnumerable<string> IFilterTreeModelPageSpecificSettings<TFilterEntities>.AdditionalFilterProperties
        {
            get { return additionalFilterProperties; }
        }

        void IFilterTreeModelPageSpecificSettings<TFilterEntities>.SaveSettings()
        {
            settings.Save();
        }

        private PropertyDescriptor GetProperty(Expression<Func<TSettings, FilterInfoList>> expression)
        {
            if (expression != null)
                return TypeDescriptor.GetProperties(settings)[GetPropertyName(expression)];
            return null;
        }

        private FilterInfoList GetFilters(PropertyDescriptor property)
        {
            return property != null ? (FilterInfoList) property.GetValue(settings) : null;
        }

        private void SetFilters(PropertyDescriptor property, FilterInfoList value)
        {
            if (property != null)
                property.SetValue(settings, value);
        }

        private static string GetPropertyName(Expression<Func<TSettings, FilterInfoList>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("expression");
            return memberExpression.Member.Name;
        }
    }
}