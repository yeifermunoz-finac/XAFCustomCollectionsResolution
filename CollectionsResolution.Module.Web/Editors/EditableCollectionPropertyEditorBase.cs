using System;
using System.Collections;
using System.Linq;
using System.Web.UI.WebControls;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Web.Editors.ASPx;
using DevExpress.Utils;
using DevExpress.Web;

namespace CollectionsResolution.Module.Web.Editors
{
    /// <summary>
    /// Base class for editable collection property editors in Web applications.
    /// Provides common functionality for displaying and editing collections in ASPxGridView with inline editing.
    /// Automatically enables/disables editing based on the parent DetailView's edit mode.
    /// Follows the pattern from NPXMLFormatsListPropertyEditor using postback mode.
    /// </summary>
    /// <summary>
    /// Event args for when user requests to show detail view of an item
    /// </summary>
    public class ShowDetailRequestedEventArgs : EventArgs
    {
        public object Item { get; set; }
        public ShowDetailRequestedEventArgs(object item)
        {
            Item = item;
        }
    }

    public abstract class EditableCollectionPropertyEditorBase : ASPxPropertyEditor
    {
        protected ASPxGridView grid;
        protected Panel containerPanel;
        private IList collectionSource;
        private bool? isEditMode = null;
        private GridViewCommandColumn commandColumn;

        /// <summary>
        /// Fired when user clicks the "Details" button for an item
        /// </summary>
        public event EventHandler<ShowDetailRequestedEventArgs> ShowDetailRequested;

        protected EditableCollectionPropertyEditorBase(Type objectType, IModelMemberViewItem model)
            : base(objectType, model)
        {
        }

        private void DetailView_ViewEditModeChanged(object sender, EventArgs e)
        {
            isEditMode = null; // Force state re-evaluation
            UpdateEditingState();
        }

        protected override WebControl CreateEditModeControlCore()
        {
            // Force state re-evaluation when a new control is created
            isEditMode = null;

            containerPanel = new Panel
            {
                ID = GetPanelId(),
                Width = Unit.Percentage(100)
            };

            grid = new ASPxGridView
            {
                ID = GetGridId(),
                Width = Unit.Percentage(100),
                AutoGenerateColumns = false,
                EnableCallBacks = false  // Use postbacks for inline editing (like NPXMLFormatsListPropertyEditor)
            };

            // Set key field name (must be done before defining columns)
            grid.KeyFieldName = GetKeyFieldName();

            // Grid settings
            grid.Settings.ShowFilterRow = false;
            grid.Settings.ShowGroupPanel = false;
            grid.Settings.ShowHeaderFilterButton = false;
            grid.Settings.ShowFooter = false;

            // Enable inline editing
            grid.SettingsEditing.Mode = GridViewEditingMode.Inline;
            
            // Enable rows cache for better postback performance
            grid.EnableRowsCache = true;
            
            // Behavior settings
            grid.SettingsBehavior.AllowFocusedRow = false;
            grid.SettingsBehavior.AllowSelectByRowClick = false;

            // Paging settings - show all records
            grid.SettingsPager.Mode = GridViewPagerMode.ShowAllRecords;
            grid.SettingsPager.Visible = false;

            // Styles
            grid.Styles.Header.Wrap = DefaultBoolean.True;
            grid.Styles.Cell.Wrap = DefaultBoolean.False;

            // Add hidden Oid column first (required for tracking items by Oid)
            AddHiddenColumn("Oid");

            // Define columns (implemented by derived classes)
            DefineColumns();

            // Add command column for CRUD operations
            // Update/Cancel buttons appear automatically when in edit/insert mode for inline editing
            commandColumn = new GridViewCommandColumn
            {
                ShowNewButtonInHeader = true,   // Show "New" button in header
                ShowEditButton = true,          // Show "Edit" button for each row
                ShowDeleteButton = true,        // Show "Delete" button for each row
                ShowUpdateButton = true,        // Show "Update" button (appears in edit mode)
                ShowCancelButton = true,        // Show "Cancel" button (appears in edit mode)
                Width = Unit.Pixel(200),
                ButtonRenderMode = GridCommandButtonRenderMode.Button,
                Caption = "Actions",
                VisibleIndex = 0,
                Visible = true  // Must be visible for inline editing to work
            };
            
            // Add custom "Details" button to the command column
            var detailsButton = new GridViewCommandColumnCustomButton
            {
                ID = "btnDetails",
                Text = "Details"
            };
            commandColumn.CustomButtons.Add(detailsButton);
            
            grid.Columns.Insert(0, commandColumn);

            // Wire up grid events for CRUD operations
            grid.InitNewRow += Grid_InitNewRow;
            grid.StartRowEditing += Grid_StartRowEditing;
            grid.CancelRowEditing += Grid_CancelRowEditing;
            grid.RowInserting += Grid_RowInserting;
            grid.RowUpdating += Grid_RowUpdating;
            grid.RowDeleting += Grid_RowDeleting;
            grid.CustomButtonCallback += Grid_CustomButtonCallback;

            // Pre-configure grid with expected data type (additional safeguard)
            // This helps ASPxGridView know what type of objects to expect
            // Especially important for empty collections during first load
            if (MemberInfo != null && MemberInfo.ListElementType != null)
            {
                grid.ForceDataRowType(MemberInfo.ListElementType);
            }

            // CRITICAL: Enable data security for inline editing to work
            // These must be TRUE for the grid to render edit controls
            grid.SettingsDataSecurity.AllowInsert = true;
            grid.SettingsDataSecurity.AllowEdit = true;
            grid.SettingsDataSecurity.AllowDelete = true;

            containerPanel.Controls.Add(grid);
            
            // Subscribe to view events after control creation
            if (View != null && View is DetailView detailView)
            {
                detailView.ViewEditModeChanged += DetailView_ViewEditModeChanged;
            }

            return containerPanel;
        }

