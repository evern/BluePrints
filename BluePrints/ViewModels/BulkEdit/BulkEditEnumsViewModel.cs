using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using BluePrints.Common;

namespace BluePrints.ViewModels
{
    public class BulkEditEnumsViewModel
    {
        public static BulkEditEnumsViewModel Create(IEnumerable<object> enumerableObjects, string comboBoxDisplayMember)
        {
            return ViewModelSource.Create(() => new BulkEditEnumsViewModel(enumerableObjects, comboBoxDisplayMember));
        }

        public IEnumerable<object> SourceObjects { get; set; }
        public string ComboBoxDisplayMember { get; set; }
        protected BulkEditEnumsViewModel(IEnumerable<object> enumerableObjects, string comboBoxDisplayMember)
        {
            SourceObjects = enumerableObjects;
            ComboBoxDisplayMember = comboBoxDisplayMember;
        }

        object selectedItem { get; set; }
        public object SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (value != null)
                {
                    selectedItem = value;
                    this.RaisePropertiesChanged();
                }
            }
        }
    }
}
