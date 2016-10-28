using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;

public class WebcamHelper
{
    /*
    private MediaCapture mediaCapture;
    private bool isInitialized = false;
    private bool isPreviewing = false;

    private DeviceInformationCollection devices;
    private int deviceIndex;

    public WebcamHelper()
    {
    }

    public async Task NextCamera(CaptureElement element)
    {
        this.deviceIndex = (this.deviceIndex + 1) % this.devices.Count;
        await Teardown();
        await Initialize();
        await StartPreview(element);
    }

    public async Task Initialize()
    {
        if (this.devices == null)
        {
            this.devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            this.deviceIndex = 0;
        }
        if (this.deviceIndex >= this.devices.Count) { return; }
        var cameraDevice = this.devices[this.deviceIndex];

        if (cameraDevice == null)
        {
            Debug.WriteLine("No camera device found!");
            return;
        }

        this.mediaCapture = new MediaCapture();
        var settings = new MediaCaptureInitializationSettings { VideoDeviceId = cameraDevice.Id };

        await this.mediaCapture.InitializeAsync(settings);
        this.isInitialized = true;
    }

    public async Task Teardown()
    {
        if (this.isInitialized)
        {
            if (this.isPreviewing)
            {
                await StopPreview();
            }

            this.isInitialized = false;
        }

        if (this.mediaCapture != null)
        {
            this.mediaCapture.Dispose();
            this.mediaCapture = null;
        }
    }

    public async Task StartPreview(CaptureElement element)
    {
        if (!this.isPreviewing && this.mediaCapture != null)
        {
            element.Source = this.mediaCapture;
            await this.mediaCapture.StartPreviewAsync();
            this.isPreviewing = true;
        }
    }

    public async Task StopPreview()
    {
        if (!this.isPreviewing || this.mediaCapture == null) { return; }
        this.isPreviewing = false;
        await this.mediaCapture.StopPreviewAsync();
    }

    public async Task<WriteableBitmap> GetImage()
    {
        if (this.mediaCapture == null)
        {
            return null;
        }
        InMemoryRandomAccessStream inStream = new InMemoryRandomAccessStream();
        await this.mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), inStream);
        BitmapDecoder decoder = await BitmapDecoder.CreateAsync(inStream);
        BitmapTransform transform = new BitmapTransform();
        PixelDataProvider provider = await decoder.GetPixelDataAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, transform, ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.DoNotColorManage);
        byte[] sourcePixels = provider.DetachPixelData();

        WriteableBitmap bmp = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
        using (Stream outputStream = bmp.PixelBuffer.AsStream())
        {
            await outputStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
        }
        return bmp;
    }

    private async Task<DeviceInformation> FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel desiredPanel)
    {
        var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
        DeviceInformation desiredDevice = allVideoDevices.FirstOrDefault(x => x.EnclosureLocation != null && x.EnclosureLocation.Panel == desiredPanel);
        return desiredDevice ?? allVideoDevices.FirstOrDefault();
    }
    */
}
