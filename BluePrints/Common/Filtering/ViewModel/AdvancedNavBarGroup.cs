using DevExpress.Xpf.NavBar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BluePrints.Common.Filtering
{
    public class AdvancedNavBarGroup : NavBarGroup
    {
        public static readonly DependencyProperty AdvancedDisplayTextProperty =
            DependencyProperty.Register("AdvancedDisplayText", typeof(string), typeof(AdvancedNavBarGroup), new PropertyMetadata(null));
        public string AdvancedDisplayText
        {
            get { return (string)GetValue(AdvancedDisplayTextProperty); }
            set { SetValue(AdvancedDisplayTextProperty, value); }
        }
    }
}
