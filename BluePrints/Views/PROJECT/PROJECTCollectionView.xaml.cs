using BaseModel.Misc;

namespace BluePrints.Views
{
    public partial class PROJECTCollectionView : ViewStateRestoreUserControl
    {
        public PROJECTCollectionView()
        {
            InitializeComponent();
            InitializeViewControl(GridControl, tableView);
        }

        private void HyperlinkEditSettings_RequestNavigation(object sender, DevExpress.Xpf.Editors.HyperlinkEditRequestNavigationEventArgs e)
        {
            string s = e.ToString();
        }
    }
}