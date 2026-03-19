using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using System;
using System.ComponentModel;

namespace CollectionsResolution.Module.NonPersistentBusinessObjects.CollectionRendering
{
    /// <summary>
    /// Non-persistent object to verify collection rendering with default editors in XAF.
    /// This object contains primitive types and two collections to test default rendering.
    /// </summary>
    [DomainComponent]
    public class CollectionRenderingNonPersistentDefault : NonPersistentLiteObject
    {
        private string _testName;
        private string _description;
        private int _numberValue;
        private decimal _decimalValue;
        private DateTime _dateValue;
        private bool _boolValue;
        private BindingList<CollectionItemNonPersistent> _collectionItems;
        private BindingList<DetailItemNonPersistent> _detailItems;

        public CollectionRenderingNonPersistentDefault()
        {
            _collectionItems = new BindingList<CollectionItemNonPersistent>();
            _detailItems = new BindingList<DetailItemNonPersistent>();
            _dateValue = DateTime.Today;
        }

        /// <summary>
        /// Gets or sets the test name.
        /// </summary>
        public string TestName
        {
            get => _testName;
            set => SetPropertyValue(ref _testName, value, nameof(TestName));
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description
        {
            get => _description;
            set => SetPropertyValue(ref _description, value, nameof(Description));
        }

        /// <summary>
        /// Gets or sets a numeric integer value.
        /// </summary>
        public int NumberValue
        {
            get => _numberValue;
            set => SetPropertyValue(ref _numberValue, value, nameof(NumberValue));
        }

        /// <summary>
        /// Gets or sets a decimal value.
        /// </summary>
        public decimal DecimalValue
        {
            get => _decimalValue;
            set => SetPropertyValue(ref _decimalValue, value, nameof(DecimalValue));
        }

        /// <summary>
        /// Gets or sets a date value.
        /// </summary>
        public DateTime DateValue
        {
            get => _dateValue;
            set => SetPropertyValue(ref _dateValue, value, nameof(DateValue));
        }

        /// <summary>
        /// Gets or sets a boolean value.
        /// </summary>
        public bool BoolValue
        {
            get => _boolValue;
            set => SetPropertyValue(ref _boolValue, value, nameof(BoolValue));
        }

        /// <summary>
        /// Gets or sets the first collection of items.
        /// </summary>
        [DevExpress.ExpressApp.DC.Aggregated]
        public BindingList<CollectionItemNonPersistent> CollectionItems
        {
            get => _collectionItems;
            set => SetPropertyValue(ref _collectionItems, value, nameof(CollectionItems));
        }

        /// <summary>
        /// Gets or sets the second collection of detail items.
        /// </summary>
        [DevExpress.ExpressApp.DC.Aggregated]
        public BindingList<DetailItemNonPersistent> DetailItems
        {
            get => _detailItems;
            set => SetPropertyValue(ref _detailItems, value, nameof(DetailItems));
        }
    }
}
