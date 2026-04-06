using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;

namespace CollectionsResolution.Module.BusinessObjects.CollectionRendering
{
    /// <summary>
    /// Represents a persistent sub-collection item to demonstrate nested collection rendering in XAF.
    /// This object contains primitive types (Int, String, Bool) to test sub-collection rendering.
    /// </summary>
    public class SubCollectionItemPersistentDefault : BaseObject
    {
        public SubCollectionItemPersistentDefault(Session session) : base(session) { }

        private string _identifier;
        /// <summary>
        /// Gets or sets the identifier of the sub-collection item.
        /// </summary>
        public string Identifier
        {
            get => _identifier;
            set => SetPropertyValue(nameof(Identifier), ref _identifier, value);
        }

        private int _sequenceNumber;
        /// <summary>
        /// Gets or sets the sequence number.
        /// </summary>
        public int SequenceNumber
        {
            get => _sequenceNumber;
            set => SetPropertyValue(nameof(SequenceNumber), ref _sequenceNumber, value);
        }

        private bool _isEnabled;
        /// <summary>
        /// Gets or sets whether the sub-item is enabled.
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetPropertyValue(nameof(IsEnabled), ref _isEnabled, value);
        }

        private CollectionItemPersistentDefault _collectionItem;
        /// <summary>
        /// Gets or sets the parent collection item.
        /// </summary>
        [Association("CollectionItemPersistentDefault-SubItems")]
        public CollectionItemPersistentDefault CollectionItem
        {
            get => _collectionItem;
            set => SetPropertyValue(nameof(CollectionItem), ref _collectionItem, value);
        }
    }
}
