using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace Vision
{
    public class ItemCounterImpl : IItemCounter
    {
        public IImageClassifier Classifier { get; set; }

        public async Task<int> CountItems(SoftwareBitmap frame)
        {
            uint totWidth = (uint)frame.PixelWidth;
            uint totHeight = (uint)frame.PixelHeight;
            uint halfWidth = (uint)totWidth / 2;
            uint halfHeight = (uint)totHeight / 2;

            // process image divide into 4 quadrants
            var quadrants = new List<BitmapBounds>();
            quadrants.Add(new BitmapBounds()
            {
                X = 0,
                Y = 0,
                Width = halfWidth,
                Height = halfHeight
            });
            quadrants.Add(new BitmapBounds()
            {
                X = halfWidth,
                Y = 0,
                Width = halfWidth,
                Height = halfHeight
            });
            quadrants.Add(new BitmapBounds()
            {
                X = 0,
                Y = halfHeight,
                Width = halfWidth,
                Height = halfHeight
            });
            quadrants.Add(new BitmapBounds()
            {
                X = halfWidth,
                Y = halfHeight,
                Width = halfWidth,
                Height = halfHeight
            });

            var cropTasks = new List<Task<SoftwareBitmap>>();
            foreach (var quadrant in quadrants)
            {
                cropTasks.Add(CropToBounds(frame, quadrant));
            }

            var cropContinuation = Task.WhenAll(cropTasks);
            var classifyTasks = new List<Task<string>>();
            foreach (SoftwareBitmap image in cropContinuation.Result)
            {
                classifyTasks.Add(Classifier.Classify(image));
            }

            var classifyContinuation = Task.WhenAll(classifyTasks);
            int result = 0;
            foreach (string classification in classifyContinuation.Result)
            {
                result += classification == null ? 0 : 1;
            }
            return result;
        }

        private async Task<SoftwareBitmap> CropToBounds(SoftwareBitmap frame, BitmapBounds bounds)
        {
            using (IRandomAccessStream stream = new InMemoryRandomAccessStream())
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, stream);
                encoder.SetSoftwareBitmap(frame);
                encoder.BitmapTransform.Flip = BitmapFlip.None;
                encoder.BitmapTransform.Rotation = 0;
                encoder.BitmapTransform.ScaledHeight = (uint)frame.PixelHeight;
                encoder.BitmapTransform.ScaledWidth = (uint)frame.PixelWidth;
                encoder.BitmapTransform.Bounds = bounds;

                await encoder.FlushAsync();
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                return await decoder.GetSoftwareBitmapAsync(frame.BitmapPixelFormat, frame.BitmapAlphaMode);
            };
        }

    }
}
