using System.Threading.Tasks;

namespace Washer.ShakeHandling
{
    public interface IShakeHandler
    {
        Task Trigger();
    }
}