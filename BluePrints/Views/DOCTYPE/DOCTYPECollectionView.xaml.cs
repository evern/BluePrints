using BaseModel.Misc;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Settings;
using System.Windows.Controls;
using System.Windows.Forms;

namespace BluePrints.Views
{
    public partial class DOCTYPECollectionView : System.Windows.Controls.UserControl
    {
        public DOCTYPECollectionView()
        {
            InitializeComponent();
        }

        private void GridControl_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            InplaceBaseEdit inplaceEdit = e.OriginalSource as InplaceBaseEdit;
            if (inplaceEdit != null)
            {
                CheckEditSettings checkEdit = inplaceEdit.Settings as CheckEditSettings;
                if (checkEdit != null)
                    SendKeys.SendWait(" ");
            }
        }
    }
}