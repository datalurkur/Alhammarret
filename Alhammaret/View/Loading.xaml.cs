using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;

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

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => CardDB.Instance.Build());
            CardCollection.Instance.Import();

            CardDB.Instance.OnDatabaseUpdated += OnUpdated;
            CardCollection.Instance.OnCollectionUpdated += OnUpdated;
        }

        private void OnUnload(object sender, RoutedEventArgs e)
        {
            CardDB.Instance.OnDatabaseUpdated -= OnUpdated;
            CardCollection.Instance.OnCollectionUpdated -= OnUpdated;
        }

        private void OnUpdated(float progress)
        {
            if (CardDB.Instance.Ready && CardCollection.Instance.Ready)
            {
                // Move to main menu
                this.Frame.Navigate(typeof(MainPage), null);
            }
            else
            {
                this.LoadProgress = (int)(100 * progress);
            }
        }
    }
}
