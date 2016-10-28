using System.Windows.Controls;
using System.Windows;
using Alhammaret.ViewModel;

namespace Alhammaret.View
{
    public sealed partial class MainPage : Page
    {
        private LoadingProgressViewModel viewModel;

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = this.viewModel = new LoadingProgressViewModel();
        }

        private void DoImport(object sender, RoutedEventArgs e)
        {
            if (!CardCollection.Instance.DefaultImport())
            {
                CardCollection.Instance.Import();
            }
        }

        private void DoExport(object sender, RoutedEventArgs e)
        {
            if (!CardCollection.Instance.DefaultExport())
            {
                CardCollection.Instance.Export();
            }
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            CardDB.Instance.OnDatabaseUpdated += OnUpdated;
            CardCollection.Instance.OnCollectionUpdated += CheckReady;

            CardDB.Instance.Build();
            bool result = false;
            try
            {
                result = CardCollection.Instance.DefaultImport();
            }
            catch { }
            if (!result)
            {
                CardCollection.Instance.Import();
            }
        }
        
        private void OnUnload(object sender, RoutedEventArgs e)
        {
            if (!CardCollection.Instance.DefaultExport())
            {
                CardCollection.Instance.Export();
            }
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
                this.LoadingArea.Visibility = Visibility.Collapsed;
                this.AppArea.Visibility = Visibility.Visible;
            }
        }
    }
}
