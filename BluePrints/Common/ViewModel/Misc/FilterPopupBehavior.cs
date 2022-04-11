
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors;

namespace BluePrints.Common.ViewModel.Misc
{
    public class FilterPopupBehavior : Behavior<PopupBaseEdit>
    {
        public static void TryAttach(PopupBaseEdit editor)
        {
            if (editor == null)
                return;
            var behaviors = Interaction.GetBehaviors(editor);
            if (!behaviors.OfType<FilterPopupBehavior>().Any())
                behaviors.Add(new FilterPopupBehavior());
        }
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PopupOpened += PopupOpened;
            AssociatedObject.PopupClosed += PopupClosed;
        }
        protected override void OnDetaching()
        {
            AssociatedObject.PopupOpened -= PopupOpened;
            AssociatedObject.PopupClosed -= PopupClosed;
            base.OnDetaching();
        }
        protected virtual void PopupOpened(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Opened");
        }
        protected virtual void PopupClosed(object sender, ClosePopupEventArgs e)
        {
            Debug.WriteLine("Closed");
        }
    }
}
