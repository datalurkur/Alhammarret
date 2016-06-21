using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Popups;
using Alhammaret.ViewModel;
using System.Diagnostics;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Alhammaret.View
{
    public sealed partial class RecognizeCardUserControl : UserControl
    {
        private const float kButtonScalar = 1.5f;

        private CardRecognizerViewModel viewModel;

        public RecognizeCardUserControl()
        {
            this.DataContext = this.viewModel = new CardRecognizerViewModel();
            this.InitializeComponent();
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            this.viewModel.Initialize(this.RawFrameFeed);
            this.viewModel.CardRecognized += OnCardRecognized;
        }

        private void OnUnload(object sender, RoutedEventArgs e)
        {
            this.viewModel.CardRecognized -= OnCardRecognized;
            this.viewModel.Teardown();
        }

        private void RotateCW(object sender, RoutedEventArgs e)
        {
            this.viewModel.Rotations -= 1;
        }
        private void RotateCCW(object sender, RoutedEventArgs e)
        {
            this.viewModel.Rotations += 1;
        }

        private void StartScanning(object sender, RoutedEventArgs e)
        {
            this.viewModel.ActivelyScan(250);
        }

        private void DecreaseCardCount(object sender, RoutedEventArgs e)
        {
            if (this.viewModel.CardCount <= 1) { return; }
            this.viewModel.CardCount -= 1;
        }

        private void IncreaseCardCount(object sender, RoutedEventArgs e)
        {
            this.viewModel.CardCount += 1;
        }

        private void DecreaseFoilCount(object sender, RoutedEventArgs e)
        {
            if (this.viewModel.FoilCount < 1) { return; }
            DecreaseCardCount(sender, e);
            this.viewModel.FoilCount -= 1;
        }

        private void IncreaseFoilCount(object sender, RoutedEventArgs e)
        {
            IncreaseCardCount(sender, e);
            this.viewModel.FoilCount += 1;
        }

        private async void CardRecognitionConfirmed(object sender, RoutedEventArgs e)
        {
            if (this.viewModel.RecognizedCard.CardSets.Count == 1)
            {
                AddCard(this.viewModel.RecognizedCard.Ids[0]);
            }
            else
            {
                int idx = this.viewModel.RecognizedCard.CardSets.IndexOf(this.viewModel.ChosenSet);
                if (idx != -1)
                {
                    AddCard(this.viewModel.RecognizedCard.Ids[idx]);
                }
                else
                {
                    await Dialogs.PromptChooseSet(this.viewModel.RecognizedCard, AddCard);
                }

            }
            this.viewModel.ResetRecognizedCard();
            this.viewModel.ActivelyScan(250);
        }

        private void AddCardCallback(IUICommand cmd) { AddCard((int)cmd.Id); }
        private void AddCard(int id)
        {
            CardCollection.Instance.AddCard(id, this.viewModel.CardCount, this.viewModel.FoilCount);
        }

        private void CardRecognitionDenied(object sender, RoutedEventArgs e)
        {
            this.viewModel.ResetRecognizedCard();
            this.viewModel.ActivelyScan(250);
        }

        private async void SwitchCams(object sender, RoutedEventArgs e)
        {
            await this.viewModel.CamHelper.NextCamera(this.RawFrameFeed);
        }

        private void OnCardRecognized()
        {
            //this.SoundSource.Play(Sound)
        }
    }
}
