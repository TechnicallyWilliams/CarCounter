using System.Threading.Tasks;
using Windows.Graphics.Imaging;

namespace Vision
{
    public interface IImageClassifier
    {
        Task<string> Classify(SoftwareBitmap image);
    }
}
