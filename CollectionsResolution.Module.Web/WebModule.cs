using System;
using System.Linq;
using System.Text;
using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using System.Collections.Generic;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Model.Core;
using DevExpress.ExpressApp.Model.DomainLogics;
using DevExpress.ExpressApp.Model.NodeGenerators;
using CollectionsResolution.Module.Web.ModelExtensions;

namespace CollectionsResolution.Module.Web {
    [ToolboxItemFilter("Xaf.Platform.Web")]
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppModuleBasetopic.aspx.
    public sealed partial class CollectionsResolutionAspNetModule : ModuleBase {
        public CollectionsResolutionAspNetModule() {
            InitializeComponent();
        }
        public override IEnumerable<ModuleUpdater> GetModuleUpdaters(IObjectSpace objectSpace, Version versionFromDB) {
            return ModuleUpdater.EmptyModuleUpdaters;
        }
        public override void Setup(XafApplication application) {
            base.Setup(application);
            // Manage various aspects of the application UI and behavior at the module level.
            
            // Subscribe to SetupComplete to apply automatic layout generation
            // for DetailViews that use CustomASPxEditableCollectionPropertyEditor
            application.SetupComplete += Application_SetupComplete;
        }

        private void Application_SetupComplete(object sender, EventArgs e)
        {
            try
            {
                var application = sender as XafApplication;
                if (application != null)
                {
                    // Automatically generate layouts for DetailViews with custom collection editors
                    DetailViewLayoutGenerator.ProcessAllDetailViews(application);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in automatic layout generation: {ex.Message}");
                // Don't throw - allow application to continue
            }
        }
    }
}
