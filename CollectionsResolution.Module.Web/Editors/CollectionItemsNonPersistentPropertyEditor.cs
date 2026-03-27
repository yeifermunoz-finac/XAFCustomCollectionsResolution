using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using CollectionsResolution.Module.NonPersistentBusinessObjects.CollectionRendering;

namespace CollectionsResolution.Module.Web.Editors
{
    /// <summary>
    /// Custom Property Editor to display CollectionItem collection in a grid
    /// for testing collection rendering in Web applications.
    /// This editor renders an ASPxGridView to display and edit the collection.
    /// Inherits from EditableCollectionPropertyEditorBase to get edit mode detection.
    /// Registered with isDefaultEditor=false to allow view-specific assignment via XAFML.
    /// </summary>
    [PropertyEditor(typeof(BindingList<CollectionItemNonPersistent>), "CollectionItemsNonPersistentPropertyEditor", false)]
    public class CollectionItemsNonPersistentPropertyEditor : EditableCollectionPropertyEditorBase
    {
        public CollectionItemsNonPersistentPropertyEditor(Type objectType, IModelMemberViewItem model)
            : base(objectType, model)
        {
        }

        protected override string GetPanelId()
        {
            return "CollectionItemsNonPersistentPanel";
        }

        protected override string GetGridId()
        {
            return "CollectionItemsNonPersistentGrid";
        }

        protected override void DefineColumns()
        {
            AddTextColumn("Code", "Code", 120, true);
            AddTextColumn("Name", "Name", 200, true);
            AddDecimalColumn("Amount", "Amount", 120, true);
            AddDateColumn("Date", "Date", 120, true);
            AddCheckBoxColumn("IsActive", "Is Active", 100, true);
        }
    }
}
