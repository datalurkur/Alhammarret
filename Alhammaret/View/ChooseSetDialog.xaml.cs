using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Alhammaret.View
{
    public sealed partial class ChooseSetDialog : ContentDialog
    {
        public delegate void SetSelected(int cardId);

        private CardDB.Card card;
        private SetSelected onSetSelected;

        public CardDB.Set SelectedSet { get; set; }
        public List<CardDB.Set> CardSets { get; private set; }
        public string CardName { get; private set; }

        public ChooseSetDialog(CardDB.Card card, SetSelected onSelect)
        {
            this.card = card;
            this.onSetSelected = onSelect;
            this.InitializeComponent();

            this.CardSets = this.card.CardSets;
            this.CardName = this.card.Name;
            this.SelectedSet = this.card.CardSets[0];

            this.DataContext = this;

            this.PrimaryButtonText = "OK";
            this.SecondaryButtonText = "Cancel";
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            int idx = this.CardSets.IndexOf(this.SelectedSet);
            if (idx != -1)
            {
                int cardId = this.card.Ids[idx];
                this.onSetSelected?.Invoke(cardId);
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