        protected override WebControl CreateViewModeControlCore()
        {
            return CreateEditModeControlCore();
        }

        protected override void ReadEditModeValueCore()
        {
            // The grid is bound to the collection, changes are handled in events
        }

        protected override object GetControlValueCore()
        {
            return PropertyValue;
        }

        protected override void ReadValueCore()
        {
            base.ReadValueCore();
            RefreshGrid();
            UpdateEditingState();
        }

        public override void Refresh()
        {
            base.Refresh();
            RefreshGrid();
            UpdateEditingState();
        }

        protected override void OnCurrentObjectChanged()
        {
            base.OnCurrentObjectChanged();
            UpdateEditingState();
        }

        protected override void ApplyReadOnly()
        {
            base.ApplyReadOnly();
            isEditMode = null; // Force state re-evaluation
            UpdateEditingState();
        }

        protected override void SetupControl(WebControl control)
        {
            base.SetupControl(control);
            isEditMode = null;
            UpdateEditingState();
        }

        protected override void OnAllowEditChanged()
        {
            base.OnAllowEditChanged();
            isEditMode = null;
            UpdateEditingState();
        }

        private void UpdateEditingState()
        {
            if (grid == null || commandColumn == null)
                return;

            // Determine if we're in edit mode based on DetailView.ViewEditMode
            bool shouldEnableEditing = false;
            
            // In XAF, collection properties typically don't have setters, so AllowEdit on the 
            // property editor itself will be false. We should rely on the DetailView's edit mode
            // and the Model's AllowEdit specifically for this property if set.
            // For now, if it's a collection, we allow editing of items if the view is in edit mode.
            if (View is DetailView detailView)
            {
                shouldEnableEditing = detailView.ViewEditMode == ViewEditMode.Edit;
                
                // For list properties (collections), we usually ignore the property's AllowEdit 
                // because collections naturally don't have setters. We only disable if explicitly read-only.
                if (Model != null && !Model.AllowEdit && MemberInfo != null && !MemberInfo.IsList)
                {
                    shouldEnableEditing = false;
                }
            }

            // Only update if state changed (null != false -> true, so it will apply on first run)
            if (isEditMode != shouldEnableEditing)
            {
                isEditMode = shouldEnableEditing;
                ApplyEditingState();
            }
        }

