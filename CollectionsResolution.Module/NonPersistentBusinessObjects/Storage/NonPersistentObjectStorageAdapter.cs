using DevExpress.ExpressApp;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;

namespace CollectionsResolution.Module.NonPersistentBusinessObjects.Storage
{
    /// <summary>
    /// Storage adapter for non-persistent objects.
    /// Required for Web platform to persist objects across HTTP requests.
    /// 
    /// Uses IXafEntityObject as the storage type (common interface for all XAF entities).
    /// Supports NonPersistentLiteObject and NonPersistentBaseObject which provide auto-generated Oid.
    /// Note: NonPersistentEntityObject and NonPersistentObjectImpl are not supported unless manually defines an Oid property.
    /// </summary>
    public class NonPersistentObjectStorageAdapter
    {
        private static readonly ConcurrentDictionary<Guid, IXafEntityObject> _globalStorage = new ConcurrentDictionary<Guid, IXafEntityObject>();
        private readonly NonPersistentObjectSpace _objectSpace;

        public NonPersistentObjectStorageAdapter(NonPersistentObjectSpace objectSpace)
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
            if (typeof(IXafEntityObject).IsAssignableFrom(e.ObjectType))
            {
                var objects = new BindingList<IXafEntityObject>();
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
                if (obj is IXafEntityObject entityObject)
                {
                    // Get Oid using pattern matching for type-safe resolution
                    // Only NonPersistentLiteObject and NonPersistentBaseObject have auto-generated Oid
                    var oid = entityObject switch
                    {
                        NonPersistentLiteObject liteObject => liteObject.Oid,
                        NonPersistentBaseObject baseObject => baseObject.Oid,
                        _ => Guid.Empty
                    };

                    if (oid == Guid.Empty)
                        continue; // Skip objects without valid Oid

                    if (objectSpace.IsDeletedObject(obj))
                    {
                        // Remove from storage
                        _globalStorage.TryRemove(oid, out _);
                    }
                    else
                    {
                        // Add or update in storage
                        _globalStorage.AddOrUpdate(oid, entityObject, (key, existingValue) => entityObject);
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
