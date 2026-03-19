using System;
using System.Collections;
using System.Linq;
using System.Web.UI.WebControls;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Web.Editors.ASPx;
using DevExpress.Utils;
using DevExpress.Web;

namespace CollectionsResolution.Module.Web.Editors
{
    /// <summary>
    /// Custom Property Editor to display CollectionItem collection in a grid
    /// for testing collection rendering in Web applications.
    /// This editor renders an ASPxGridView to display and edit the collection.
    /// </summary>
    [PropertyEditor(typeof(IList), "CollectionItemsPropertyEditor", false)]
    public class CollectionItemsPropertyEditor : ASPxPropertyEditor
    {
        private ASPxGridView grid;
        private Panel containerPanel;
        private const string GRID_ID = "testCollectionItemsGrid";

        public CollectionItemsPropertyEditor(Type objectType, IModelMemberViewItem model)
            : base(objectType, model)
        {
        }

        protected override WebControl CreateEditModeControlCore()
        {
            containerPanel = new Panel();
            containerPanel.ID = "testCollectionItemsPanel";
            containerPanel.Width = Unit.Percentage(100);

            grid = new ASPxGridView();
            grid.ID = GRID_ID;
            grid.Width = Unit.Percentage(100);
            grid.AutoGenerateColumns = false;
            grid.EnableCallBacks = false;
            grid.KeyFieldName = "Code";
            
            ConfigureGrid();
            DefineColumns();

            containerPanel.Controls.Add(grid);

            return containerPanel;
        }

        protected override WebControl CreateViewModeControlCore()
        {
            return CreateEditModeControlCore();
        }

        protected override void ReadEditModeValueCore()
        {
            // Sync changes back if needed
        }

        protected override object GetControlValueCore()
        {
            return PropertyValue;
        }

        protected override void ReadValueCore()
        {
            base.ReadValueCore();
            RefreshGrid();
        }

        public override void Refresh()
        {
            base.Refresh();
            RefreshGrid();
        }

        private void ConfigureGrid()
        {
            grid.Settings.ShowFilterRow = false;
            grid.Settings.ShowGroupPanel = false;
            grid.Settings.ShowHeaderFilterButton = false;
            grid.Settings.ShowFooter = false;

            grid.SettingsEditing.Mode = GridViewEditingMode.Inline;
            
            grid.SettingsBehavior.AllowSelectByRowClick = false;
            grid.SettingsBehavior.AllowFocusedRow = true;

            grid.SettingsPager.Mode = GridViewPagerMode.ShowAllRecords;
            grid.SettingsPager.Visible = false;

            grid.Styles.Header.Wrap = DefaultBoolean.True;
            grid.Styles.Cell.Wrap = DefaultBoolean.False;

            grid.StartRowEditing += Grid_StartRowEditing;
            grid.CancelRowEditing += Grid_CancelRowEditing;
            grid.RowInserting += Grid_RowInserting;
            grid.RowUpdating += Grid_RowUpdating;
            grid.RowDeleting += Grid_RowDeleting;
        }

        private void DefineColumns()
        {
            GridViewCommandColumn editColumn = new GridViewCommandColumn();
            editColumn.ShowEditButton = true;
            editColumn.Width = Unit.Pixel(100);
            editColumn.Caption = "Edit";
            grid.Columns.Add(editColumn);

            AddTextColumn("Code", "Code", 120, false);
            AddTextColumn("Name", "Name", 200, true);
            AddDecimalColumn("Amount", "Amount", 120, true);
            AddDateColumn("Date", "Date", 120, true);
            AddCheckBoxColumn("IsActive", "Is Active", 100, true);
        }

        private void AddTextColumn(string fieldName, string caption, int width, bool allowEdit)
        {
            var column = new GridViewDataTextColumn();
            column.FieldName = fieldName;
            column.Caption = caption;
            column.Width = Unit.Pixel(width);
            column.Settings.AllowHeaderFilter = DefaultBoolean.False;
            column.Settings.AllowSort = DefaultBoolean.True;

            if (!allowEdit)
            {
                column.ReadOnly = true;
            }

            grid.Columns.Add(column);
        }

        private void AddDecimalColumn(string fieldName, string caption, int width, bool allowEdit)
        {
            var column = new GridViewDataSpinEditColumn();
            column.FieldName = fieldName;
            column.Caption = caption;
            column.Width = Unit.Pixel(width);
            column.Settings.AllowHeaderFilter = DefaultBoolean.False;
            column.Settings.AllowSort = DefaultBoolean.True;
            column.PropertiesSpinEdit.NumberType = DevExpress.Web.SpinEditNumberType.Float;
            column.PropertiesSpinEdit.DecimalPlaces = 2;

            if (!allowEdit)
            {
                column.ReadOnly = true;
            }

            grid.Columns.Add(column);
        }

