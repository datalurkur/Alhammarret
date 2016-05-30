using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.UI.Xaml;

namespace Alhammaret.DesignTimeData
{
    class CardDatabaseDTD
    {
        public List<CardDB.Card> Cards { get; set; }

        public CardDatabaseDTD()
        {
            this.Cards = new List<CardDB.Card>();
            this.Cards.Add(CardDB.Card.Sample1());
            this.Cards.Add(CardDB.Card.Sample2());
        }
    }
}

namespace Alhammaret.ViewModel
{
    class CardDatabaseViewModel : INotifyPropertyChanged
    {
        private List<CardDB.Card> cards;
        public List<CardDB.Card> Cards
        {
            get { return this.cards; }
            set
            {
                this.cards = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Cards"));
            }
        }

        private CardDB.Card focusedCard;
        public CardDB.Card FocusedCard
        {
            get { return this.focusedCard; }
            set
            {
                this.focusedCard = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FocusedCard"));
            }
        }

        public CardDatabaseViewModel()
        { }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Register()
        {
            CardDB.Instance.OnDatabaseUpdated += OnCardDatabaseUpdated;
            OnCardDatabaseUpdated();
        }

        public void Unregister()
        {

            CardDB.Instance.OnDatabaseUpdated -= OnCardDatabaseUpdated;
        }

        private void OnCardDatabaseUpdated()
        {
            this.Cards = CardDB.Instance.AllCards().OrderBy(c => c.Name).ToList();
        }

    }
}