using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using BluePrints.Common;
using BluePrints.Data;
using DevExpress.Xpf.Grid.TreeList;

namespace BluePrints.ViewModels
{
    public class AddCOMMODITYViewModel
    {
        public static AddCOMMODITYViewModel Create(IEnumerable<COMMODITY_CODE> commodityCodes)
        {
            return ViewModelSource.Create(() => new AddCOMMODITYViewModel(commodityCodes));
        }

        public COMMODITY addCOMMODITY { get; set; }
        public List<COMMODITY_CODE> COMMODITY_CODES { get; set; }
        public COMMODITY_CODE SelectedCOMMODITY_CODE { get; set; }
        public string ComboBoxDisplayMember { get; set; }

        protected AddCOMMODITYViewModel(IEnumerable<COMMODITY_CODE> commodityCodes)
        {
            COMMODITY_CODES = new List<COMMODITY_CODE>(commodityCodes);
            addCOMMODITY = new COMMODITY();
        }

        public COMMODITY newCOMMODITY
        {
            get { return addCOMMODITY; }
        }
    }
}