        private void ApplyEditingState()
        {
            if (grid != null && isEditMode.HasValue)
            {
                bool editMode = isEditMode.Value;
                
                // Enable/disable CRUD operations based on DetailView edit mode
                // When in view mode, grid shows but editing is disabled
                grid.SettingsDataSecurity.AllowInsert = editMode;
                grid.SettingsDataSecurity.AllowEdit = editMode;
                grid.SettingsDataSecurity.AllowDelete = editMode;

                // Make all editable columns read-only in view mode
                foreach (GridViewColumn column in grid.Columns)
                {
                    if (column is GridViewDataColumn dataColumn && column != commandColumn)
                    {
                        // Columns become read-only in view mode
                        dataColumn.ReadOnly = !editMode;
                    }
                }
            }
        }

        private void Grid_InitNewRow(object sender, DevExpress.Web.Data.ASPxDataInitNewRowEventArgs e)
        {
            // CRITICAL: Initialize a temporary Oid for the grid's state tracking
            // The DevExpress ASPxGridView uses KeyFieldName (Oid) to track row state
            // Without a key value in e.NewValues, the grid's state machine cannot properly
            // identify the row as being in "insert" mode, causing cells to appear non-editable
            // This is especially critical for the first item in an empty collection
            e.NewValues["Oid"] = Guid.NewGuid();
            
            // Call virtual method for derived classes to set additional default values
            // The actual object will be created in RowInserting when user clicks Update
            OnInitNewRow(e);
        }

        private void Grid_StartRowEditing(object sender, DevExpress.Web.Data.ASPxStartRowEditingEventArgs e)
        {
            // Allow editing to start
            // The grid will automatically enter edit mode for the row
            OnStartRowEditing(e);
        }

        private void Grid_CancelRowEditing(object sender, DevExpress.Web.Data.ASPxStartRowEditingEventArgs e)
        {
            // Cancel editing - grid will handle reverting changes
            OnCancelRowEditing(e);
        }

        private void Grid_CustomButtonCallback(object sender, ASPxGridViewCustomButtonCallbackEventArgs e)
        {
            if (e.ButtonID == "btnDetails")
            {
                // Only show details if the row is not currently in edit/insert mode
                // This prevents showing detail popup while user is editing inline
                if (!grid.IsEditing && !grid.IsNewRowEditing)
                {
                    // Get the Oid from the row's key value
                    var oid = grid.GetRowValues(e.VisibleIndex, "Oid");
                    if (oid != null && PropertyValue is IList collection)
                    {
                        // Find the object by Oid
                        object item = null;
                        foreach (var obj in collection)
                        {
                            var oidProp = obj.GetType().GetProperty("Oid");
                            if (oidProp != null)
                            {
                                var itemOid = oidProp.GetValue(obj, null);
                                if (itemOid != null && itemOid.Equals(oid))
                                {
                                    item = obj;
                                    break;
                                }
                            }
                        }
                        
                        if (item != null)
                        {
                            // Trigger the detail view display
                            OnShowDetailRequested(item);
                        }
                    }
                }
            }
        }

        private void Grid_RowInserting(object sender, DevExpress.Web.Data.ASPxDataInsertingEventArgs e)
        {
            try
            {
                // Create new item and add to collection
                if (PropertyValue is IList collection && MemberInfo != null)
                {
                    var itemType = MemberInfo.ListElementType;
                    
                    // Use ObjectSpace to create the object properly (supports XPO and Non-Persistent)
                    object newItem;
                    if (View != null && View.ObjectSpace != null)
                    {
                        newItem = View.ObjectSpace.CreateObject(itemType);
                    }
                    else
                    {
                        // Fallback
                        newItem = Activator.CreateInstance(itemType);
                    }
                    
                    // Set properties from grid's new values
                    foreach (var key in e.NewValues.Keys)
                    {
                        var propInfo = itemType.GetProperty(key.ToString());
                        if (propInfo != null && propInfo.CanWrite)
                        {
                            var value = e.NewValues[key];
                            if (value != null)
                            {
                                try
                                {
                                    // Type conversion
                                    if (propInfo.PropertyType != value.GetType())
                                    {
                                        if (propInfo.PropertyType.IsEnum)
                                        {
                                            if (int.TryParse(value.ToString(), out int enumIntValue))
                                                value = Enum.ToObject(propInfo.PropertyType, enumIntValue);
                                            else
                                                value = Enum.Parse(propInfo.PropertyType, value.ToString());
                                        }
                                        else if (propInfo.PropertyType == typeof(bool))
                                            value = Convert.ToBoolean(value);
                                        else if (propInfo.PropertyType == typeof(int))
                                            value = Convert.ToInt32(value);
                                        else if (propInfo.PropertyType == typeof(decimal))
                                            value = Convert.ToDecimal(value);
                                        else if (propInfo.PropertyType == typeof(DateTime))
                                            value = Convert.ToDateTime(value);
                                        else
                                            value = Convert.ChangeType(value, propInfo.PropertyType);
                                    }
                                    propInfo.SetValue(newItem, value, null);
                                }
                                catch
                                {
                                    // If conversion fails, skip this property
                                }
                            }
                        }
                    }
                    
                    // Call virtual method for customization
                    OnItemCreated(newItem, CurrentObject);
                    
                    // Add to collection
                    collection.Add(newItem);
                }
                
                // CRITICAL: Correct sequence for manual insertion
                // 1. Cancel default insertion
                e.Cancel = true;
                
                // 2. Exit edit mode (must be called AFTER e.Cancel = true)
                grid.CancelEdit();
                
                // 3. Refresh grid to show the new item
                RefreshGrid();
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                grid.CancelEdit(); // Exit edit mode on error too
                System.Diagnostics.Debug.WriteLine($"Error inserting row: {ex.Message}");
                throw;
            }
        }

