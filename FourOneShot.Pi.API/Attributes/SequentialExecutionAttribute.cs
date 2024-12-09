using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading;
using System.Threading.Tasks;

namespace FourOneShot.Pi.API.Attributes
{
    /// <summary>
    /// Forces tagged actions to be executed sequentially. Terrible for scalability/performance, 
    /// but great for interacting with physical hardware that can't tolerate concurrent usage.
    /// </summary>
    public class SequentialExecutionAttribute : ActionFilterAttribute
    {
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await _semaphore.WaitAsync();

            try
            {
                await next();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
