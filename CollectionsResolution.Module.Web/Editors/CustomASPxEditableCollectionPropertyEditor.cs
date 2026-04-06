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

    /// <summary>
    /// Event args for item creation lifecycle event
    /// </summary>
    public class ItemCreatedEventArgs : EventArgs
    {
        public object NewItem { get; set; }
        public object ParentObject { get; set; }
        public ItemCreatedEventArgs(object newItem, object parentObject)
        {
            NewItem = newItem;
            ParentObject = parentObject;
        }
    }

    /// <summary>
    /// Event args for item update lifecycle event
    /// </summary>
    public class ItemUpdatedEventArgs : EventArgs
    {
        public object Item { get; set; }
        public ItemUpdatedEventArgs(object item)
        {
            Item = item;
        }
    }

    /// <summary>
    /// Event args for item deletion lifecycle event
    /// </summary>
    public class ItemDeletingEventArgs : System.ComponentModel.CancelEventArgs
    {
        public object Item { get; set; }
        public ItemDeletingEventArgs(object item)
        {
            Item = item;
        }
    }

    /// <summary>
    /// Custom property editor for editable collections in ASP.NET Web applications.
    /// Handles any collection type (persistent XPCollection, non-persistent BindingList, or any IList).
    /// Provides inline editing with full CRUD operations via ASPxGridView.
    /// Automatically generates columns based on collection element type using reflection.
    /// Automatically enables/disables editing based on the parent DetailView's edit mode.
    /// Can be assigned to specific properties via XAFML Model.DesignedDiffs.
    /// </summary>
    /// <remarks>
    /// This single unified property editor handles all collection types uniformly,
    /// eliminating the need to create a new property editor for each collection type.
    /// 
    /// Usage in XAFML:
    /// <![CDATA[
    /// <DetailView Id="YourClass_DetailView">
    ///   <Items>
    ///     <PropertyEditor Id="YourCollectionProperty" 
    ///                     PropertyEditorType="CollectionsResolution.Module.Web.Editors.CustomASPxEditableCollectionPropertyEditor" />
    ///   </Items>
    /// </DetailView>
    /// ]]>
    /// 
    /// Features:
    /// - Full CRUD support (Create, Read, Update, Delete)
    /// - Automatic column generation via reflection
    /// - Edit mode synchronization with parent DetailView
    /// - Details button for opening nested detail views
    /// - Support for persistent and non-persistent collections
    /// - Unique control IDs to avoid conflicts when multiple collections are displayed
    /// 
    /// Extensibility:
    /// - Use events (InitNewRow, ItemCreated, ItemUpdated, ItemDeleting, etc.) to customize behavior without inheritance
    /// - Override virtual methods (DefineColumns, GetKeyFieldName, etc.) for structural customizations
    /// - Subscribe to ShowDetailRequested event via ShowNonPersistentDetailPopupController for custom detail view handling
    /// </remarks>
    [PropertyEditor(typeof(IList), "CustomASPxEditableCollectionPropertyEditor", false)]
    public class CustomASPxEditableCollectionPropertyEditor : ASPxPropertyEditor
    {
        protected ASPxGridView grid;
        protected Panel containerPanel;
        private IList collectionSource;
        private bool? isEditMode = null;
        private GridViewCommandColumn commandColumn;
        private string panelIdCache;
        private string gridIdCache;
        
        // Dictionary to map temporary Oids (from InitNewRow) to real object Oids (after RowInserting)
        // This allows the Details button to work on newly created objects before the parent is saved
        private System.Collections.Generic.Dictionary<Guid, Guid> tempOidMapping = 
            new System.Collections.Generic.Dictionary<Guid, Guid>();

        #region Events

        /// <summary>
        /// Fired when user clicks the "Details" button for an item
        /// </summary>
        public event EventHandler<ShowDetailRequestedEventArgs> ShowDetailRequested;

        /// <summary>
        /// Fired when initializing a new row. Allows setting default values for new items.
        /// </summary>
        public event EventHandler<DevExpress.Web.Data.ASPxDataInitNewRowEventArgs> InitNewRow;

        /// <summary>
        /// Fired when starting to edit a row.
        /// </summary>
        public event EventHandler<DevExpress.Web.Data.ASPxStartRowEditingEventArgs> StartRowEditing;

        /// <summary>
        /// Fired when canceling row editing.
        /// </summary>
        public event EventHandler<DevExpress.Web.Data.ASPxStartRowEditingEventArgs> CancelRowEditing;

        /// <summary>
        /// Fired after a new item has been created and added to the collection.
        /// Allows setting additional properties or performing post-creation logic.
        /// </summary>
        public event EventHandler<ItemCreatedEventArgs> ItemCreated;

        /// <summary>
        /// Fired after an item has been updated.
        /// Allows performing post-update logic or validation.
        /// </summary>
        public event EventHandler<ItemUpdatedEventArgs> ItemUpdated;

        /// <summary>
        /// Fired before an item is deleted. Set Cancel = true to prevent deletion.
        /// </summary>
        public event EventHandler<ItemDeletingEventArgs> ItemDeleting;

        #endregion

        public CustomASPxEditableCollectionPropertyEditor(Type objectType, IModelMemberViewItem model)
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
                EnableCallBacks = false  // Use postbacks for inline editing
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

            // Define columns using automatic generation
            DefineColumns();

            // Add command column for CRUD operations
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

            // Pre-configure grid with expected data type
            if (MemberInfo != null && MemberInfo.ListElementType != null)
            {
                grid.ForceDataRowType(MemberInfo.ListElementType);
            }

            // CRITICAL: Enable data security for inline editing to work
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

            // Only update if state changed
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
                grid.SettingsDataSecurity.AllowInsert = editMode;
                grid.SettingsDataSecurity.AllowEdit = editMode;
                grid.SettingsDataSecurity.AllowDelete = editMode;

                // Make all editable columns read-only in view mode
                foreach (GridViewColumn column in grid.Columns)
                {
                    if (column is GridViewDataColumn dataColumn && column != commandColumn)
                    {
                        dataColumn.ReadOnly = !editMode;
                    }
                }
            }
        }

        private void Grid_InitNewRow(object sender, DevExpress.Web.Data.ASPxDataInitNewRowEventArgs e)
        {
            // CRITICAL: Initialize a temporary Oid for the grid's state tracking
            var tempOid = Guid.NewGuid();
            e.NewValues["Oid"] = tempOid;
            
            InitNewRow?.Invoke(this, e);
        }

        private void Grid_StartRowEditing(object sender, DevExpress.Web.Data.ASPxStartRowEditingEventArgs e)
        {
            StartRowEditing?.Invoke(this, e);
        }

        private void Grid_CancelRowEditing(object sender, DevExpress.Web.Data.ASPxStartRowEditingEventArgs e)
        {
            CancelRowEditing?.Invoke(this, e);
        }

        private void Grid_CustomButtonCallback(object sender, ASPxGridViewCustomButtonCallbackEventArgs e)
        {
            if (e.ButtonID == "btnDetails")
            {
                if (!grid.IsEditing && !grid.IsNewRowEditing)
                {
                    var oidFromGrid = grid.GetRowValues(e.VisibleIndex, "Oid");
                    if (oidFromGrid != null && PropertyValue is IList collection)
                    {
                        Guid oidToFind = Guid.Empty;
                        
                        if (oidFromGrid is Guid gridGuid)
                        {
                            if (tempOidMapping.ContainsKey(gridGuid))
                            {
                                oidToFind = tempOidMapping[gridGuid];
                            }
                            else
                            {
                                oidToFind = gridGuid;
                            }
                        }
                        
                        // Find the object by Oid in the collection
                        object item = null;
                        foreach (var obj in collection)
                        {
                            var oidProp = obj.GetType().GetProperty("Oid");
                            if (oidProp != null)
                            {
                                var itemOid = oidProp.GetValue(obj, null);
                                if (itemOid != null && itemOid is Guid itemGuid && itemGuid == oidToFind)
                                {
                                    item = obj;
                                    break;
                                }
                            }
                        }
                        
                        if (item != null)
                        {
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
                if (PropertyValue is IList collection && MemberInfo != null)
                {
                    var itemType = MemberInfo.ListElementType;
                    
                    // Use ObjectSpace to create the object properly
                    object newItem;
                    if (View != null && View.ObjectSpace != null)
                    {
                        newItem = View.ObjectSpace.CreateObject(itemType);
                    }
                    else
                    {
                        newItem = Activator.CreateInstance(itemType);
                    }
                    
                    // Set properties from grid's new values (skip Oid - it's auto-generated)
                    var tempOid = e.NewValues["Oid"];
                    foreach (var key in e.NewValues.Keys)
                    {
                        if (key.ToString() == "Oid")
                            continue;
                            
                        var propInfo = itemType.GetProperty(key.ToString());
                        if (propInfo != null && propInfo.CanWrite)
                        {
                            var value = e.NewValues[key];
                            if (value != null)
                            {
                                try
                                {
                                    value = ConvertValue(value, propInfo.PropertyType);
                                    propInfo.SetValue(newItem, value, null);
                                }
                                catch
                                {
                                    // If conversion fails, skip this property
                                }
                            }
                        }
                    }
                    
                    ItemCreated?.Invoke(this, new ItemCreatedEventArgs(newItem, CurrentObject));
                    
                    collection.Add(newItem);
                    
                    // Map temp Oid to real Oid for Details button support
                    if (tempOid != null)
                    {
                        var realOidProp = newItem.GetType().GetProperty("Oid");
                        if (realOidProp != null)
                        {
                            var realOid = realOidProp.GetValue(newItem, null);
                            if (realOid != null && tempOid is Guid tempGuid && realOid is Guid realGuid)
                            {
                                tempOidMapping[tempGuid] = realGuid;
                            }
                        }
                    }
                }
                
                e.Cancel = true;
                grid.CancelEdit();
                RefreshGrid();
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                grid.CancelEdit();
                System.Diagnostics.Debug.WriteLine($"Error inserting row: {ex.Message}");
                throw;
            }
        }

        private void Grid_RowUpdating(object sender, DevExpress.Web.Data.ASPxDataUpdatingEventArgs e)
        {
            try
            {
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
                                    if (value != null)
                                    {
                                        value = ConvertValue(value, propInfo.PropertyType);
                                    }
                                    propInfo.SetValue(itemToUpdate, value, null);
                                }
                                catch
                                {
                                    // If conversion fails, skip this property
                                }
                            }
                        }
                        
                        ItemUpdated?.Invoke(this, new ItemUpdatedEventArgs(itemToUpdate));
                    }
                }
                
                e.Cancel = true;
                grid.CancelEdit();
                RefreshGrid();
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                grid.CancelEdit();
                System.Diagnostics.Debug.WriteLine($"Error updating row: {ex.Message}");
                throw;
            }
        }

        private void Grid_RowDeleting(object sender, DevExpress.Web.Data.ASPxDataDeletingEventArgs e)
        {
            try
            {
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
                    
                    var deletingArgs = new ItemDeletingEventArgs(itemToDelete);
                    ItemDeleting?.Invoke(this, deletingArgs);
                    
                    if (itemToDelete != null && !deletingArgs.Cancel)
                    {
                        collection.Remove(itemToDelete);
                        
                        // For persistent objects, explicitly delete them using ObjectSpace
                        if (View != null && View.ObjectSpace != null)
                        {
                            View.ObjectSpace.Delete(itemToDelete);
                        }
                    }
                }
                
                e.Cancel = true;
                RefreshGrid();
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
                    
                    // For empty collections, inform the grid about the element type
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

        // Virtual Methods for Extensibility (methods with actual implementation logic)

        /// <summary>
        /// Gets the unique panel ID for this property editor instance.
        /// Override to customize the panel ID generation.
        /// </summary>
        protected virtual string GetPanelId()
        {
            if (panelIdCache == null)
            {
                string propertyName = Model?.PropertyName ?? "Collection";
                panelIdCache = $"CustomASPxEditableCollection_{propertyName}_Panel";
            }
            return panelIdCache;
        }

        /// <summary>
        /// Gets the unique grid ID for this property editor instance.
        /// Override to customize the grid ID generation.
        /// </summary>
        protected virtual string GetGridId()
        {
            if (gridIdCache == null)
            {
                string propertyName = Model?.PropertyName ?? "Collection";
                gridIdCache = $"CustomASPxEditableCollection_{propertyName}_Grid";
            }
            return gridIdCache;
        }

        /// <summary>
        /// Defines the columns to display in the grid.
        /// Default implementation uses automatic column generation via reflection.
        /// Override to manually define columns or customize column generation logic.
        /// </summary>
        protected virtual void DefineColumns()
        {
            // Use automatic column generation based on reflection
            if (MemberInfo != null && MemberInfo.ListElementType != null)
            {
                var columnDefs = GridColumnBuilder.GetColumnDefinitions(MemberInfo.ListElementType);
                
                foreach (var colDef in columnDefs)
                {
                    AddColumnByDefinition(colDef);
                }
            }
        }
        
        /// <summary>
        /// Gets the key field name for the grid. Default is "Oid".
        /// Override if your objects use a different key field.
        /// </summary>
        protected virtual string GetKeyFieldName()
        {
            return "Oid";
        }

        /// <summary>
        /// Called when user requests to show detail view of an item.
        /// Default implementation raises the ShowDetailRequested event.
        /// Override to customize the detail view display behavior.
        /// </summary>
        protected virtual void OnShowDetailRequested(object item)
        {
            ShowDetailRequested?.Invoke(this, new ShowDetailRequestedEventArgs(item));
        }

        // Private Helper Methods

        /// <summary>
        /// Adds a column to the grid based on a column definition.
        /// </summary>
        private void AddColumnByDefinition(ColumnDefinition colDef)
        {
            switch (colDef.ColumnType)
            {
                case ColumnType.Text:
                    AddTextColumn(colDef.FieldName, colDef.Caption, colDef.Width, colDef.AllowEdit, colDef.Visible);
                    break;
                case ColumnType.Integer:
                    AddIntColumn(colDef.FieldName, colDef.Caption, colDef.Width, colDef.AllowEdit, colDef.Visible);
                    break;
                case ColumnType.Decimal:
                    AddDecimalColumn(colDef.FieldName, colDef.Caption, colDef.Width, colDef.AllowEdit, colDef.Visible, colDef.DecimalPlaces);
                    break;
                case ColumnType.Date:
                    AddDateColumn(colDef.FieldName, colDef.Caption, colDef.Width, colDef.AllowEdit, colDef.Visible);
                    break;
                case ColumnType.CheckBox:
                    AddCheckBoxColumn(colDef.FieldName, colDef.Caption, colDef.Width, colDef.AllowEdit, colDef.Visible);
                    break;
                case ColumnType.Enum:
                    AddEnumColumn(colDef.FieldName, colDef.Caption, colDef.Width, colDef.PropertyType, colDef.AllowEdit, colDef.Visible);
                    break;
                default:
                    AddTextColumn(colDef.FieldName, colDef.Caption, colDef.Width, colDef.AllowEdit, colDef.Visible);
                    break;
            }
        }

        /// <summary>
        /// Converts a value to the target type with support for common type conversions.
        /// </summary>
        private object ConvertValue(object value, Type targetType)
        {
            if (value.GetType() == targetType)
                return value;

            if (targetType.IsEnum)
            {
                if (int.TryParse(value.ToString(), out int enumIntValue))
                    return Enum.ToObject(targetType, enumIntValue);
                else
                    return Enum.Parse(targetType, value.ToString());
            }
            else if (targetType == typeof(bool))
                return Convert.ToBoolean(value);
            else if (targetType == typeof(int))
                return Convert.ToInt32(value);
            else if (targetType == typeof(decimal))
                return Convert.ToDecimal(value);
            else if (targetType == typeof(DateTime))
                return Convert.ToDateTime(value);
            else
                return Convert.ChangeType(value, targetType);
        }

        // Column Helper Methods

        protected void AddHiddenColumn(string fieldName)
        {
            var column = new GridViewDataTextColumn
            {
                FieldName = fieldName,
                Visible = false
            };
            grid.Columns.Add(column);
        }

        protected void AddTextColumn(string fieldName, string caption, int width, bool allowEdit = true, bool visible = true)
        {
            var column = new GridViewDataTextColumn
            {
                FieldName = fieldName,
                Caption = caption,
                Width = Unit.Pixel(width),
                Settings = { AllowHeaderFilter = DefaultBoolean.False, AllowSort = DefaultBoolean.True },
                ReadOnly = !allowEdit,
                Visible = visible
            };

            grid.Columns.Add(column);
        }

        protected void AddDecimalColumn(string fieldName, string caption, int width, bool allowEdit = true, bool visible = true, int decimalPlaces = 2)
        {
            var column = new GridViewDataSpinEditColumn
            {
                FieldName = fieldName,
                Caption = caption,
                Width = Unit.Pixel(width),
                Settings = { AllowHeaderFilter = DefaultBoolean.False, AllowSort = DefaultBoolean.True },
                ReadOnly = !allowEdit,
                Visible = visible
            };

            column.PropertiesSpinEdit.NumberType = DevExpress.Web.SpinEditNumberType.Float;
            column.PropertiesSpinEdit.DecimalPlaces = decimalPlaces;

            grid.Columns.Add(column);
        }

        protected void AddDateColumn(string fieldName, string caption, int width, bool allowEdit = true, bool visible = true)
        {
            var column = new GridViewDataDateColumn
            {
                FieldName = fieldName,
                Caption = caption,
                Width = Unit.Pixel(width),
                Settings = { AllowHeaderFilter = DefaultBoolean.False, AllowSort = DefaultBoolean.True },
                ReadOnly = !allowEdit,
                Visible = visible
            };

            column.PropertiesDateEdit.DisplayFormatString = "d";

            grid.Columns.Add(column);
        }

        protected void AddIntColumn(string fieldName, string caption, int width, bool allowEdit = true, bool visible = true)
        {
            var column = new GridViewDataSpinEditColumn
            {
                FieldName = fieldName,
                Caption = caption,
                Width = Unit.Pixel(width),
                Settings = { AllowHeaderFilter = DefaultBoolean.False, AllowSort = DefaultBoolean.True },
                ReadOnly = !allowEdit,
                Visible = visible
            };

            column.PropertiesSpinEdit.NumberType = DevExpress.Web.SpinEditNumberType.Integer;

            grid.Columns.Add(column);
        }

        protected void AddCheckBoxColumn(string fieldName, string caption, int width, bool allowEdit = true, bool visible = true)
        {
            var column = new GridViewDataCheckColumn
            {
                FieldName = fieldName,
                Caption = caption,
                Width = Unit.Pixel(width),
                Settings = { AllowHeaderFilter = DefaultBoolean.False, AllowSort = DefaultBoolean.True },
                ReadOnly = !allowEdit,
                Visible = visible
            };

            grid.Columns.Add(column);
        }

        protected void AddComboBoxColumn(string fieldName, string caption, int width, object dataSource, string valueField, string textField, bool allowEdit = true, bool visible = true)
        {
            var column = new GridViewDataComboBoxColumn
            {
                FieldName = fieldName,
                Caption = caption,
                Width = Unit.Pixel(width),
                Settings = { AllowHeaderFilter = DefaultBoolean.False, AllowSort = DefaultBoolean.True },
                ReadOnly = !allowEdit,
                Visible = visible
            };

            column.PropertiesComboBox.DataSource = dataSource;
            column.PropertiesComboBox.ValueField = valueField;
            column.PropertiesComboBox.TextField = textField;

            grid.Columns.Add(column);
        }

        protected void AddEnumColumn(string fieldName, string caption, int width, Type enumType, bool allowEdit = true, bool visible = true)
        {
            var column = new GridViewDataComboBoxColumn
            {
                FieldName = fieldName,
                Caption = caption,
                Width = Unit.Pixel(width),
                Settings = { AllowHeaderFilter = DefaultBoolean.False, AllowSort = DefaultBoolean.True },
                ReadOnly = !allowEdit,
                Visible = visible
            };

            // Populate with enum values
            if (enumType != null && enumType.IsEnum)
            {
                var enumValues = Enum.GetValues(enumType);
                var enumList = new System.Collections.Generic.List<object>();
                
                foreach (var enumValue in enumValues)
                {
                    enumList.Add(new { Value = enumValue, Text = enumValue.ToString() });
                }
                
                column.PropertiesComboBox.DataSource = enumList;
                column.PropertiesComboBox.ValueField = "Value";
                column.PropertiesComboBox.TextField = "Text";
            }

            grid.Columns.Add(column);
        }
    }
}
