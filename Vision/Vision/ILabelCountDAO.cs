using System.Threading.Tasks;

namespace Vision
{
    public interface ILabelCountDAO
    {
        Task Save(LabelCount labelCount);
    }
}
