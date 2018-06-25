using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Capture;
using Windows.ApplicationModel;
using Windows.System.Display;
using Windows.Graphics.Display;
using Windows.UI.Popups;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Devices.Enumeration;
using Windows.Media.Capture.Frames;
using Windows.Media.Core;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Media.MediaProperties;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.Media;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Vision
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MediaCapture mediaCapture;
        MediaCaptureInitializationSettings mediaSettings;
        bool isPreviewing;
        DisplayRequest displayRequest = new DisplayRequest();
        public string mainText { get; set; }
        IReadOnlyList<Windows.Media.Capture.Frames.MediaFrameSourceGroup> sensorGroups;

        public MainPage()
        {
            this.InitializeComponent();
            mainText = "Person: 30";

            GetCameras();
        }

        public async void GetCameras()
        {
            var cameras = new List<string>();
            cameras = await this.GetVideoProfileSupportedDeviceIdAsync();

            foreach (var camera in cameras)
                cam.Items.Add(camera);
        }

        public async void PreviewCamera()
        {
            await StartPreviewAsync();

            //Instructions: Uncomment to test ability to save frames locally
            //Continually saves frame to a local folder while videos are running.
            //await GetPreviewFrameAsSoftwareBitmapAsync();
        }

        /// <summary>
        /// Connect to Camera asynchronously
        /// </summary>
        /// <returns>Asynchronous Task</returns>
        private async Task StartPreviewAsync()
        {
            try
            {
                mediaCapture = new MediaCapture();

                mediaSettings = new MediaCaptureInitializationSettings { VideoDeviceId = cam.SelectedValue.ToString() };
                await mediaCapture.InitializeAsync(mediaSettings);

                displayRequest.RequestActive();

                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
            }
            catch (UnauthorizedAccessException)
            {
                // This will be thrown if the user denied access to the camera in privacy settings
                var dialog = new MessageDialog("The app was denied access to the camera", "UnauthorizedAccessException");
                var result = await dialog.ShowAsync();
                return;
            }

            try
            {

                PreviewControl.Source = mediaCapture;
                await mediaCapture.StartPreviewAsync();

                isPreviewing = true;
            }
            catch (System.IO.FileLoadException)
            {
                mediaCapture.CaptureDeviceExclusiveControlStatusChanged += _mediaCapture_CaptureDeviceExclusiveControlStatusChanged;
            }
        }


        private async void _mediaCapture_CaptureDeviceExclusiveControlStatusChanged(MediaCapture sender, MediaCaptureDeviceExclusiveControlStatusChangedEventArgs args)
        {
            if (args.Status == MediaCaptureDeviceExclusiveControlStatus.SharedReadOnlyAvailable)
            {
                var dialog = new MessageDialog("The camera preview can't be displayed because another app has exclusive access", "UnauthorizedAccessException");
                var result = await dialog.ShowAsync();

            }
            else if (args.Status == MediaCaptureDeviceExclusiveControlStatus.ExclusiveControlAvailable && !isPreviewing)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await StartPreviewAsync();
                });
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // combo box for camera selection
            // show camera list in here
            PreviewCamera();

        }

        public async Task<List<string>> GetVideoProfileSupportedDeviceIdAsync()
        {
            string deviceId = string.Empty;
            var cameras = new List<string>();

            // Finds all video capture devices
            DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            foreach (var device in devices)
            {
                //Debug.WriteLine("hello");

                // Check if the device Video Profile
                if (MediaCapture.IsVideoProfileSupported(device.Id))
                {
                    // locate a device that supports Video Profiles on expected panel
                    deviceId = device.Id;
                    cameras.Add(deviceId);
                    // break;
                }
            }
            return cameras;
        }

        private async void SaveSoftwareBitmapToFile(SoftwareBitmap softwareBitmap, StorageFile outputFile)
        {
            using (IRandomAccessStream stream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                // Create an encoder with the desired format
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);

                // Set the software bitmap
                encoder.SetSoftwareBitmap(softwareBitmap);

                try
                {
                    await encoder.FlushAsync();
                    encoder.IsThumbnailGenerated = true;
                }
                catch (Exception err)
                {
                    switch (err.HResult)
                    {
                        case unchecked((int)0x88982F81): //WINCODEC_ERR_UNSUPPORTEDOPERATION
                                                         // If the encoder does not support writing a thumbnail, then try again
                                                         // but disable thumbnail generation.
                            encoder.IsThumbnailGenerated = false;
                            break;
                        default:
                            throw err;
                    }
                }

                if (encoder.IsThumbnailGenerated == false)
                {
                    await encoder.FlushAsync();
                }


            }
        }

        private async Task GetPreviewFrameAsSoftwareBitmapAsync()
        {
            var cameraState = mediaCapture.CameraStreamState.ToString();

            if (string.Equals(cameraState, "Streaming"))
            {

                // Get information about the preview
                var previewProperties = mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;

                // Create the video frame to request a SoftwareBitmap preview frame
                var videoFrame = new VideoFrame(BitmapPixelFormat.Bgra8, (int)previewProperties.Width, (int)previewProperties.Height);

                // Capture the preview frame
                using (var currentFrame = await mediaCapture.GetPreviewFrameAsync(videoFrame))
                {
                    // Collect the resulting frame
                    SoftwareBitmap previewFrame = currentFrame.SoftwareBitmap;

                    // Setup filepath and file to test bitmap
                    var folderPath = Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Assets");
                    StorageFolder localfolder = await StorageFolder.GetFolderFromPathAsync(folderPath);
                    var file = await localfolder.CreateFileAsync("PreviewFrame.jpg", CreationCollisionOption.GenerateUniqueName);

                    // Save the captured frame to a file
                    SaveSoftwareBitmapToFile(previewFrame, file);
                }

                await GetPreviewFrameAsSoftwareBitmapAsync();
            }
        }

    }
}
