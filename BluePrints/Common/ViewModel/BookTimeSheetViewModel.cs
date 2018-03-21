using BluePrints.Common.ViewModel.Reporting;
using DevExpress.Mvvm.POCO;
using System.Collections.Generic;

namespace BaseModel.ViewModel.Dialogs
{
    public class BookTimeSheetViewModel
    {
        public static BookTimeSheetViewModel Create(IDeliverable deliverable)
        {
            return ViewModelSource.Create(() => new BookTimeSheetViewModel(deliverable));
        }

        protected BookTimeSheetViewModel(IDeliverable deliverable)
        {

        }
    }
}