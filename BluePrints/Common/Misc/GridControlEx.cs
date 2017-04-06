using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using DevExpress.Xpf.Grid;
using System.Windows.Data;

namespace BluePrints.Common
{
    public class TableViewEx : TableView
    {
        //public bool isEditorActive;

        //public TableViewEx()
        //{
        //    this.PreviewKeyDown += TableViewEx_PreviewKeyDown;
        //    this.ShownEditor += TableViewEx_ShownEditor;
        //    this.HiddenEditor += TableViewEx_HiddenEditor;
        //}

        //private void TableViewEx_HiddenEditor(object sender, EditorEventArgs e)
        //{
        //    isEditorActive = false;
        //}

        //private void TableViewEx_ShownEditor(object sender, EditorEventArgs e)
        //{
        //    isEditorActive = true;
        //}

        //void TableViewEx_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        //{
        //    if (e.Key == Key.Enter)
        //    {
        //        Dispatcher.BeginInvoke(new Action(() =>
        //        {
        //            CommitEditing();
        //            MoveNextRow();
        //        }));
        //    }
        //}
    }
}
