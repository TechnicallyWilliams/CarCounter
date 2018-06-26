using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.Media.Capture;
using Windows.System.Display;
using Windows.Graphics.Display;
using Windows.UI.Popups;
using Windows.UI.Core;
using Windows.Devices.Enumeration;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Media.MediaProperties;
using Windows.Media;
using System.Linq;
using System.Diagnostics;



// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Vision
{
    public class CameraReference
    {
        public string Id { get; }
        public string Name { get; }

        public CameraReference(string id, string name)
        {
            this.Id = id;
            this.Name = name == null ? "Default" : name;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MediaCapture mediaCapture;
        public List<CameraReference> Cameras { get; set; }
        public IImageClassifier Classifier { get; set; }
        private ILabelCountDAO LabelCountDAO;
        public IItemCounter Counter { get; set; }

        public void ShowText(string msg)
        {

            lblName.Text = msg == null? "" : msg;
        }

        public MainPage()
        {
            this.InitializeComponent();
            this.Classifier = new HttpImageClassifier();
            this.LabelCountDAO = new LabelCountDAO();
            this.Counter = new ItemCounterImpl()
            {
                Classifier = this.Classifier
            };
            ShowText("0");
            LoadCameras();
        }
        

        public void LoadCameras()
        {
            cam.Items.Clear();
            DeviceInformation.FindAllAsync(DeviceClass.VideoCapture).AsTask().ContinueWith(deviceContinuation => {
                this.Cameras = deviceContinuation.Result.Select(c => new CameraReference(c.Id, c.Name)).ToList();
                return Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                    foreach (var c in this.Cameras)
                    {
                        cam.Items.Add(c.Name);
                    }
                    if (cam.Items.Count > 0)
                    {
                        cam.SelectedValue = cam.Items[0];
                    }
                });
            });
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CameraCaptureStart();
        }

        private void CameraCaptureStart()
        {
            var capture = new MediaCapture();
            var mediaSettings = new MediaCaptureInitializationSettings
            {
                VideoDeviceId = this.Cameras[this.cam.SelectedIndex].Id
            };
            capture.InitializeAsync(mediaSettings).AsTask().ContinueWith(initContinuation => {
                return Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                    this.PreviewControl.Source = capture;
                    capture.StartPreviewAsync().AsTask().ContinueWith(previewContinuation => {
                        this.mediaCapture = capture;
                        return QueueFrameCapture(capture);
                    });
                });
            });
        }

        private Task QueueFrameCapture(MediaCapture capture)
        {
            if (capture != this.mediaCapture)
            {
                return Task.FromCanceled(new System.Threading.CancellationToken(true));
            }

            var previewProperties = mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;
            var videoFrame = new VideoFrame(BitmapPixelFormat.Bgra8, (int)previewProperties.Width, (int)previewProperties.Height);
            return mediaCapture.GetPreviewFrameAsync(videoFrame).AsTask().ContinueWith(preview=> {
                var currentFrame = preview.Result;
                SoftwareBitmap previewFrame = currentFrame.SoftwareBitmap;
                return this.Counter.CountItems(previewFrame).ContinueWith(count => {
                    return Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                        if (count.IsFaulted)
                        {
                            Debug.WriteLine("Unable to count items in frame.");
                            return;
                        }
                        LabelCount record = new LabelCount();
                        record.TimeStamp = DateTime.UtcNow;
                        record.Label = "Car";
                        record.Count = count.Result;
                        this.LabelCountDAO.Save(record).ContinueWith(t => { Debug.WriteLine($"Complete: Image count of {count.Result} sent to DB."); });

                        this.lblName.Text = count.Result.ToString();

                    }).AsTask().ContinueWith(display=> {
                        return QueueFrameCapture(capture);
                    });
                });
            });
        }

    }
}
