using BluePrints.Common.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BluePrints.Common.ViewModel.UndoRedo;
using DevExpress.Xpf.Grid;
using System.Collections.ObjectModel;
using DevExpress.Xpf.Bars;
using System.Windows;
using DevExpress.Mvvm.POCO;
using BluePrints.Data.Attributes;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using DevExpress.Mvvm;
using DevExpress.Xpf.Editors;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using BluePrints.Common.Utils;
using BluePrints.Data.Helpers;
using BluePrints.Common.ViewModel.Filtering;
using BluePrints.Common.ViewModel.Utils;
using DevExpress.Xpf.Editors.Settings;

namespace BluePrints.Common.ViewModel
{
    /// <summary>
    /// The base class for a POCO view models exposing a collection of entities of a given type and CRUD operations against these entities.
    /// This is a partial class that provides extension point to add custom properties, commands and override methods without modifying the auto-generated code.
    /// All extensions from DevExpress will be implemented here
    /// </summary>
    /// <typeparam name="TEntity">An entity type.</typeparam>
    /// <typeparam name="TPrimaryKey">A primary key value type.</typeparam>
    /// <typeparam name="TUnitOfWork">A unit of work type.</typeparam>
    public partial class CollectionViewModel<TEntity, TPrimaryKey, TUnitOfWork> :
        CollectionViewModel<TEntity, TEntity, TPrimaryKey, TUnitOfWork>, ISupportUndoRedo<TEntity>,
        ISupportFiltering<TEntity>
        where TEntity : class
        where TUnitOfWork : IUnitOfWork
    {
        //#region Data Operations
        ///// <summary>
        ///// Converts clipboard text into entity values and saves to database
        ///// </summary>
        ///// <param name="e"></param>
        //public override void PastingFromClipboard(PastingFromClipboardEventArgs e)
        //{
        //    String PasteString = Clipboard.GetText();
        //    string[] RowData = PasteString.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        //    GridControl sourceGridControl = (GridControl)e.Source;
        //    PropertyInfo[] entityProperties = typeof(TEntity).GetProperties();
        //    DataViewBase gridView = sourceGridControl.View;

        //    if (gridView.GetType() == typeof(TableView))
        //    {
        //        TableView gridTableView = gridView as TableView;
        //        foreach (string Row in RowData)
        //        {
        //            TEntity newEntity = CreateEntity();

        //            string[] ColumnStrings = Row.Split('\t');
        //            for (int i = 0; i < ColumnStrings.Count(); i++)
        //            {
        //                try
        //                {
        //                    string s = string.Empty;
        //                    GridColumn copyColumn = gridTableView.VisibleColumns[i];

        //                    if (copyColumn.ReadOnly)
        //                        continue;

        //                    string columnName = copyColumn.FieldName;
        //                    PropertyInfo columnPropertyInfo = newEntity.GetType().GetProperty(columnName);
        //                    if (columnPropertyInfo != null)
        //                    {
        //                        if (columnPropertyInfo.PropertyType == typeof(Guid))
        //                        {
        //                            ComboBoxEditSettings copyColumnEditSettings = copyColumn.ActualEditSettings as ComboBoxEditSettings;
        //                            if (copyColumnEditSettings != null)
        //                            {
        //                                string copyColumnValueMember = copyColumnEditSettings.ValueMember;
        //                                string copyColumnDisplayMember = copyColumnEditSettings.DisplayMember;
        //                                var copyColumnItemsSource = copyColumnEditSettings.ItemsSource as IEnumerable<object>;
        //                                Guid? itemValue = null;
        //                                foreach (var copyColumnItem in copyColumnItemsSource)
        //                                {
        //                                    PropertyInfo itemDisplayMemberPropertyInfo = copyColumnItem.GetType().GetProperty(copyColumnDisplayMember);
        //                                    PropertyInfo itemValueMemberPropertyInfo = copyColumnItem.GetType().GetProperty(copyColumnValueMember);
        //                                    if (itemDisplayMemberPropertyInfo.GetValue(copyColumnItem).ToString() == ColumnStrings[i])
        //                                    {
        //                                        itemValue = (Guid)itemValueMemberPropertyInfo.GetValue(copyColumnItem);
        //                                    }
        //                                }

        //                                if (itemValue != null)
        //                                    newEntity.GetType().GetProperty(columnName).SetValue(newEntity, itemValue);
        //                                else
        //                                    break;
        //                            }
        //                        }
        //                        else if (columnPropertyInfo.PropertyType == typeof(string))
        //                            newEntity.GetType().GetProperty(columnName).SetValue(newEntity, ColumnStrings[i]);
        //                        else if (columnPropertyInfo.PropertyType.BaseType == typeof(Enum))
        //                        {
        //                            var enumValues = Enum.GetValues(columnPropertyInfo.PropertyType);
        //                            foreach (var enumValue in enumValues)
        //                            {
        //                                var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
        //                                if (fieldInfo == null)
        //                                    return;

        //                                var descriptionAttributes = fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];
        //                                if (descriptionAttributes == null || descriptionAttributes.Count() == 0)
        //                                    return;

        //                                var descriptionAttribute = descriptionAttributes.First();
        //                                if (ColumnStrings[i] == descriptionAttribute.Name)
        //                                {
        //                                    newEntity.GetType().GetProperty(columnName).SetValue(newEntity, enumValue);
        //                                    break;
        //                                }
        //                            }
        //                        }
        //                        else if (columnPropertyInfo.PropertyType == typeof(decimal) || columnPropertyInfo.PropertyType == typeof(int) || columnPropertyInfo.PropertyType == typeof(double))
        //                        {
        //                            Regex rgx = new Regex("[^0-9a-z\\.]");
        //                            string cleanColumnString = rgx.Replace(ColumnStrings[i], "");

        //                            if (columnPropertyInfo.PropertyType == typeof(decimal))
        //                            {
        //                                decimal getDecimal;
        //                                if (decimal.TryParse(cleanColumnString, out getDecimal))
        //                                {
        //                                    if (columnName.Contains('%') || columnName.ToUpper().Contains("PERCENT"))
        //                                        getDecimal /= 100;

        //                                    newEntity.GetType().GetProperty(columnName).SetValue(newEntity, getDecimal);
        //                                }
        //                                else
        //                                    return;
        //                            }
        //                            else if (columnPropertyInfo.PropertyType == typeof(int))
        //                            {
        //                                int getInt;
        //                                if (int.TryParse(cleanColumnString, out getInt))
        //                                    newEntity.GetType().GetProperty(columnName).SetValue(newEntity, getInt);
        //                                else
        //                                    return;
        //                            }
        //                            else if (columnPropertyInfo.PropertyType == typeof(double))
        //                            {
        //                                double getDouble;
        //                                if (double.TryParse(cleanColumnString, out getDouble))
        //                                {
        //                                    if (columnName.Contains('%') || columnName.ToUpper().Contains("PERCENT"))
        //                                        getDouble /= 100;

        //                                    newEntity.GetType().GetProperty(columnName).SetValue(newEntity, getDouble);
        //                                }
        //                                else
        //                                    return;
        //                            }
        //                            else
        //                                return;
        //                        }
        //                        else
        //                            return;
        //                    }
        //                    else
        //                        return;
        //                }
        //                catch
        //                {
        //                    return;
        //                }
        //            }

        //            string errorMessage = "Duplicate exists on constraint field named: ";
        //            if (IsValidEntity(newEntity, ref errorMessage))
        //                Save(newEntity);
        //            else
        //            {
        //                errorMessage += " , paste operation will be terminated";
        //                MessageBoxService.ShowMessage(errorMessage, CommonResources.Exception_UpdateErrorCaption, MessageButton.OK);
        //                break;
        //            }
        //        }
        //    }

        //    e.Handled = true;
        //}
        //#endregion
    }
}