        private void Grid_RowUpdating(object sender, DevExpress.Web.Data.ASPxDataUpdatingEventArgs e)
        {
            try
            {
                // Update existing item
                if (PropertyValue is IList collection && e.Keys != null && e.Keys.Count > 0)
                {
                    var oid = e.Keys["Oid"];
                    if (oid == null) 
                    {
                        e.Cancel = true;
                        return;
                    }
                    
                    // Find item by Oid
                    object itemToUpdate = null;
                    foreach (var item in collection)
                    {
                        var oidProp = item.GetType().GetProperty("Oid");
                        if (oidProp != null)
                        {
                            var itemOid = oidProp.GetValue(item, null);
                            if (itemOid != null && itemOid.Equals(oid))
                            {
                                itemToUpdate = item;
                                break;
                            }
                        }
                    }
                    
                    if (itemToUpdate != null)
                    {
                        var itemType = itemToUpdate.GetType();
                        
                        // Update properties from grid's new values
                        foreach (var key in e.NewValues.Keys)
                        {
                            var propInfo = itemType.GetProperty(key.ToString());
                            if (propInfo != null && propInfo.CanWrite)
                            {
                                var value = e.NewValues[key];
                                try
                                {
                                    // Type conversion
                                    if (value != null && propInfo.PropertyType != value.GetType())
                                    {
                                        if (propInfo.PropertyType.IsEnum)
                                        {
                                            // Try to parse enum
                                            if (int.TryParse(value.ToString(), out int enumIntValue))
                                            {
                                                value = Enum.ToObject(propInfo.PropertyType, enumIntValue);
                                            }
                                            else
                                            {
                                                value = Enum.Parse(propInfo.PropertyType, value.ToString());
                                            }
                                        }
                                        else if (propInfo.PropertyType == typeof(bool))
                                        {
                                            value = Convert.ToBoolean(value);
                                        }
                                        else if (propInfo.PropertyType == typeof(int))
                                        {
                                            value = Convert.ToInt32(value);
                                        }
                                        else if (propInfo.PropertyType == typeof(decimal))
                                        {
                                            value = Convert.ToDecimal(value);
                                        }
                                        else if (propInfo.PropertyType == typeof(DateTime))
                                        {
                                            value = Convert.ToDateTime(value);
                                        }
                                        else
                                        {
                                            value = Convert.ChangeType(value, propInfo.PropertyType);
                                        }
                                    }
                                    propInfo.SetValue(itemToUpdate, value, null);
                                }
                                catch
                                {
                                    // If conversion fails, skip this property
                                }
                            }
                        }
                        
                        // Call virtual method for derived classes
                        OnItemUpdated(itemToUpdate);
                    }
                }
                
                // CRITICAL: Correct sequence for manual update
                // 1. Cancel default update
                e.Cancel = true;
                
                // 2. Exit edit mode (must be called AFTER e.Cancel = true)
                grid.CancelEdit();
                
                // 3. Refresh to show updated values
                RefreshGrid();
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                grid.CancelEdit(); // Exit edit mode on error too
                System.Diagnostics.Debug.WriteLine($"Error updating row: {ex.Message}");
                throw;
            }
        }

