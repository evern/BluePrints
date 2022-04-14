using System;
using DevExpress.Mvvm.POCO;

namespace BaseModel.ViewModel.Dialogs
{
    public class DateFromToDialogViewModel
    {
        public static DateFromToDialogViewModel Create(string labelFrom = "From: ", string labelTo = "To: ")
        {
            return ViewModelSource.Create(() => new DateFromToDialogViewModel(labelFrom, labelTo));
        }

        public string LabelFrom { get; set; }
        public string LabelTo { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        protected DateFromToDialogViewModel(string labelFrom, string labelTo)
        {
            DateFrom = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
            DateTo = DateFrom.AddDays(6).Date;
            LabelFrom = labelFrom;
            LabelTo = labelTo;
        }
    }
}