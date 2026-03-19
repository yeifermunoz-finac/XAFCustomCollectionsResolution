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
    /// Custom Property Editor to display DetailItem collection in a grid
    /// for testing collection rendering in Web applications.
    /// Inherits from EditableCollectionPropertyEditorBase to get edit mode detection.
    /// Registered with isDefaultEditor=false to allow view-specific assignment via XAFML.
    /// </summary>
    [PropertyEditor(typeof(BindingList<DetailItemNonPersistent>), "DetailItemsPropertyEditor", false)]
    public class DetailItemsPropertyEditor : EditableCollectionPropertyEditorBase
    {
        public DetailItemsPropertyEditor(Type objectType, IModelMemberViewItem model)
            : base(objectType, model)
        {
        }

        protected override string GetPanelId()
        {
            return "testDetailItemsPanel";
        }

        protected override string GetGridId()
        {
            return "testDetailItemsGrid";
        }

        protected override void DefineColumns()
        {
            AddTextColumn("Description", "Description", 200, true);
            AddIntColumn("Quantity", "Quantity", 100, true);
            AddDecimalColumn("UnitPrice", "Unit Price", 120, true);
            AddTextColumn("Category", "Category", 150, true);
            AddDecimalColumn("Total", "Total", 120, false);
        }
    }
}
