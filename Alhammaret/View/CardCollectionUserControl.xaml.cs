using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Popups;
using Alhammaret.ViewModel;

namespace Alhammaret.View
{
    public sealed partial class CardCollectionUserControl : UserControl
    {
        private CardCollectionViewModel viewModel;

        public CardCollectionUserControl()
        {
            this.InitializeComponent();
            this.viewModel = new CardCollectionViewModel();
            this.DataContext = this.viewModel;
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            ((CardCollectionViewModel)this.DataContext).Register();
        }

        private void OnUnload(object sender, RoutedEventArgs e)
        {
            ((CardCollectionViewModel)this.DataContext).Unregister();
        }

        private void OnTextChanged(AutoSuggestBox box, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string currentText = box.Text;
                List<string> matches = this.viewModel.AllCardNames.Where(c => c.IndexOf(currentText, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                box.ItemsSource = matches;
            }
        }

        private async void OnQuerySubmitted(AutoSuggestBox box, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string chosenCard = null;
            if (args.ChosenSuggestion != null)
            {
                chosenCard = (string)args.ChosenSuggestion;
            }
            else
            {
                chosenCard = box.Text;
            }
            if (this.viewModel.AllCardNames.Contains(chosenCard))
            {
                CardDB.Card chosenCardData = CardDB.Instance.Get(chosenCard);
                if (chosenCardData.CardSets.Count == 1)
                {
                    AddCard(chosenCardData.Ids[0]);
                }
                else
                {
                    int indexOfSet = chosenCardData.CardSets.IndexOf(this.viewModel.ActiveSet);
                    if (chosenCardData.CardSets.Contains(this.viewModel.ActiveSet))
                    {
                        AddCard(chosenCardData.Ids[indexOfSet]);
                    }
                    else
                    {
                        await Dialogs.PromptChooseSet(chosenCardData, AddCard);
                    }
                }
            }
            box.ItemsSource = null;
            box.Text = "";
        }

        private void AddCardCommand(IUICommand cmd)
        {
            AddCard((int)cmd.Id);
        }

        private void AddCard(int id)
        {
            CardCollection.Instance.AddCard(id, 1, 0);
        }

        private void ReduceCount(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            CardContainer card = (CardContainer)button.DataContext;
            CardCollection.Instance.ReduceCard(card.LoadedCard.Id, 1, 0);
        }

        private void RemoveCard(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            CardContainer card = (CardContainer)button.DataContext;
            CardCollection.Instance.RemoveCard(card.LoadedCard.Id);
        }
    }
}
