using BluePrints.Common.ViewModel.Misc;
using BluePrints.Data;
using DevExpress.Mvvm.POCO;
using System.Collections.Generic;

namespace BaseModel.ViewModel.Dialogs
{
    public class TenderProfileItemEditConfirmationViewModel
    {
        public static TenderProfileItemEditConfirmationViewModel Create(IEnumerable<TenderProfileItemEditModel> enumerableObjects, string label, IEnumerable<DEPARTMENT> departments, IEnumerable<DISCIPLINE> disciplines)
        {
            return ViewModelSource.Create(() => new TenderProfileItemEditConfirmationViewModel(enumerableObjects, label, departments, disciplines));
        }

        public IEnumerable<TenderProfileItemEditModel> SourceObjects { get; set; }
        public IEnumerable<DISCIPLINE> DISCIPLINECollection { get; set; }
        public IEnumerable<DEPARTMENT> DEPARTMENTCollection { get; set; }
        public string Label { get; set; }
        protected TenderProfileItemEditConfirmationViewModel(IEnumerable<TenderProfileItemEditModel> enumerableObjects, string label, IEnumerable<DEPARTMENT> departments, IEnumerable<DISCIPLINE> disciplines)
        {
            SourceObjects = enumerableObjects;
            Label = label;

            DEPARTMENTCollection = departments;
            DISCIPLINECollection = disciplines;
        }
    }
}