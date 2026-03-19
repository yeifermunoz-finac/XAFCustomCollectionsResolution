using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using System;

namespace Solution2.Module.NonPersistentBusinessObjects.CollectionRendering
{
    /// <summary>
    /// Represents an item in a collection for testing collection rendering in XAF Web.
    /// </summary>
    [DomainComponent]
    public class CollectionItemNonPersistent : NonPersistentLiteObject
    {
        private string _code;
        private string _name;
        private decimal _amount;
        private DateTime _date;
        private bool _isActive;

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

        // INotifyPropertyChanged implementation is inherited from NonPersistentLiteObject
    }
}