        private void Grid_RowDeleting(object sender, DevExpress.Web.Data.ASPxDataDeletingEventArgs e)
        {
            try
            {
                // Delete item from collection
                if (PropertyValue is IList collection && e.Keys != null && e.Keys.Count > 0)
                {
                    var oid = e.Keys["Oid"];
                    if (oid == null)
                    {
                        e.Cancel = true;
                        return;
                    }
                    
                    // Find item by Oid
                    object itemToDelete = null;
                    foreach (var item in collection)
                    {
                        var oidProp = item.GetType().GetProperty("Oid");
                        if (oidProp != null)
                        {
                            var itemOid = oidProp.GetValue(item, null);
                            if (itemOid != null && itemOid.Equals(oid))
                            {
                                itemToDelete = item;
                                break;
                            }
                        }
                    }
                    
                    if (itemToDelete != null && OnItemDeleting(itemToDelete))
                    {
                        collection.Remove(itemToDelete);
                        
                        // For persistent objects, we must explicitly delete them using ObjectSpace
                        // so they are marked as deleted in the database (or removed completely if aggregated).
                        if (View != null && View.ObjectSpace != null)
                        {
                            View.ObjectSpace.Delete(itemToDelete);
                        }
                    }
                }
                
                e.Cancel = true; // Prevent default deletion
                RefreshGrid(); // Refresh to reflect deletion
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                System.Diagnostics.Debug.WriteLine($"Error deleting row: {ex.Message}");
                throw;
            }
        }

        protected void RefreshGrid()
        {
            if (grid != null && PropertyValue != null)
            {
                collectionSource = PropertyValue as IList;
                if (collectionSource != null)
                {
                    var list = collectionSource.Cast<object>().ToList();
                    
                    // CRITICAL FIX: For empty collections, inform the grid about the element type
                    // This fixes the issue where the first item added to an empty collection
                    // appears editable but cells are not actually editable until Update->Edit again
                    // DevExpress ASPxGridView needs type information to create proper edit controls
                    if (list.Count == 0 && MemberInfo != null && MemberInfo.ListElementType != null)
                    {
                        grid.ForceDataRowType(MemberInfo.ListElementType);
                    }
                    
                    grid.DataSource = list;
                    grid.DataBind();
                }
            }
        }

        public override void BreakLinksToControl(bool unwireEventsOnly)
        {
            if (grid != null)
            {
                grid.InitNewRow -= Grid_InitNewRow;
                grid.StartRowEditing -= Grid_StartRowEditing;
                grid.CancelRowEditing -= Grid_CancelRowEditing;
                grid.RowInserting -= Grid_RowInserting;
                grid.RowUpdating -= Grid_RowUpdating;
                grid.RowDeleting -= Grid_RowDeleting;
                grid.CustomButtonCallback -= Grid_CustomButtonCallback;

                if (!unwireEventsOnly)
                {
                    grid.DataSource = null;
                }
            }

            if (View is DetailView detailView)
            {
                detailView.ViewEditModeChanged -= DetailView_ViewEditModeChanged;
            }

            base.BreakLinksToControl(unwireEventsOnly);
        }

        // Abstract and virtual methods for derived classes

        protected abstract string GetPanelId();
        protected abstract string GetGridId();
        protected abstract void DefineColumns();
        
        /// <summary>
        /// Gets the key field name for the grid. Default is "Oid" for NonPersistentLiteObject.
        /// Override this if your objects use a different key field.
        /// </summary>
        protected virtual string GetKeyFieldName()
        {
            return "Oid";
        }

        protected virtual void OnInitNewRow(DevExpress.Web.Data.ASPxDataInitNewRowEventArgs e)
        {
            // Derived classes can override to set default values for new rows
        }

        protected virtual void OnStartRowEditing(DevExpress.Web.Data.ASPxStartRowEditingEventArgs e)
        {
            // Derived classes can override
        }

        protected virtual void OnCancelRowEditing(DevExpress.Web.Data.ASPxStartRowEditingEventArgs e)
        {
            // Derived classes can override
        }

