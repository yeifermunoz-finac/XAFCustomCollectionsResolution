using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;

namespace CollectionsResolution.Module.BusinessObjects.CollectionRendering
{
    /// <summary>
    /// Persistent object to verify collection rendering with custom editors in XAF.
    /// This object contains primitive types and two collections to test custom rendering.
    /// </summary>
    public class CollectionRenderingPersistentCustom : BaseObject
    {
        public CollectionRenderingPersistentCustom(Session session) : base(session) { }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            _dateValue = DateTime.Today;
        }

        private string _testName;
        /// <summary>
        /// Gets or sets the test name.
        /// </summary>
        [Size(255)]
        public string TestName
        {
            get => _testName;
            set => SetPropertyValue(nameof(TestName), ref _testName, value);
        }

        private string _description;
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description
        {
            get => _description;
            set => SetPropertyValue(nameof(Description), ref _description, value);
        }

        private int _numberValue;
        /// <summary>
        /// Gets or sets a numeric integer value.
        /// </summary>
        public int NumberValue
        {
            get => _numberValue;
            set => SetPropertyValue(nameof(NumberValue), ref _numberValue, value);
        }

        private decimal _decimalValue;
        /// <summary>
        /// Gets or sets a decimal value.
        /// </summary>
        public decimal DecimalValue
        {
            get => _decimalValue;
            set => SetPropertyValue(nameof(DecimalValue), ref _decimalValue, value);
        }

        private DateTime _dateValue;
        /// <summary>
        /// Gets or sets a date value.
        /// </summary>
        public DateTime DateValue
        {
            get => _dateValue;
            set => SetPropertyValue(nameof(DateValue), ref _dateValue, value);
        }

        private bool _boolValue;
        /// <summary>
        /// Gets or sets a boolean value.
        /// </summary>
        public bool BoolValue
        {
            get => _boolValue;
            set => SetPropertyValue(nameof(BoolValue), ref _boolValue, value);
        }

        /// <summary>
        /// Gets the first collection of items.
        /// </summary>
        [Association("CollectionRenderingPersistentCustom-CollectionItems")]
        [Aggregated]
        public XPCollection<CollectionItemPersistentCustom> CollectionItems => GetCollection<CollectionItemPersistentCustom>(nameof(CollectionItems));

        /// <summary>
        /// Gets the second collection of detail items.
        /// </summary>
        [Association("CollectionRenderingPersistentCustom-DetailItems")]
        [Aggregated]
        public XPCollection<DetailItemPersistentCustom> DetailItems => GetCollection<DetailItemPersistentCustom>(nameof(DetailItems));
    }
}
