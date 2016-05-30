using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml;
using Windows.Media.Ocr;
using Windows.Graphics.Imaging;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Diagnostics;
using Recognizer;

namespace Alhammaret.ViewModel
{
    class VariableControl : INotifyPropertyChanged
    {
        public string Name { get; private set; }

        private int val;
        public int Val
        {
            get { return this.val; }
            set
            {
                if (this.val != value)
                {
                    Settings.Instance.Set(this.Name, value);
                }
                this.val = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Val"));
                this.onDataChanged?.Invoke();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public delegate void DataChanged();

        private DataChanged onDataChanged;

        public VariableControl(string name, DataChanged odc)
        {
            this.Name = name;
            this.Val = Settings.Instance.Get(name);
            this.onDataChanged = odc;
        }

        public void IncreaseVal()
        {
            this.Val += ModAmount();
        }

        public void DecreaseVal()
        {
            this.Val -= ModAmount();
        }

        private int ModAmount()
        {
                 if (this.Val > 100000) { return 10000; }
            else if (this.Val > 10000)  { return 1000; }
            else if (this.Val > 1000)   { return 100; }
            else if (this.Val > 100)    { return 10; }
            else { return 1; }
        }
    }

    class CardRecognizerViewModel : INotifyPropertyChanged
    {
        public delegate void CardRecognizedHandler();
        public CardRecognizedHandler CardRecognized;

        public VariableControl CannyLowerControl { get; private set; }
        public VariableControl CannyUpperControl { get; private set; }
        public VariableControl CannyKernelControl { get; private set; }
        public VariableControl CannyBlurControl { get; private set; }
        public VariableControl MinContourControl { get; private set; }
        public VariableControl MaxContourControl { get; private set; }
        public VariableControl MinAreaControl { get; private set; }
        public VariableControl MaxAreaControl { get; private set; }
        
        private bool scanning = false;
        public bool Scanning
        {
            get { return this.scanning; }
            set
            {
                this.scanning = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Scanning"));
            }
        }

        private int rotations;
        public int Rotations
        {
            get { return this.rotations; }
            set
            {
                if (this.rotations != value)
                {
                    Settings.Instance.Set("Rotations", value);
                }
                this.rotations = value;
                this.Recognizer.SetRotation(this.rotations);
            }
        }

        private ImageSource contourImage;
        public ImageSource ContourImage
        {
            get { return this.contourImage; }
            set
            {
                this.contourImage = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ContourImage"));
            }
        }

        private ImageSource cornersImage;
        public ImageSource CornersImage
        {
            get { return this.cornersImage; }
            set
            {
                this.cornersImage = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CornersImage"));
            }
        }

        private ImageSource transformedImage;
        public ImageSource TransformedImage
        {
            get { return this.transformedImage; }
            set
            {
                this.transformedImage = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TransformedImage"));
            }
        }

        private ImageSource nameRegion;
        public ImageSource NameRegion
        {
            get { return this.nameRegion; }
            set
            {
                this.nameRegion = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NameRegion"));
            }
        }

        private CardDB.Card recognizedCard;
        public CardDB.Card RecognizedCard
        {
            get { return this.recognizedCard; }
            set
            {
                this.recognizedCard = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RecognizedCard"));
            }
        }

        private CardDB.Set chosenSet;
        public CardDB.Set ChosenSet
        {
            get { return this.chosenSet; }
            set
            {
                this.chosenSet = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ChosenSet"));
            }
        }

        private string ocrName;
        public string OCRName
        {
            get { return this.ocrName; }
            set
            {
                this.ocrName = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OCRName"));
            }
        }

        private int cardCount = 1;
        public int CardCount
        {
            get { return this.cardCount; }
            set
            {
                this.cardCount = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CardCount"));
            }
        }

        private int foilCount = 0;
        public int FoilCount
        {
            get { return this.foilCount; }
            set
            {
                this.foilCount = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FoilCount"));
            }
        }

        public CardRecognizer Recognizer;
        public WebcamHelper CamHelper;

        private OcrEngine ocrEngine;
        private bool ready = false;

        public CardRecognizerViewModel()
        {
            this.CamHelper = new WebcamHelper();
            this.Recognizer = new CardRecognizer();
            this.ocrEngine = OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language("en"));

            this.CannyLowerControl = new VariableControl("Canny Lower", UpdateCannyParams);
            this.CannyUpperControl = new VariableControl("Canny Upper", UpdateCannyParams);
            this.CannyKernelControl = new VariableControl("Canny Kernel", UpdateCannyParams);
            this.CannyBlurControl = new VariableControl("Canny Blur", UpdateCannyParams);
            this.MinContourControl = new VariableControl("Min Contour", UpdateContourBounds);
            this.MaxContourControl = new VariableControl("Max Contour", UpdateContourBounds);
            this.MinAreaControl = new VariableControl("Min Area", UpdateAreaBounds);
            this.MaxAreaControl = new VariableControl("Max Area", UpdateAreaBounds);

            this.Rotations = Settings.Instance.Get("Rotations");

            UpdateCannyParams();
            UpdateContourBounds();
            UpdateAreaBounds();
        }

        private void UpdateCannyParams()
        {
            this.Recognizer.SetCannyParams(this.CannyLowerControl.Val, this.CannyUpperControl.Val, this.CannyKernelControl.Val, this.CannyBlurControl.Val);
        }

        private void UpdateContourBounds()
        {
            this.Recognizer.SetContourBounds(this.MinContourControl.Val, this.MaxContourControl.Val);
        }

        private void UpdateAreaBounds()
        {
            this.Recognizer.SetAreaBounds(this.MinAreaControl.Val, this.MaxAreaControl.Val);
        }

        public async void Initialize(CaptureElement captureElement)
        {
            await this.CamHelper.Initialize();
            await this.CamHelper.StartPreview(captureElement);
            CardDB.Instance.OnDatabaseUpdated += OnCardsUpdated;
            CardCollection.Instance.OnCollectionUpdated += OnCardsUpdated;
        }

        public async void Teardown()
        {
            await this.CamHelper.StopPreview();
            await this.CamHelper.Teardown();
            CardDB.Instance.OnDatabaseUpdated -= OnCardsUpdated;
            CardCollection.Instance.OnCollectionUpdated -= OnCardsUpdated;
        }

        private void OnCardsUpdated()
        {
            if (CardDB.Instance.Ready && CardCollection.Instance.Ready)
            {
                this.ready = true;
            }
            else
            {
                this.ready = false;
            }
        }

        public void ResetRecognizedCard()
        {
            this.CardCount = 1;
            this.foilCount = 0;
            this.RecognizedCard = null;
            this.ChosenSet = CardDB.Set.None;
        }

        private async Task<bool> Refresh()
        {
            bool ret = false;
            WriteableBitmap bmp = await this.CamHelper.GetImage();
            this.Recognizer.SetFrame(bmp);
            if (this.Recognizer.FilterContours())
            {
                if (this.Recognizer.FindCorners())
                {
                    if (this.Recognizer.IsolateCard())
                    {
                        // Successfully framed
                        WriteableBitmap nameBmp = this.Recognizer.GetNameRegion();
                        this.NameRegion = nameBmp;

                        SoftwareBitmap sbmp = new SoftwareBitmap(BitmapPixelFormat.Bgra8, nameBmp.PixelWidth, nameBmp.PixelHeight);
                        sbmp.CopyFromBuffer(nameBmp.PixelBuffer);

                        OcrResult result = await this.ocrEngine.RecognizeAsync(sbmp);
                        this.OCRName = $"'{result.Text}'";

                        if (this.ready && result.Text != null && result.Text.Length > 0)
                        {
                            CardDB.Card card = CardDB.Instance.Get(result.Text);
                            this.RecognizedCard = card;
                            if (this.RecognizedCard != null)
                            {
                                ret = true;
                                this.CardRecognized?.Invoke();
                            }
                        }
                    }
                    this.TransformedImage = this.Recognizer.GetTransformedCard();
                }
                this.CornersImage = this.Recognizer.GetCornersDebug();
            }
            this.ContourImage = this.Recognizer.GetContourDebug();
            return ret;
        }

        public async void ActivelyScan(int delay)
        {
            if (this.Scanning) { return; }

            bool foundCard = false;
            this.Scanning = true;
            while (!foundCard && this.Scanning)
            {
                foundCard |= await Refresh();
                await Task.Delay(delay);
            }
            this.Scanning = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}