        private void AddDateColumn(string fieldName, string caption, int width, bool allowEdit)
        {
            var column = new GridViewDataDateColumn();
            column.FieldName = fieldName;
            column.Caption = caption;
            column.Width = Unit.Pixel(width);
            column.Settings.AllowHeaderFilter = DefaultBoolean.False;
            column.Settings.AllowSort = DefaultBoolean.True;
            column.PropertiesDateEdit.DisplayFormatString = "d";

            if (!allowEdit)
            {
                column.ReadOnly = true;
            }

            grid.Columns.Add(column);
        }

        private void AddCheckBoxColumn(string fieldName, string caption, int width, bool allowEdit)
        {
            var column = new GridViewDataCheckColumn();
            column.FieldName = fieldName;
            column.Caption = caption;
            column.Width = Unit.Pixel(width);
            column.Settings.AllowHeaderFilter = DefaultBoolean.False;
            column.Settings.AllowSort = DefaultBoolean.True;

            if (!allowEdit)
            {
                column.ReadOnly = true;
            }

            grid.Columns.Add(column);
        }

        private void RefreshGrid()
        {
            if (grid != null && PropertyValue != null)
            {
                var collection = PropertyValue as IList;
                if (collection != null)
                {
                    var list = collection.Cast<object>().ToList();
                    grid.DataSource = list;
                    grid.DataBind();
                }
            }
        }

        private void Grid_StartRowEditing(object sender, DevExpress.Web.Data.ASPxStartRowEditingEventArgs e)
        {
            // Allow editing
        }

        private void Grid_CancelRowEditing(object sender, DevExpress.Web.Data.ASPxStartRowEditingEventArgs e)
        {
            RefreshGrid();
        }

        private void Grid_RowInserting(object sender, DevExpress.Web.Data.ASPxDataInsertingEventArgs e)
        {
            e.Cancel = true;
        }

        private void Grid_RowUpdating(object sender, DevExpress.Web.Data.ASPxDataUpdatingEventArgs e)
        {
            try
            {
                var collection = PropertyValue as IList;
                if (collection != null && e.Keys != null && e.Keys.Count > 0)
                {
                    string code = e.Keys["Code"]?.ToString();
                    
                    if (!string.IsNullOrEmpty(code))
                    {
                        CollectionsResolution.Module.NonPersistentBusinessObjects.CollectionRendering.CollectionItemNonPersistent item = null;
                        foreach (var obj in collection)
                        {
                            var testItem = obj as CollectionsResolution.Module.NonPersistentBusinessObjects.CollectionRendering.CollectionItemNonPersistent;
                            if (testItem != null && testItem.Code == code)
                            {
                                item = testItem;
                                break;
                            }
                        }
                        
                        if (item != null)
                        {
                            foreach (var key in e.NewValues.Keys)
                            {
                                var propertyInfo = item.GetType().GetProperty(key.ToString());
                                if (propertyInfo != null && propertyInfo.CanWrite)
                                {
                                    var value = e.NewValues[key];
                                    
                                    if (value != null)
                                    {
                                        if (propertyInfo.PropertyType == typeof(bool))
                                        {
                                            value = Convert.ToBoolean(value);
                                        }
                                        else if (propertyInfo.PropertyType == typeof(decimal))
                                        {
                                            value = Convert.ToDecimal(value);
                                        }
                                        else if (propertyInfo.PropertyType == typeof(DateTime))
                                        {
                                            value = Convert.ToDateTime(value);
                                        }
                                    }
                                    
                                    propertyInfo.SetValue(item, value);
                                }
                            }
                        }
                    }
                }
                e.Cancel = true;
                grid.CancelEdit();
                RefreshGrid();
            }
            catch
            {
                e.Cancel = true;
            }
        }

        private void Grid_RowDeleting(object sender, DevExpress.Web.Data.ASPxDataDeletingEventArgs e)
        {
            e.Cancel = true;
        }

        public override void BreakLinksToControl(bool unwireEventsOnly)
        {
            if (!unwireEventsOnly)
            {
                if (grid != null)
                {
                    grid.StartRowEditing -= Grid_StartRowEditing;
                    grid.CancelRowEditing -= Grid_CancelRowEditing;
                    grid.RowInserting -= Grid_RowInserting;
                    grid.RowUpdating -= Grid_RowUpdating;
                    grid.RowDeleting -= Grid_RowDeleting;
                    grid.DataSource = null;
                }
            }
            base.BreakLinksToControl(unwireEventsOnly);
        }
    }
}
