using BaseModel.Misc;
using BluePrints.Common.Projections;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using System.Windows.Controls;
using System.Windows.Forms;

namespace BluePrints.Views
{
    /// <summary>
    /// Interaction logic for PROJECTVARIATIONDetailsCollectionView.xaml
    /// </summary>
    public partial class VARIATIONCollectionView : System.Windows.Controls.UserControl
    {
        public VARIATIONCollectionView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Circumvent an issue with checkedit not responsive on treelist because of drag drop manager and cellvaluechanged being implemented
        /// </summary>
        private void GridControl_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            InplaceBaseEdit inplaceEdit = e.OriginalSource as InplaceBaseEdit;
            if (inplaceEdit != null)
            {
                CheckEditSettings checkEdit = inplaceEdit.Settings as CheckEditSettings;
                if (checkEdit != null)
                {
                    SendKeys.SendWait(" ");
                }
            }
        }
    }
}