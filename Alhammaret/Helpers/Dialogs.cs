using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Popups;
using Alhammaret.ViewModel;
using System.Threading.Tasks;

namespace Alhammaret
{
    public class Dialogs
    {
        public static async Task PromptChooseSet(CardDB.Card card, UICommandInvokedHandler cmd)
        {
            MessageDialog dlg = new MessageDialog($"{card.Name} is part of multiple sets.  Please select one.", "Choose Card Set");
            dlg.Commands.Add(new UICommand("Cancel"));
            for (int i = 0; i < card.CardSets.Count; ++i)
            {
                dlg.Commands.Add(new UICommand(card.CardSets[i].ToString(), cmd, card.Ids[i]));
            }
            await dlg.ShowAsync();
        }
    }
}