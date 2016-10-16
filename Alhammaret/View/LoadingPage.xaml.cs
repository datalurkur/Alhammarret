using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Alhammaret.ViewModel;
using System;

namespace Alhammaret.View
{
    public sealed partial class LoadingPage : Page
    {
        private LoadingProgressViewModel viewModel;

        public LoadingPage()
        {
            this.InitializeComponent();
            this.DataContext = this.viewModel = new LoadingProgressViewModel();
        }

        private async void OnLoad(object sender, RoutedEventArgs e)
        {
            CardDB.Instance.OnDatabaseUpdated += OnUpdated;
            CardCollection.Instance.OnCollectionUpdated += CheckReady;

            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => CardDB.Instance.Build());
            bool result = false;
            try
            {
                result = await CardCollection.Instance.DefaultImport();
            }
            catch { }
            if (!result)
            {
                CardCollection.Instance.Import();
            }
        }

        private void OnUnload(object sender, RoutedEventArgs e)
        {
            CardDB.Instance.OnDatabaseUpdated -= OnUpdated;
            CardCollection.Instance.OnCollectionUpdated -= CheckReady;
        }

        private void OnUpdated(float progress)
        {
            this.viewModel.LoadProgress = (int)(100 * progress);
            CheckReady();
        }

        private void CheckReady()
        {
            if (CardDB.Instance.Ready && CardCollection.Instance.Ready)
            {
                // Move to main menu
                this.Frame.Navigate(typeof(MainPage), null);
            }
        }
    }
}
