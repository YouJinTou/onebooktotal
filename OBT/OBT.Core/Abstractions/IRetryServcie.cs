using System;
using System.Threading.Tasks;

namespace OBT.Core.Abstractions
{
    public interface IRetryServcie
    {
        void EnqueueTask(Func<Task> failedTask);

        Task RetryAsync(Func<Task> call, int retryCount = 5);
    }
}
