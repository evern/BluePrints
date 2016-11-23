using BluePrints.Common;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.ViewModels
{
    public class BulkEditNumbersViewModel
    {
        public static BulkEditNumbersViewModel Create(decimal editNumber)
        {
            return ViewModelSource.Create(() => new BulkEditNumbersViewModel(editNumber));
        }

        public decimal EditValue { get; set; }
        public EnumMemberInfo SelectedOperation { get; set; }
        protected BulkEditNumbersViewModel(decimal editValue)
        {
            EditValue = editValue;
        }
    }
}
