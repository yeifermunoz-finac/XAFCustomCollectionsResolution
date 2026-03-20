using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;

namespace CollectionsResolution.Module.BusinessObjects.CollectionRendering
{
    /// <summary>
    /// Represents a persistent detail item in a secondary collection for testing collection rendering in XAF.
    /// </summary>
    public class DetailItemPersistent : BaseObject
    {
        public DetailItemPersistent(Session session) : base(session) { }

        private string _description;
        /// <summary>
        /// Gets or sets the description of the detail item.
        /// </summary>
        [Size(SizeAttribute.Unlimited)]
        public string Description
        {
            get => _description;
            set => SetPropertyValue(nameof(Description), ref _description, value);
        }

        private int _quantity;
        /// <summary>
        /// Gets or sets the quantity.
        /// </summary>
        public int Quantity
        {
            get => _quantity;
            set => SetPropertyValue(nameof(Quantity), ref _quantity, value);
        }

        private decimal _unitPrice;
        /// <summary>
        /// Gets or sets the unit price.
        /// </summary>
        public decimal UnitPrice
        {
            get => _unitPrice;
            set => SetPropertyValue(nameof(UnitPrice), ref _unitPrice, value);
        }

        private string _category;
        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        [Size(255)]
        public string Category
        {
            get => _category;
            set => SetPropertyValue(nameof(Category), ref _category, value);
        }

        /// <summary>
        /// Gets the calculated total (Quantity * UnitPrice).
        /// </summary>
        [PersistentAlias("Quantity * UnitPrice")]
        public decimal Total => Convert.ToDecimal(EvaluateAlias(nameof(Total)));

        private CollectionRenderingPersistentDefault _collectionRendering;
        /// <summary>
        /// Gets or sets the parent collection rendering object.
        /// </summary>
        [Association("CollectionRenderingPersistentDefault-DetailItems")]
        public CollectionRenderingPersistentDefault CollectionRendering
        {
            get => _collectionRendering;
            set => SetPropertyValue(nameof(CollectionRendering), ref _collectionRendering, value);
        }
    }
}
