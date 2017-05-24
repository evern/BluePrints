using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BluePrints.Common
{
    public interface ITableViewService
    {
        void TakeSnapshot();
    }

    public class TableViewService : ServiceBase, ITableViewService
    {
        public TableView TableView
        {
            get { return (TableView)GetValue(CameraProperty); }
            set { SetValue(CameraProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Camera.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CameraProperty =
            DependencyProperty.Register("Camera", typeof(TableView), typeof(TableViewService), new PropertyMetadata(null));

        public void TakeSnapshot()
        {
            if (this.TableView != null)
            {
                
            }
            else
            {
                
            }
        }
    }
}
