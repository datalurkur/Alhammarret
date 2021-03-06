﻿using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml;
using Windows.UI.Popups;
using Alhammaret.ViewModel;
using System.Threading.Tasks;
using Alhammaret.View;

namespace Alhammaret
{
    public class Dialogs
    {
        public static async Task PromptChooseSet(CardDB.Card card, ChooseSetDialog.SetSelected cmd)
        {
            ChooseSetDialog dlg = new ChooseSetDialog(card, cmd);
            await dlg.ShowAsync();
        }
    }
}