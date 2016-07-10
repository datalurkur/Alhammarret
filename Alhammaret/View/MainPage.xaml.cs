using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;

namespace Alhammaret.View
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void DoImport(object sender, RoutedEventArgs e)
        {
            if (!await CardCollection.Instance.DefaultImport())
            {
                CardCollection.Instance.Import();
            }
        }

        private async void DoExport(object sender, RoutedEventArgs e)
        {
            if (!await CardCollection.Instance.DefaultExport())
            {
                CardCollection.Instance.Export();
            }
        }
        
        private async void OnUnload(object sender, RoutedEventArgs e)
        {
            if (!await CardCollection.Instance.DefaultExport())
            {
                CardCollection.Instance.Export();
            }
        }
    }
}
