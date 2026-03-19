using System;
using System.Collections;
using System.Linq;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;

namespace CollectionsResolution.Module.Web.Editors
{
    /// <summary>
    /// Custom Property Editor to display SubCollectionItem collection in a grid
    /// for testing nested collection rendering in Web applications.
    /// This editor renders an ASPxGridView to display and edit the sub-collection.
    /// Inherits from EditableCollectionPropertyEditorBase to get edit mode detection.
    /// </summary>
    [PropertyEditor(typeof(IList), "SubCollectionItemsPropertyEditor", false)]
    public class SubCollectionItemsPropertyEditor : EditableCollectionPropertyEditorBase
    {
        public SubCollectionItemsPropertyEditor(Type objectType, IModelMemberViewItem model)
            : base(objectType, model)
        {
        }

        protected override string GetPanelId()
        {
            return "testSubCollectionItemsPanel";
        }

        protected override string GetGridId()
        {
            return "testSubCollectionItemsGrid";
        }

        protected override void DefineColumns()
        {
            AddTextColumn("Identifier", "Identifier", 200, true);
            AddIntColumn("SequenceNumber", "Sequence Number", 150, true);
            AddCheckBoxColumn("IsEnabled", "Is Enabled", 120, true);
        }
    }
}
