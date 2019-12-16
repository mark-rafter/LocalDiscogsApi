using System;
using System.Threading.Tasks;

namespace LocalDiscogsApi.Services
{
    public interface ITimerService
    {
        Task Delay(TimeSpan delay);
    }

    public class TimerService : ITimerService
    {
        public async Task Delay(TimeSpan delay) => await Task.Delay(delay);
    }
}