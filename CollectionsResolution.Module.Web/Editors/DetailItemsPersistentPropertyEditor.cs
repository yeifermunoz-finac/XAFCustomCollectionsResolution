using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using CollectionsResolution.Module.BusinessObjects.CollectionRendering;
using DevExpress.Xpo;

namespace CollectionsResolution.Module.Web.Editors
{
    /// <summary>
    /// Custom Property Editor to display persistent DetailItem collection in a grid
    /// for testing collection rendering in Web applications.
    /// </summary>
    [PropertyEditor(typeof(XPCollection<DetailItemPersistentCustom>), "DetailItemsPersistentPropertyEditor", false)]
    public class DetailItemsPersistentPropertyEditor : EditableCollectionPropertyEditorBase
    {
        public DetailItemsPersistentPropertyEditor(Type objectType, IModelMemberViewItem model)
            : base(objectType, model)
        {
        }

        protected override string GetPanelId()
        {
            return "DetailItemsPersistentPanel";
        }

        protected override string GetGridId()
        {
            return "DetailItemsPersistentGrid";
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
