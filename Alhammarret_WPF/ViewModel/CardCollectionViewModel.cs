using Alhammaret;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Alhammaret.DesignTimeData
{
    class CardCollectionDTD
    {
        public List<string> AllCardNames { get; set; }
        public List<CardCollection.Card> Cards { get; set; }

        public CardCollectionDTD()
        {
            this.AllCardNames = new List<string>();
            this.AllCardNames.Add("Alhammaret's Archive");
            this.AllCardNames.Add("Tireless Tracker");

            this.Cards = new List<CardCollection.Card>();
            /* Replace with multiverse IDs
            this.Cards.Add(new CardCollection.Card("Alhammaret's Archive"));
            this.Cards.Add(new CardCollection.Card("Tireless Tracker"));
            */
        }
    }
}

namespace Alhammaret.ViewModel
{
    class CardContainer
    {
        public CardCollection.Card LoadedCard { get; set; }
        public CardDB.Set CardSet { get; set; }
        public CardDB.Card CardData { get; set; }

        public CardContainer(CardCollection.Card c)
        {
            this.LoadedCard = c;
            this.CardSet = CardDB.Instance.GetSet(this.LoadedCard.Id);
            this.CardData = CardDB.Instance.Get(this.LoadedCard.Id);
        }
    }

    class CardCollectionViewModel : INotifyPropertyChanged
    {
        public List<string> AllCardNames { get; private set; }

        private CardDB.Set activeSet = CardDB.Set.ShadowsOverInnistrad;
        public CardDB.Set ActiveSet
        {
            get { return this.activeSet; }
            set
            {
                this.activeSet = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ActiveSet"));
            }
        }

        private List<CardContainer> cards;
        public List<CardContainer> Cards
        {
            get { return this.cards; }
            set
            {
                this.cards = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Cards"));
            }
        }

        public CardCollectionViewModel()
        {
            this.AllCardNames = CardDB.Instance.AllCards().OrderBy(c => c.Name).Select(c => c.Name).ToList();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Register()
        {
            CardCollection.Instance.OnCollectionUpdated += OnCardCollectionUpdated;
            OnCardCollectionUpdated();
        }

        public void Unregister()
        {
            CardCollection.Instance.OnCollectionUpdated -= OnCardCollectionUpdated;
        }

        private void OnCardCollectionUpdated()
        {
            this.Cards = CardCollection.Instance.AllCards().Select(c => new CardContainer(c)).OrderBy(c => c.CardData.Name).ToList();
        }
    }
}