using System;
using System.Collections;
using System.Linq;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;

namespace CollectionsResolution.Module.Web.Editors
{
    /// <summary>
    /// Custom Property Editor to display CollectionItem collection in a grid
    /// for testing collection rendering in Web applications.
    /// This editor renders an ASPxGridView to display and edit the collection.
    /// Inherits from EditableCollectionPropertyEditorBase to get edit mode detection.
    /// </summary>
    [PropertyEditor(typeof(IList), "CollectionItemsPropertyEditor", false)]
    public class CollectionItemsPropertyEditor : EditableCollectionPropertyEditorBase
    {
        public CollectionItemsPropertyEditor(Type objectType, IModelMemberViewItem model)
            : base(objectType, model)
        {
        }

        protected override string GetPanelId()
        {
            return "testCollectionItemsPanel";
        }

        protected override string GetGridId()
        {
            return "testCollectionItemsGrid";
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
