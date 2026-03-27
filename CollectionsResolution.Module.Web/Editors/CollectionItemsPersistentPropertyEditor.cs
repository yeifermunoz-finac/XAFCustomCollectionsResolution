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
    /// Custom Property Editor to display persistent CollectionItem collection in a grid
    /// for testing collection rendering in Web applications.
    /// </summary>
    [PropertyEditor(typeof(XPCollection<CollectionItemPersistentCustom>), "CollectionItemsPersistentPropertyEditor", false)]
    public class CollectionItemsPersistentPropertyEditor : EditableCollectionPropertyEditorBase
    {
        public CollectionItemsPersistentPropertyEditor(Type objectType, IModelMemberViewItem model)
            : base(objectType, model)
        {
        }

        protected override string GetPanelId()
        {
            return "testCollectionItemsPersistentPanel";
        }

        protected override string GetGridId()
        {
            return "testCollectionItemsPersistentGrid";
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
