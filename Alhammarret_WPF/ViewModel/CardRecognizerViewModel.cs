using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Alhammarret;

namespace Alhammarret.ViewModel
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
                    DBInterface.SetSetting(this.Name, value);
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
            this.Val = DBInterface.GetSetting(this.Name);
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

        private int framesScanned = 0;
        public int FramesScanned
        {
            get { return this.framesScanned; }
            set
            {
                this.framesScanned = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FramesScanned"));
            }
        }
        
        private bool scanning = false;
        public bool Scanning
        {
            get { return this.scanning; }
            set
            {
                this.scanning = value;
                if (this.scanning)
                {
                    if (!this.Recognizer.StartCapturing())
                    {
                        this.scanning = false;
                    }
                }
                else
                {
                    this.Recognizer.StopCapturing();
                }
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
                    DBInterface.SetSetting("Rotations", value);
                }
                this.rotations = value;
                this.Recognizer.SetRotation(this.rotations);
            }
        }

        private ImageSource noImagePlaceholder;

        private ImageSource previewImage;
        public ImageSource PreviewImage
        {
            get { return this.previewImage; }
            set
            {
                this.previewImage = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PreviewImage"));
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

        private ImageSource textDecomp;
        public ImageSource TextDecomp
        {
            get { return this.textDecomp; }
            set
            {
                this.textDecomp = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TextDecomp"));
            }
        }

        private ImageSource outputDebug;
        public ImageSource OutputDebug
        {
            get { return this.outputDebug; }
            set
            {
                this.outputDebug = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OutputDebug"));
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

        public Recognizer.CardRecognizer Recognizer;

        public CardRecognizerViewModel()
        {
            this.noImagePlaceholder = new BitmapImage(new System.Uri("/Assets/cancel.png", UriKind.RelativeOrAbsolute));

            this.Recognizer = new Recognizer.CardRecognizer();

            this.CannyLowerControl = new VariableControl("Canny Lower", UpdateCannyParams);
            this.CannyUpperControl = new VariableControl("Canny Upper", UpdateCannyParams);
            this.CannyKernelControl = new VariableControl("Canny Kernel", UpdateCannyParams);
            this.CannyBlurControl = new VariableControl("Canny Blur", UpdateCannyParams);
            this.MinContourControl = new VariableControl("Min Contour", UpdateContourBounds);
            this.MaxContourControl = new VariableControl("Max Contour", UpdateContourBounds);
            this.MinAreaControl = new VariableControl("Min Area", UpdateAreaBounds);
            this.MaxAreaControl = new VariableControl("Max Area", UpdateAreaBounds);

            this.Rotations = DBInterface.GetSetting("Rotations");

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

        public void ResetRecognizedCard()
        {
            this.CardCount = 1;
            this.FoilCount = 0;
            this.RecognizedCard = null;
            //this.ChosenSet = CardDB.Set.None;
        }

        private async Task<bool> Refresh()
        {
            bool ret = false;
            if (this.Recognizer.CaptureFrame())
            {
                if (this.Recognizer.FilterContours())
                {
                    if (this.Recognizer.FindCorners())
                    {
                        if (this.Recognizer.IsolateCard())
                        {
                            // Successfully framed
                            this.NameRegion = this.Recognizer.GetNameRegion();

                            string recognizedText = this.Recognizer.RecognizeText();
                            this.TextDecomp = this.Recognizer.GetTextDecomposition();
                            this.OutputDebug = this.Recognizer.GetTextOuputImage();

                            this.OCRName = $"'{recognizedText}'";
                            // FIXME
                            /*
                            SoftwareBitmap sbmp = new SoftwareBitmap(BitmapPixelFormat.Bgra8, nameBmp.PixelWidth, nameBmp.PixelHeight);
                            sbmp.CopyFromBuffer(nameBmp.PixelBuffer);

                            OcrResult result = await this.ocrEngine.RecognizeAsync(sbmp);
                            this.OCRName = $"'{result.Text}'";

                            if (result.Text != null && result.Text.Length > 0)
                            {
                                CardDB.Card card = CardDB.Instance.Get(result.Text);
                                this.RecognizedCard = card;
                                if (this.RecognizedCard != null)
                                {
                                    ret = true;
                                    this.CardRecognized?.Invoke();
                                }
                            }
                            */
                        }
                        this.TransformedImage = this.Recognizer.GetTransformedCard();
                    }
                    else
                    {
                        this.TransformedImage = this.noImagePlaceholder;
                    }
                    this.CornersImage = this.Recognizer.GetCornersDebug();
                }
                else
                {
                    this.CornersImage = this.noImagePlaceholder;
                }
                this.PreviewImage = this.Recognizer.GetPreview();
                this.ContourImage = this.Recognizer.GetContourDebug();

                this.FramesScanned += 1;
            }
            else
            {
                this.PreviewImage = this.noImagePlaceholder;
                this.ContourImage = this.noImagePlaceholder;
            }
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