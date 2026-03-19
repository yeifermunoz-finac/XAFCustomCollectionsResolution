using DevExpress.ExpressApp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Solution2.Module.NonPersistentBusinessObjects.TestCollections
{
    /// <summary>
    /// Minimal storage adapter for non-persistent objects.
    /// Required for Web platform to persist objects across HTTP requests.
    /// </summary>
    public class TestCollectionsStorageAdapter
    {
        private static readonly ConcurrentDictionary<Guid, object> _globalStorage = new ConcurrentDictionary<Guid, object>();
        private readonly NonPersistentObjectSpace _objectSpace;

        public TestCollectionsStorageAdapter(NonPersistentObjectSpace objectSpace)
        {
            _objectSpace = objectSpace;
            
            // Subscribe to essential events
            _objectSpace.ObjectsGetting += ObjectSpace_ObjectsGetting;
            _objectSpace.Committing += ObjectSpace_Committing;
            _objectSpace.Disposed += ObjectSpace_Disposed;
        }

        private void ObjectSpace_ObjectsGetting(object sender, ObjectsGettingEventArgs e)
        {
            // Return stored objects of the requested type
            if (typeof(NonPersistentLiteObject).IsAssignableFrom(e.ObjectType))
            {
                var objects = new BindingList<object>();
                objects.AllowNew = true;
                objects.AllowEdit = true;
                objects.AllowRemove = true;

                foreach (var storedObject in _globalStorage.Values)
                {
                    if (e.ObjectType.IsAssignableFrom(storedObject.GetType()))
                    {
                        objects.Add(storedObject);
                    }
                }

                e.Objects = objects;
            }
        }

        private void ObjectSpace_Committing(object sender, CancelEventArgs e)
        {
            // Save all modified objects to global storage
            var objectSpace = (NonPersistentObjectSpace)sender;

            foreach (var obj in objectSpace.ModifiedObjects)
            {
                if (obj is NonPersistentLiteObject nonPersistent)
                {
                    if (objectSpace.IsDeletedObject(obj))
                    {
                        // Remove from storage
                        _globalStorage.TryRemove(nonPersistent.Oid, out _);
                    }
                    else
                    {
                        // Add or update in storage
                        _globalStorage.AddOrUpdate(nonPersistent.Oid, obj, (key, existingValue) => obj);
                    }
                }
            }
        }

        private void ObjectSpace_Disposed(object sender, EventArgs e)
        {
            // Unsubscribe from events
            _objectSpace.ObjectsGetting -= ObjectSpace_ObjectsGetting;
            _objectSpace.Committing -= ObjectSpace_Committing;
            _objectSpace.Disposed -= ObjectSpace_Disposed;
        }
    }
}
