using System;
using DevExpress.Mvvm.POCO;

namespace BaseModel.ViewModel.Dialogs
{
    public class YearSelectViewModel
    {
        public static YearSelectViewModel Create()
        {
            return ViewModelSource.Create(() => new YearSelectViewModel());
        }

        public DateTime YearSelect { get; set; }
        protected YearSelectViewModel()
        {
            YearSelect = DateTime.Now;
        }
    }
}