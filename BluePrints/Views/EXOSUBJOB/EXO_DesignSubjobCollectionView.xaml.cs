using BaseModel.Misc;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Settings;
using System.Windows.Controls;
using System.Windows.Forms;

namespace BluePrints.Views
{
    public partial class EXO_DesignSubjobCollectionView : System.Windows.Controls.UserControl
    {
        public EXO_DesignSubjobCollectionView()
        {
            InitializeComponent();
        }

        private void tableView1_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
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