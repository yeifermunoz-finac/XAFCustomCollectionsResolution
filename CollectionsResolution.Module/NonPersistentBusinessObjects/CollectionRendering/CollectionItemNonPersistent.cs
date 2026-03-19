using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using System;
using System.ComponentModel;

namespace CollectionsResolution.Module.NonPersistentBusinessObjects.CollectionRendering
{
    /// <summary>
    /// Represents an item in a collection for testing collection rendering in XAF Web.
    /// This object demonstrates how sub-collections are rendered.
    /// </summary>
    [DomainComponent]
    public class CollectionItemNonPersistent : NonPersistentLiteObject
    {
        private string _code;
        private string _name;
        private decimal _amount;
        private DateTime _date;
        private bool _isActive;
        private BindingList<SubCollectionItemNonPersistent> _subItems;

        public CollectionItemNonPersistent()
        {
            _subItems = new BindingList<SubCollectionItemNonPersistent>();
        }

        /// <summary>
        /// Gets or sets the code of the collection item.
        /// </summary>
        public string Code
        {
            get => _code;
            set => SetPropertyValue(ref _code, value);
        }

        /// <summary>
        /// Gets or sets the name of the collection item.
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetPropertyValue(ref _name, value);
        }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        public decimal Amount
        {
            get => _amount;
            set => SetPropertyValue(ref _amount, value);
        }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        public DateTime Date
        {
            get => _date;
            set => SetPropertyValue(ref _date, value);
        }

        /// <summary>
        /// Gets or sets whether the item is active.
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set => SetPropertyValue(ref _isActive, value);
        }

        /// <summary>
        /// Gets or sets the sub-collection of items to demonstrate nested collection rendering.
        /// </summary>
        [Aggregated]
        public BindingList<SubCollectionItemNonPersistent> SubItems
        {
            get => _subItems;
            set => SetPropertyValue(ref _subItems, value);
        }

        // INotifyPropertyChanged implementation is inherited from NonPersistentLiteObject
    }
}
