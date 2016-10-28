using System;
using System.Collections.Generic;
using System.Linq;
using Alhammaret.ViewModel;
using System.Windows.Controls;

namespace Alhammaret.View
{
    public sealed partial class CardDatabaseUserControl : UserControl
    {
        private CardDatabaseViewModel viewModel;

        public CardDatabaseUserControl()
        {
            this.InitializeComponent();
            this.viewModel = new CardDatabaseViewModel();
            this.DataContext = this.viewModel;
        }

        private void OnCardSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                this.viewModel.FocusedCard = e.AddedItems[0] as CardDB.Card;
            }
            else
            {
                this.viewModel.FocusedCard = null;
            }
        }
    }
}
