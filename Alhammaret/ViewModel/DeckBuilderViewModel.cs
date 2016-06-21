using Alhammaret;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Core;

namespace Alhammaret.ViewModel
{
    class DeckBuilderViewModel : INotifyPropertyChanged
    {
        private List<CardContainer> collectionCards;
        public List<CardContainer> CollectionCards
        {
            get { return this.collectionCards; }
            set
            {
                this.collectionCards = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CollectionCards"));
            }
        }

        private List<CardContainer> deckCards;
        public List<CardContainer> DeckCards
        {
            get { return this.deckCards; }
            set
            {
                this.deckCards = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DeckCards"));
            }
        }

        public DeckBuilderViewModel()
        { }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Register()
        {
            CardCollection.Instance.OnCollectionUpdated += OnCardCollectionUpdated;
            OnCardCollectionUpdated();
            DeckManager.Instance.OnActiveDeckUpdated += OnDeckUpdated;
            OnDeckUpdated();
        }

        public void Unregister()
        {
            CardCollection.Instance.OnCollectionUpdated -= OnCardCollectionUpdated;
            DeckManager.Instance.OnActiveDeckUpdated -= OnDeckUpdated;
        }

        private void OnCardCollectionUpdated()
        {
            IEnumerable<CardCollection.Card> cards = CardCollection.Instance.AllCards();

            // Filter out cards already in the deck
            cards = cards.Where(c => DeckManager.Instance.ActiveDeck.GetCount(c.Id) < c.Count);

            // Construct
            IEnumerable<CardContainer> containers = cards.Select(c => new CardContainer(c));
            
            // Order
            containers = containers.OrderBy(c => c.CardData.Name);

            this.CollectionCards = containers.ToList();
        }

        private void OnDeckUpdated()
        {
            IEnumerable<CardCollection.Card> cards = DeckManager.Instance.ActiveDeck.AllCards();

            // Construct
            IEnumerable<CardContainer> containers = cards.Select(c => new CardContainer(c));

            // Order
            containers = containers.OrderBy(c => c.CardData.Name);

            this.DeckCards = containers.ToList();

            OnCardCollectionUpdated();
        }
    }
}