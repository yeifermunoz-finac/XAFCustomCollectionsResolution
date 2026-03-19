using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;

namespace Solution2.Module.NonPersistentBusinessObjects.CollectionRendering
{
    /// <summary>
    /// Represents a detail item in a secondary collection for testing collection rendering in XAF Web.
    /// </summary>
    [DomainComponent]
    public class DetailItemNonPersistent : NonPersistentLiteObject
    {
        private string _description;
        private int _quantity;
        private decimal _unitPrice;
        private string _category;

        /// <summary>
        /// Gets or sets the description of the detail item.
        /// </summary>
        public string Description
        {
            get => _description;
            set => SetPropertyValue(ref _description, value);
        }

        /// <summary>
        /// Gets or sets the quantity.
        /// </summary>
        public int Quantity
        {
            get => _quantity;
            set => SetPropertyValue(ref _quantity, value);
        }

        /// <summary>
        /// Gets or sets the unit price.
        /// </summary>
        public decimal UnitPrice
        {
            get => _unitPrice;
            set => SetPropertyValue(ref _unitPrice, value);
        }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        public string Category
        {
            get => _category;
            set => SetPropertyValue(ref _category, value);
        }

        /// <summary>
        /// Gets the calculated total (Quantity * UnitPrice).
        /// </summary>
        public decimal Total => Quantity * UnitPrice;

        // INotifyPropertyChanged implementation is inherited from NonPersistentLiteObject
    }
}
