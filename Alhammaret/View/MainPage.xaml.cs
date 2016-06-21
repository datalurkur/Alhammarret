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

        private void DoImport(object sender, RoutedEventArgs e)
        {
            CardCollection.Instance.Import();
        }

        private void DoExport(object sender, RoutedEventArgs e)
        {
            CardCollection.Instance.Export();
        }
        
        private void OnUnload(object sender, RoutedEventArgs e)
        {
            CardCollection.Instance.Export();
        }
    }
}
