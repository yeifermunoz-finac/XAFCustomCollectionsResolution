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
    /// Custom Property Editor to display SubCollectionItem collection in a grid
    /// for testing nested collection rendering in Web applications.
    /// This editor renders an ASPxGridView to display and edit the sub-collection.
    /// Inherits from EditableCollectionPropertyEditorBase to get edit mode detection.
    /// Registered with isDefaultEditor=false to allow view-specific assignment via XAFML.
    /// </summary>
    [PropertyEditor(typeof(BindingList<SubCollectionItemNonPersistent>), "SubCollectionItemsNonPersistentPropertyEditor", false)]
    public class SubCollectionItemsNonPersistentPropertyEditor : EditableCollectionPropertyEditorBase
    {
        public SubCollectionItemsNonPersistentPropertyEditor(Type objectType, IModelMemberViewItem model)
            : base(objectType, model)
        {
        }

        protected override string GetPanelId()
        {
            return "SubCollectionItemsNonPersistentPanel";
        }

        protected override string GetGridId()
        {
            return "SubCollectionItemsNonPersistentGrid";
        }

        protected override void DefineColumns()
        {
            AddTextColumn("Identifier", "Identifier", 200, true);
            AddIntColumn("SequenceNumber", "Sequence Number", 150, true);
            AddCheckBoxColumn("IsEnabled", "Is Enabled", 120, true);
        }
    }
}
