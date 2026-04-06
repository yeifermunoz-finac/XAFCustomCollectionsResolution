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
                if (propertyEditor is CustomASPxEditableCollectionPropertyEditor editableEditor)
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
                if (propertyEditor is CustomASPxEditableCollectionPropertyEditor editableEditor)
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
                // Determine the appropriate ObjectSpace type based on the object
                IObjectSpace objectSpace;
                object objectToShow;
                bool isPersistent = false;
                bool useNestedObjectSpace = false;
                
                // Check if this is a persistent object (has been saved) or non-persistent
                var sessionProp = e.Item.GetType().GetProperty("Session");
                if (sessionProp != null)
                {
                    var session = sessionProp.GetValue(e.Item, null);
                    isPersistent = session != null;
                }
                
                if (isPersistent)
                {
                    // For persistent objects, create a NESTED ObjectSpace
                    // This allows us to:
                    // 1. Make changes independently from the parent
                    // 2. Commit to sync back to parent, or rollback to discard
                    // 3. Create a root view (needed for proper Edit/View mode switching)
                    objectSpace = Application.CreateNestedObjectSpace(View.ObjectSpace);
                    useNestedObjectSpace = true;
                    objectToShow = objectSpace.GetObject(e.Item);
                }
                else
                {
                    // For non-persistent objects, create a new NonPersistentObjectSpace
                    objectSpace = Application.CreateObjectSpace(e.Item.GetType());
                    
                    // For non-persistent objects, GetObject may return the same instance
                    // or a tracked version depending on the ObjectSpace implementation
                    objectToShow = objectSpace.GetObject(e.Item);
                    
                    // If GetObject returns null (object not tracked), use the original object
                    if (objectToShow == null)
                    {
                        objectToShow = e.Item;
                    }
                }
                
                if (objectToShow != null)
                {
                    DetailView detailView;
                    
                    // Determine if a custom detail view exists for this type in the application model
                    string viewId = objectToShow.GetType().Name + "_Custom_DetailView";
                    var modelView = Application.FindModelView(viewId);
                    
                    // Now we can always create root views since each has its own ObjectSpace
                    // (nested ObjectSpace for persistent, new ObjectSpace for non-persistent)
                    if (modelView != null)
                    {
                        // Create the specific custom DetailView as root view
                        detailView = Application.CreateDetailView(objectSpace, viewId, true, objectToShow);
                    }
                    else
                    {
                        // Create the default DetailView for the selected object as root view
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
                    
                    // Track whether user explicitly entered Edit mode and made changes
                    bool userEnteredEditMode = false;
                    bool changesSavedExplicitly = false;
                    
                    dialogController.Accepting += (s, args) => 
                    {
                        if (detailView.ViewEditMode == ViewEditMode.View)
                        {
                            // Switch to Edit mode
                            detailView.ViewEditMode = ViewEditMode.Edit;
                            userEnteredEditMode = true;  // Track that user entered edit mode
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
                                    // Commit the nested/separate ObjectSpace
                                    // For nested ObjectSpace (persistent): commits changes to parent ObjectSpace
                                    // For separate ObjectSpace (non-persistent): commits changes to memory
                                    objectSpace.CommitChanges();
                                    changesSavedExplicitly = true;  // Mark that changes were saved
                                    
                                    // Mark the parent object as modified so XAF knows it needs saving
                                    View.ObjectSpace.SetModified(View.CurrentObject);
                                    
                                    // Refresh the property editor to show updated data
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
                    
                    // Handle closing the popup
                    dialogController.Cancelling += (s, args) => 
                    {
                        // CRITICAL LOGIC for nested editing scenarios:
                        // When user clicks "Close", we need to distinguish between:
                        // 
                        // Case 1: User entered Edit mode directly and made unsaved changes
                        //         → Should ROLLBACK (discard their changes)
                        // 
                        // Case 2: User stayed in View mode, but nested child modals saved changes
                        //         → Should COMMIT (preserve nested changes up to parent)
                        // 
                        // Case 3: User entered Edit mode and explicitly saved changes
                        //         → Already committed, do nothing (changes already in parent)
                        // 
                        // Case 4: No changes at all
                        //         → Do nothing
                        
                        if (!objectSpace.IsDisposed)
                        {
                            try
                            {
                                if (objectSpace.IsModified)
                                {
                                    // Determine whether to commit or rollback based on the scenario
                                    bool shouldCommit = false;
                                    
                                    if (changesSavedExplicitly)
                                    {
                                        // Case 3: Already saved, nothing to do
                                        shouldCommit = false; // Already committed
                                    }
                                    else if (userEnteredEditMode && detailView.ViewEditMode == ViewEditMode.Edit)
                                    {
                                        // Case 1: User entered Edit mode but didn't save → Rollback
                                        shouldCommit = false;
                                    }
                                    else if (!userEnteredEditMode && detailView.ViewEditMode == ViewEditMode.View)
                                    {
                                        // Case 2: Still in View mode, changes came from nested modals → Commit
                                        shouldCommit = true;
                                    }
                                    
                                    if (shouldCommit)
                                    {
                                        // Commit nested changes to parent ObjectSpace
                                        objectSpace.CommitChanges();
                                        
                                        // Mark parent as modified
                                        View.ObjectSpace.SetModified(View.CurrentObject);
                                    }
                                    else if (!changesSavedExplicitly)
                                    {
                                        // Rollback unsaved direct edits
                                        objectSpace.Rollback();
                                    }
                                }
                                
                                // Refresh the parent view to ensure UI is in sync
                                if (sender is PropertyEditor propertyEditor)
                                {
                                    propertyEditor.Refresh();
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error during popup close: {ex.Message}");
                                // Don't throw - let the popup close
                            }
                            finally
                            {
                                // Dispose the nested/separate ObjectSpace to free resources
                                // IMPORTANT: Only dispose if it's NOT the parent's ObjectSpace
                                if (useNestedObjectSpace || !isPersistent)
                                {
                                    objectSpace.Dispose();
                                }
                            }
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
