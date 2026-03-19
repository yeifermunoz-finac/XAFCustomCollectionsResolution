using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;

namespace CollectionsResolution.Module.NonPersistentBusinessObjects.CollectionRendering
{
    /// <summary>
    /// Represents a sub-collection item to demonstrate nested collection rendering in XAF.
    /// This object contains primitive types (Int, String, Bool) to test sub-collection rendering.
    /// </summary>
    [DomainComponent]
    public class SubCollectionItemNonPersistent : NonPersistentLiteObject
    {
        private string _identifier;
        private int _sequenceNumber;
        private bool _isEnabled;

        /// <summary>
        /// Gets or sets the identifier of the sub-collection item.
        /// </summary>
        public string Identifier
        {
            get => _identifier;
            set => SetPropertyValue(ref _identifier, value);
        }

        /// <summary>
        /// Gets or sets the sequence number.
        /// </summary>
        public int SequenceNumber
        {
            get => _sequenceNumber;
            set => SetPropertyValue(ref _sequenceNumber, value);
        }

        /// <summary>
        /// Gets or sets whether the sub-item is enabled.
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetPropertyValue(ref _isEnabled, value);
        }

        // INotifyPropertyChanged implementation is inherited from NonPersistentLiteObject
    }
}
