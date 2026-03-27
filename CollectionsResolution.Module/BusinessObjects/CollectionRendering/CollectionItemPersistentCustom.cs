using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;

namespace CollectionsResolution.Module.BusinessObjects.CollectionRendering
{
    /// <summary>
    /// Represents a persistent item in a collection for testing custom collection rendering in XAF.
    /// This object demonstrates how sub-collections are rendered.
    /// </summary>
    public class CollectionItemPersistentCustom : BaseObject
    {
        public CollectionItemPersistentCustom(Session session) : base(session) { }

        private string _code;
        /// <summary>
        /// Gets or sets the code of the collection item.
        /// </summary>
        [Size(50)]
        public string Code
        {
            get => _code;
            set => SetPropertyValue(nameof(Code), ref _code, value);
        }

        private string _name;
        /// <summary>
        /// Gets or sets the name of the collection item.
        /// </summary>
        [Size(255)]
        public string Name
        {
            get => _name;
            set => SetPropertyValue(nameof(Name), ref _name, value);
        }

        private decimal _amount;
        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        public decimal Amount
        {
            get => _amount;
            set => SetPropertyValue(nameof(Amount), ref _amount, value);
        }

        private DateTime _date;
        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        public DateTime Date
        {
            get => _date;
            set => SetPropertyValue(nameof(Date), ref _date, value);
        }

        private bool _isActive;
        /// <summary>
        /// Gets or sets whether the item is active.
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set => SetPropertyValue(nameof(IsActive), ref _isActive, value);
        }

        /// <summary>
        /// Gets the sub-collection of items to demonstrate nested collection rendering.
        /// </summary>
        [Aggregated]
        [Association("CollectionItemPersistentCustom-SubItems")]
        public XPCollection<SubCollectionItemPersistentCustom> SubItems => GetCollection<SubCollectionItemPersistentCustom>(nameof(SubItems));

        private CollectionRenderingPersistentCustom _collectionRendering;
        /// <summary>
        /// Gets or sets the parent collection rendering object.
        /// </summary>
        [Association("CollectionRenderingPersistentCustom-CollectionItems")]
        public CollectionRenderingPersistentCustom CollectionRendering
        {
            get => _collectionRendering;
            set => SetPropertyValue(nameof(CollectionRendering), ref _collectionRendering, value);
        }
    }
}
