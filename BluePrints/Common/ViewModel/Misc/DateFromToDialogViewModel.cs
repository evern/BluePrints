using System;
using DevExpress.Mvvm.POCO;

namespace BaseModel.ViewModel.Dialogs
{
    public class DateFromToDialogViewModel
    {
        public static DateFromToDialogViewModel Create()
        {
            return ViewModelSource.Create(() => new DateFromToDialogViewModel());
        }

        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        protected DateFromToDialogViewModel()
        {
            DateFrom = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
            DateTo = DateTime.Now.AddDays(6).Date;
        }
    }
}