        protected virtual void OnItemCreated(object newItem, object parentObject)
        {
            // Derived classes can override to set additional properties
        }

        protected virtual void OnItemUpdated(object item)
        {
            // Derived classes can override to handle post-update logic
        }

        protected virtual bool OnItemDeleting(object item)
        {
            // Derived classes can override to add validation
            return true;
        }

        /// <summary>
        /// Called when user requests to show detail view of an item.
        /// Raises the ShowDetailRequested event.
        /// </summary>
        protected virtual void OnShowDetailRequested(object item)
        {
            ShowDetailRequested?.Invoke(this, new ShowDetailRequestedEventArgs(item));
        }

        // Helper methods for adding columns

        protected void AddHiddenColumn(string fieldName)
        {
            var column = new GridViewDataTextColumn
            {
                FieldName = fieldName,
                Visible = false
            };
            grid.Columns.Add(column);
        }

        protected void AddTextColumn(string fieldName, string caption, int width, bool allowEdit = true)
        {
            var column = new GridViewDataTextColumn
            {
                FieldName = fieldName,
                Caption = caption,
                Width = Unit.Pixel(width),
                Settings = { AllowHeaderFilter = DefaultBoolean.False, AllowSort = DefaultBoolean.True },
                ReadOnly = !allowEdit
            };

            grid.Columns.Add(column);
        }

        protected void AddDecimalColumn(string fieldName, string caption, int width, bool allowEdit = true)
        {
            var column = new GridViewDataSpinEditColumn
            {
                FieldName = fieldName,
                Caption = caption,
                Width = Unit.Pixel(width),
                Settings = { AllowHeaderFilter = DefaultBoolean.False, AllowSort = DefaultBoolean.True },
                ReadOnly = !allowEdit
            };

            column.PropertiesSpinEdit.NumberType = DevExpress.Web.SpinEditNumberType.Float;
            column.PropertiesSpinEdit.DecimalPlaces = 2;

            grid.Columns.Add(column);
        }

        protected void AddDateColumn(string fieldName, string caption, int width, bool allowEdit = true)
        {
            var column = new GridViewDataDateColumn
            {
                FieldName = fieldName,
                Caption = caption,
                Width = Unit.Pixel(width),
                Settings = { AllowHeaderFilter = DefaultBoolean.False, AllowSort = DefaultBoolean.True },
                ReadOnly = !allowEdit
            };

            column.PropertiesDateEdit.DisplayFormatString = "d";

            grid.Columns.Add(column);
        }

        protected void AddIntColumn(string fieldName, string caption, int width, bool allowEdit = true)
        {
            var column = new GridViewDataSpinEditColumn
            {
                FieldName = fieldName,
                Caption = caption,
                Width = Unit.Pixel(width),
                Settings = { AllowHeaderFilter = DefaultBoolean.False, AllowSort = DefaultBoolean.True },
                ReadOnly = !allowEdit
            };

            column.PropertiesSpinEdit.NumberType = DevExpress.Web.SpinEditNumberType.Integer;

            grid.Columns.Add(column);
        }

        protected void AddCheckBoxColumn(string fieldName, string caption, int width, bool allowEdit = true)
        {
            var column = new GridViewDataCheckColumn
            {
                FieldName = fieldName,
                Caption = caption,
                Width = Unit.Pixel(width),
                Settings = { AllowHeaderFilter = DefaultBoolean.False, AllowSort = DefaultBoolean.True },
                ReadOnly = !allowEdit
            };

            grid.Columns.Add(column);
        }

        protected void AddComboBoxColumn(string fieldName, string caption, int width, object dataSource, string valueField, string textField, bool allowEdit = true)
        {
            var column = new GridViewDataComboBoxColumn
            {
                FieldName = fieldName,
                Caption = caption,
                Width = Unit.Pixel(width),
                Settings = { AllowHeaderFilter = DefaultBoolean.False, AllowSort = DefaultBoolean.True },
                ReadOnly = !allowEdit
            };

            column.PropertiesComboBox.DataSource = dataSource;
            column.PropertiesComboBox.ValueField = valueField;
            column.PropertiesComboBox.TextField = textField;

            grid.Columns.Add(column);
        }
    }
}
