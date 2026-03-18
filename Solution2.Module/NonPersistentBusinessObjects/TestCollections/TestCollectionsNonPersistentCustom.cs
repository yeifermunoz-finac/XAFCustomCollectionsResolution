using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Solution2.Module.NonPersistentBusinessObjects.TestCollections
{
    /// <summary>
    /// Non-persistent test object to analyze collection rendering with custom property editors in XAF Web.
    /// This object uses custom property editors for collections.
    /// </summary>
    [DomainComponent]
    public class TestCollectionsNonPersistentCustom : NonPersistentBaseObject
    {
        private string _testName;
        private string _description;
        private int _numberValue;
        private decimal _decimalValue;
        private DateTime _dateValue;
        private bool _boolValue;
        private BindingList<TestCollectionItem> _collectionItems;
        private BindingList<TestDetailItem> _detailItems;

        public TestCollectionsNonPersistentCustom()
        {
            _collectionItems = new BindingList<TestCollectionItem>();
            _detailItems = new BindingList<TestDetailItem>();
            _dateValue = DateTime.Today;
        }

        /// <summary>
        /// Gets or sets the test name.
        /// </summary>
        public string TestName
        {
            get => _testName;
            set => SetPropertyValue(ref _testName, value);
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description
        {
            get => _description;
            set => SetPropertyValue(ref _description, value);
        }

        /// <summary>
        /// Gets or sets a numeric integer value.
        /// </summary>
        public int NumberValue
        {
            get => _numberValue;
            set => SetPropertyValue(ref _numberValue, value);
        }

        /// <summary>
        /// Gets or sets a decimal value.
        /// </summary>
        public decimal DecimalValue
        {
            get => _decimalValue;
            set => SetPropertyValue(ref _decimalValue, value);
        }

        /// <summary>
        /// Gets or sets a date value.
        /// </summary>
        public DateTime DateValue
        {
            get => _dateValue;
            set => SetPropertyValue(ref _dateValue, value);
        }

        /// <summary>
        /// Gets or sets a boolean value.
        /// </summary>
        public bool BoolValue
        {
            get => _boolValue;
            set => SetPropertyValue(ref _boolValue, value);
        }

        /// <summary>
        /// Gets or sets the first collection of items (will use custom editor).
        /// </summary>
        [DevExpress.Xpo.Aggregated]
        public BindingList<TestCollectionItem> CollectionItems
        {
            get => _collectionItems;
            set => SetPropertyValue(ref _collectionItems, value);
        }

        /// <summary>
        /// Gets or sets the second collection of detail items (will use custom editor).
        /// </summary>
        [DevExpress.Xpo.Aggregated]
        public BindingList<TestDetailItem> DetailItems
        {
            get => _detailItems;
            set => SetPropertyValue(ref _detailItems, value);
        }

        // INotifyPropertyChanged implementation is inherited from NonPersistentBaseObject
    }
}
