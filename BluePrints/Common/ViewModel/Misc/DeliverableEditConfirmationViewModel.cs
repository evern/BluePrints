using BluePrints.Common.ViewModel.Misc;
using BluePrints.Data;
using DevExpress.Mvvm.POCO;
using System.Collections.Generic;

namespace BaseModel.ViewModel.Dialogs
{
    public class DeliverableEditConfirmationViewModel
    {
        public static DeliverableEditConfirmationViewModel Create(IEnumerable<DeliverableEditModel> enumerableObjects, string label, IEnumerable<DISCIPLINE> disciplines)
        {
            return ViewModelSource.Create(() => new DeliverableEditConfirmationViewModel(enumerableObjects, label, disciplines));
        }

        public IEnumerable<DeliverableEditModel> SourceObjects { get; set; }
        public IEnumerable<DISCIPLINE> DISCIPLINECollection { get; set; }
        public string Label { get; set; }
        protected DeliverableEditConfirmationViewModel(IEnumerable<DeliverableEditModel> enumerableObjects, string label, IEnumerable<DISCIPLINE> disciplines)
        {
            SourceObjects = enumerableObjects;
            Label = label;

            DISCIPLINECollection = disciplines;
        }
    }
}