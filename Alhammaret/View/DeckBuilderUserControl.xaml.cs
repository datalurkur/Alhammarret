using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Popups;
using Alhammaret.ViewModel;

namespace Alhammaret.View
{
    public sealed partial class DeckBuilderUserControl : UserControl
    {
        private DeckBuilderViewModel viewModel;

        public DeckBuilderUserControl()
        {
            this.InitializeComponent();
            this.viewModel = new DeckBuilderViewModel();
            this.DataContext = this.viewModel;
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            ((DeckBuilderViewModel)this.DataContext).Register();
        }

        private void OnUnload(object sender, RoutedEventArgs e)
        {
            ((DeckBuilderViewModel)this.DataContext).Unregister();
        }

        private void AddToDeck(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            CardContainer card = (CardContainer)button.DataContext;
            DeckManager.Instance.ActiveDeck.AddCard(card.LoadedCard.Id);
        }

        private void RemoveFromDeck(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            CardContainer card = (CardContainer)button.DataContext;
            DeckManager.Instance.ActiveDeck.ReduceCard(card.LoadedCard.Id);
        }
    }
}
