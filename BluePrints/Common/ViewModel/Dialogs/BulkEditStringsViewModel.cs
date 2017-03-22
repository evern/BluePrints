using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.ViewModels
{
    public class BulkEditStringsViewModel
    {
        public static BulkEditStringsViewModel Create(string editString)
        {
            return ViewModelSource.Create(() => new BulkEditStringsViewModel(editString));
        }

        public string EditValue { get; set; }

        protected BulkEditStringsViewModel(string editString)
        {
            EditValue = editString;
        }
    }
}