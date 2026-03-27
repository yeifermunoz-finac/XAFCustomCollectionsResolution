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
    /// Custom Property Editor to display persistent SubCollectionItem collection in a grid
    /// for testing nested collection rendering in Web applications.
    /// </summary>
    [PropertyEditor(typeof(XPCollection<SubCollectionItemPersistentCustom>), "SubCollectionItemsPersistentPropertyEditor", false)]
    public class SubCollectionItemsPersistentPropertyEditor : EditableCollectionPropertyEditorBase
    {
        public SubCollectionItemsPersistentPropertyEditor(Type objectType, IModelMemberViewItem model)
            : base(objectType, model)
        {
        }

        protected override string GetPanelId()
        {
            return "SubCollectionItemsPersistentPanel";
        }

        protected override string GetGridId()
        {
            return "SubCollectionItemsPersistentGrid";
        }

        protected override void DefineColumns()
        {
            AddTextColumn("Identifier", "Identifier", 200, true);
            AddIntColumn("SequenceNumber", "Sequence Number", 150, true);
            AddCheckBoxColumn("IsEnabled", "Is Enabled", 120, true);
        }
    }
}
