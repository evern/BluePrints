using BaseModel.Misc;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using System.Windows;
using System.Windows.Controls;

namespace BluePrints.Views
{
    public partial class TransactionCollectionInstantFeedbackView : UserControl
    {
        public TransactionCollectionInstantFeedbackView()
        {
            InitializeComponent();
        }

        //set an empty string so that change tracker can register changes
        private void PART_Editor_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            if ((string)e.NewValue == null)
            {
                (sender as BaseEdit).EditValue = string.Empty;
            }
        }
    }
}