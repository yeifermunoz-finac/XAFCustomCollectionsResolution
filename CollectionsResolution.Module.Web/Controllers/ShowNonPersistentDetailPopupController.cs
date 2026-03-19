using System;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.SystemModule;
using CollectionsResolution.Module.Web.Editors;

namespace CollectionsResolution.Module.Web.Controllers
{
    /// <summary>
    /// Controller that handles showing detail view popups when user clicks "Details" button
    /// in custom collection property editors.
    /// Supports recursive/nested popups for sub-collections.
    /// </summary>
    public class ShowNonPersistentDetailPopupController : ViewController<DetailView>
    {
        protected override void OnActivated()
        {
            base.OnActivated();
            
            // Subscribe to ShowDetailRequested event for all property editors in the view
            foreach (PropertyEditor propertyEditor in View.GetItems<PropertyEditor>())
            {
                if (propertyEditor is EditableCollectionPropertyEditorBase editableEditor)
                {
                    editableEditor.ShowDetailRequested += EditableEditor_ShowDetailRequested;
                }
            }
        }

        protected override void OnDeactivated()
        {
            // Unsubscribe from events to prevent memory leaks
            foreach (PropertyEditor propertyEditor in View.GetItems<PropertyEditor>())
            {
                if (propertyEditor is EditableCollectionPropertyEditorBase editableEditor)
                {
                    editableEditor.ShowDetailRequested -= EditableEditor_ShowDetailRequested;
                }
            }
            
            base.OnDeactivated();
        }

        private void EditableEditor_ShowDetailRequested(object sender, ShowDetailRequestedEventArgs e)
        {
            if (e.Item == null)
                return;

            try
            {
                // Create a NonPersistentObjectSpace for the popup
                // This ensures proper object tracking and allows nested editing
                var objectSpace = Application.CreateObjectSpace(e.Item.GetType());
                
                // Get the object in the context of this object space
                // For non-persistent objects, this may return the same object or a tracked version
                var objectToShow = objectSpace.GetObject(e.Item);
                
                if (objectToShow != null)
                {
                    DetailView detailView;
                    
                    // Determine if a custom detail view exists for this type in the application model
                    string viewId = objectToShow.GetType().Name + "_Custom_DetailView";
                    var modelView = Application.FindModelView(viewId);
                    
                    if (modelView != null)
                    {
                        // Create the specific custom DetailView
                        detailView = Application.CreateDetailView(objectSpace, viewId, true, objectToShow);
                    }
                    else
                    {
                        // Create the default DetailView for the selected object
                        // isRoot = true allows Save/Edit actions to appear
                        detailView = Application.CreateDetailView(objectSpace, objectToShow, true);
                    }
                    
                    // Start in View mode by default
                    detailView.ViewEditMode = ViewEditMode.View;
                    
                    // Show the detail view in a popup window
                    var svp = new ShowViewParameters(detailView)
                    {
                        Context = TemplateContext.PopupWindow,
                        TargetWindow = TargetWindow.NewModalWindow
                    };
                    
                    var dialogController = Application.CreateController<DialogController>();
                    dialogController.SaveOnAccept = false;
                    
                    // We want Edit and Close instead of Ok and Cancel.
                    dialogController.CancelAction.Caption = "Close";
                    
                    // The Accept action will switch to Edit mode, or Save if already in Edit mode.
                    dialogController.AcceptAction.Caption = "Edit";
                    
                    dialogController.Accepting += (s, args) => 
                    {
                        if (detailView.ViewEditMode == ViewEditMode.View)
                        {
                            // Switch to Edit mode
                            detailView.ViewEditMode = ViewEditMode.Edit;
                            detailView.BreakLinksToControls(); // Force recreation of ASP.NET controls in Web
                            detailView.CreateControls();
                            dialogController.AcceptAction.Caption = "Save";
                            
                            // Prevent the dialog from closing
                            args.Cancel = true;
                        }
                        else
                        {
                            // In Edit mode, this acts as Save
                            if (!objectSpace.IsDisposed)
                            {
                                try
                                {
                                    objectSpace.CommitChanges();
                                    
                                    // Mark the parent object as modified so XAF knows it needs saving
                                    View.ObjectSpace.SetModified(View.CurrentObject);
                                    
                                    if (sender is PropertyEditor propertyEditor)
                                    {
                                        propertyEditor.Refresh();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Error committing changes in popup: {ex.Message}");
                                    throw;
                                }
                            }
                        }
                    };
                    
                    // Ensure state is preserved when closing if any inline changes occurred
                    dialogController.Cancelling += (s, args) => 
                    {
                        if (!objectSpace.IsDisposed && objectSpace.IsModified)
                        {
                            try
                            {
                                objectSpace.CommitChanges();
                                View.ObjectSpace.Refresh();
                                if (sender is PropertyEditor propertyEditor)
                                {
                                    propertyEditor.Refresh();
                                }
                            }
                            catch (Exception) { /* ignore */ }
                        }
                    };

                    svp.Controllers.Add(dialogController);
                    
                    #pragma warning disable XAF0022
                    Application.ShowViewStrategy.ShowView(svp, new ShowViewSource(Frame, null));
                    #pragma warning restore XAF0022
                }
            }
            catch (Exception ex)
            {
                // Log any errors that occur during popup creation
                System.Diagnostics.Debug.WriteLine($"Error showing detail popup: {ex.Message}");
                // Re-throw to let XAF's error handling show the error to the user
                throw;
            }
        }
    }
}
