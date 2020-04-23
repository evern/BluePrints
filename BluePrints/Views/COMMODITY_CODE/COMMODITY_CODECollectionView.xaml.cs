using BaseModel.Misc;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Settings;
using System.Windows.Controls;
using System.Windows.Forms;

namespace BluePrints.Views
{
    /// <summary>
    /// Interaction logic for AREADetailsCollectionView.xaml
    /// </summary>
    public partial class COMMODITY_CODECollectionView : System.Windows.Controls.UserControl
    {
        public COMMODITY_CODECollectionView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Circumvent an issue with checkedit not responsive on treelist because of drag drop manager and cellvaluechanged being implemented
        /// </summary>
        private void GridControl_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed)
                return;

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