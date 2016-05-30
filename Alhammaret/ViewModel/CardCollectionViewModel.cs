﻿using Alhammaret;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Core;

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

        private List<CardCollection.Card> cards;
        public List<CardCollection.Card> Cards
        {
            get { return this.cards; }
            set
            {
                this.cards = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Cards"));
            }
        }

        public CardCollectionViewModel()
        { }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Register()
        {
            CardDB.Instance.OnDatabaseUpdated += OnCardDatabaseUpdated;
            OnCardDatabaseUpdated();
            CardCollection.Instance.OnCollectionUpdated += OnCardCollectionUpdated;
            OnCardCollectionUpdated();
        }

        public void Unregister()
        {

            CardDB.Instance.OnDatabaseUpdated -= OnCardDatabaseUpdated;
            CardCollection.Instance.OnCollectionUpdated -= OnCardCollectionUpdated;
        }

        private void OnCardDatabaseUpdated()
        {
            this.AllCardNames = CardDB.Instance.AllCards().Select(c => c.Name).ToList();
        }

        private void OnCardCollectionUpdated()
        {
            this.Cards = CardCollection.Instance.AllCards();
        }
    }
}