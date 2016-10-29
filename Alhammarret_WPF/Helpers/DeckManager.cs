using System.Collections.Generic;

namespace Alhammarret
{
    public class DeckManager
    {
        private static DeckManager instance;
        public static DeckManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DeckManager();
                }
                return instance;
            }
        }

        private CardCollection activeDeck;
        public CardCollection ActiveDeck
        {
            get { return this.activeDeck; }
            private set
            {
                if (this.activeDeck != null)
                {
                    this.activeDeck.OnCollectionUpdated -= OnActiveDeckUpdated_Internal;
                }
                this.activeDeck = value;
                if (this.activeDeck != null)
                {
                    this.activeDeck.OnCollectionUpdated += OnActiveDeckUpdated_Internal;
                }
            }
        }
        
        public CardCollection.CollectionUpdated OnActiveDeckUpdated;

        private DeckManager()
        {
            this.ActiveDeck = new CardCollection(true);
        }

        private void OnActiveDeckUpdated_Internal()
        {
            this.OnActiveDeckUpdated?.Invoke();
        }
    }
}