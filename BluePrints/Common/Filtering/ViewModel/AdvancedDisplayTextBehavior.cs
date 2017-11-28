using DevExpress.Mvvm.UI.Interactivity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace BluePrints.Common.Filtering
{
    public class AdvancedDisplayTextBehavior : Behavior<AdvancedNavBarGroup>
    {
        public static readonly DependencyProperty FiltersProperty =
            DependencyProperty.Register("Filters", typeof(ObservableCollection<FilterItem>), typeof(AdvancedDisplayTextBehavior),
            new PropertyMetadata(null, (d, e) => ((AdvancedDisplayTextBehavior)d).OnFiltersChanged(e)));
        public ObservableCollection<FilterItem> Filters
        {
            get { return (ObservableCollection<FilterItem>)GetValue(FiltersProperty); }
            set { SetValue(FiltersProperty, value); }
        }
        public static readonly DependencyProperty DisplayPropertyProperty =
           DependencyProperty.Register("DisplayProperty", typeof(string), typeof(AdvancedDisplayTextBehavior), new PropertyMetadata("Overdue Tasks"));
        public string DisplayProperty
        {
            get { return (string)GetValue(DisplayPropertyProperty); }
            set { SetValue(DisplayPropertyProperty, value); }
        }
        void OnFiltersChanged(DependencyPropertyChangedEventArgs e)
        {
            if (AssociatedObject == null)
                return;
            BindingOperations.ClearBinding(this.AssociatedObject, AdvancedNavBarGroup.AdvancedDisplayTextProperty);
            FilterItem item = Filters.FirstOrDefault(x => x.Name == DisplayProperty);
            if (item != null)
                BindingOperations.SetBinding(this.AssociatedObject, AdvancedNavBarGroup.AdvancedDisplayTextProperty, new Binding("EntitiesCount") { Source = item, Mode = BindingMode.OneWay });
        }
        protected override void OnAttached()
        {
            base.OnAttached();
        }
        protected override void OnDetaching()
        {
            BindingOperations.ClearBinding(this.AssociatedObject, AdvancedNavBarGroup.AdvancedDisplayTextProperty);
            base.OnDetaching();
        }
    }
}
