using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Dialogs;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BluePrints.Common.ViewModel
{
    public class MapDeliverablesClientNumberDialogViewModel<TEntity> : DialogCollectionViewModel<TEntity>
        where TEntity : ClientNumberAssignment
    {
        public static MapDeliverablesClientNumberDialogViewModel<TEntity> CreateViewModel(IEnumerable<TEntity> enumerableObjects)
        {
            return ViewModelSource.Create(() => new MapDeliverablesClientNumberDialogViewModel<TEntity>(enumerableObjects));
        }

        public MapDeliverablesClientNumberDialogViewModel(IEnumerable<TEntity> enumerableObjects)
            : base(enumerableObjects)
        {
        }

        /// <summary>
        /// Converts clipboard text into entity values
        /// </summary>
        /// <param name="e"></param>
        public void PastingFromClipboard(PastingFromClipboardEventArgs e)
        {
            var PasteString = Clipboard.GetText();
            var RowData = PasteString.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var sourceGridControl = (GridControl)e.Source;
            foreach (var Row in RowData)
            {
                var ColumnStrings = Row.Split('\t');
                if (ColumnStrings.Count() != 2)
                    break;

                string internal_number = ColumnStrings[0];
                ClientNumberAssignment find_internal_number = SourceObjects.FirstOrDefault(x => x.INTERNAL_NUM == internal_number);
                if(find_internal_number != null)
                {
                    find_internal_number.CLIENT_NUM = ColumnStrings[1];
                    IPOCOViewModel pocoViewModel = find_internal_number as IPOCOViewModel;
                    if(pocoViewModel != null)
                        pocoViewModel.RaisePropertiesChanged();
                }
            }
        }
    }
}
