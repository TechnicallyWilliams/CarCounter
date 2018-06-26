using System.Threading.Tasks;
using Windows.Graphics.Imaging;

namespace Vision
{
    public interface IItemCounter
    {
        Task<int> CountItems(SoftwareBitmap frame);
    }
}
