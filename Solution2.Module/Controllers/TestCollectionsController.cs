using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using Solution2.Module.NonPersistentBusinessObjects.TestCollections;
using System;
using System.ComponentModel;

namespace Solution2.Module.Controllers
{
    /// <summary>
    /// Controller to manage test non-persistent objects for collection rendering analysis.
    /// </summary>
    public class TestCollectionsController : ObjectViewController<ListView, TestCollectionsNonPersistentDefault>
    {
        public TestCollectionsController()
        {
            TargetViewType = ViewType.ListView;
        }

        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            PopulateTestData();
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            ObjectSpace.ObjectChanged += ObjectSpace_ObjectChanged;
        }

        protected override void OnDeactivated()
        {
            ObjectSpace.ObjectChanged -= ObjectSpace_ObjectChanged;
            base.OnDeactivated();
        }

        private void ObjectSpace_ObjectChanged(object sender, ObjectChangedEventArgs e)
        {
            // Handle object changes if needed
        }

        /// <summary>
        /// Populates the non-persistent object space with test data.
        /// </summary>
        private void PopulateTestData()
        {
            if (ObjectSpace is NonPersistentObjectSpace nonPersistentObjectSpace)
            {
                // Clear existing objects
                var existingObjects = nonPersistentObjectSpace.GetObjects<TestCollectionsNonPersistentDefault>();
                
                // Create sample test objects with collections
                for (int i = 1; i <= 3; i++)
                {
                    var testObject = nonPersistentObjectSpace.CreateObject<TestCollectionsNonPersistentDefault>();
                    testObject.TestName = $"Test Case {i}";
                    testObject.Description = $"Description for test case {i}";
                    testObject.NumberValue = i * 100;
                    testObject.DecimalValue = i * 10.5m;
                    testObject.DateValue = DateTime.Today.AddDays(i);
                    testObject.BoolValue = i % 2 == 0;

                    // Populate first collection
                    for (int j = 1; j <= 3; j++)
                    {
                        var collectionItem = nonPersistentObjectSpace.CreateObject<TestCollectionItem>();
                        collectionItem.Code = $"CODE{i}-{j}";
                        collectionItem.Name = $"Item {i}-{j}";
                        collectionItem.Amount = (i * j) * 10.0m;
                        collectionItem.Date = DateTime.Today.AddDays(j);
                        collectionItem.IsActive = j % 2 == 1;
                        
                        testObject.CollectionItems.Add(collectionItem);
                    }

                    // Populate second collection
                    for (int k = 1; k <= 2; k++)
                    {
                        var detailItem = nonPersistentObjectSpace.CreateObject<TestDetailItem>();
                        detailItem.Description = $"Detail {i}-{k}";
                        detailItem.Quantity = k * 5;
                        detailItem.UnitPrice = (i + k) * 2.5m;
                        detailItem.Category = k == 1 ? "Category A" : "Category B";
                        
                        testObject.DetailItems.Add(detailItem);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Controller to manage test non-persistent objects with custom property editors.
    /// </summary>
    public class TestCollectionsCustomController : ObjectViewController<ListView, TestCollectionsNonPersistentCustom>
    {
        public TestCollectionsCustomController()
        {
            TargetViewType = ViewType.ListView;
        }

        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            PopulateTestData();
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            ObjectSpace.ObjectChanged += ObjectSpace_ObjectChanged;
        }

        protected override void OnDeactivated()
        {
            ObjectSpace.ObjectChanged -= ObjectSpace_ObjectChanged;
            base.OnDeactivated();
        }

        private void ObjectSpace_ObjectChanged(object sender, ObjectChangedEventArgs e)
        {
            // Handle object changes if needed
        }

        /// <summary>
        /// Populates the non-persistent object space with test data.
        /// </summary>
        private void PopulateTestData()
        {
            if (ObjectSpace is NonPersistentObjectSpace nonPersistentObjectSpace)
            {
                // Clear existing objects
                var existingObjects = nonPersistentObjectSpace.GetObjects<TestCollectionsNonPersistentCustom>();

                // Create sample test objects with collections
                for (int i = 1; i <= 3; i++)
                {
                    var testObject = nonPersistentObjectSpace.CreateObject<TestCollectionsNonPersistentCustom>();
                    testObject.TestName = $"Custom Test {i}";
                    testObject.Description = $"Custom editor description {i}";
                    testObject.NumberValue = i * 200;
                    testObject.DecimalValue = i * 20.5m;
                    testObject.DateValue = DateTime.Today.AddDays(i * 2);
                    testObject.BoolValue = i % 2 == 1;

                    // Populate first collection
                    for (int j = 1; j <= 4; j++)
                    {
                        var collectionItem = nonPersistentObjectSpace.CreateObject<TestCollectionItem>();
                        collectionItem.Code = $"CUSTOM{i}-{j}";
                        collectionItem.Name = $"Custom Item {i}-{j}";
                        collectionItem.Amount = (i * j) * 15.0m;
                        collectionItem.Date = DateTime.Today.AddDays(j * 2);
                        collectionItem.IsActive = j % 2 == 0;
                        
                        testObject.CollectionItems.Add(collectionItem);
                    }

                    // Populate second collection
                    for (int k = 1; k <= 3; k++)
                    {
                        var detailItem = nonPersistentObjectSpace.CreateObject<TestDetailItem>();
                        detailItem.Description = $"Custom Detail {i}-{k}";
                        detailItem.Quantity = k * 10;
                        detailItem.UnitPrice = (i + k) * 5.0m;
                        detailItem.Category = $"Custom Category {k}";
                        
                        testObject.DetailItems.Add(detailItem);
                    }
                }
            }
        }
    }
}
