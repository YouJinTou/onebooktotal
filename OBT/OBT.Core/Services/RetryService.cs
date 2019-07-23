using OBT.Core.Abstractions;
using OBT.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace OBT.Core.Services
{
    public class RetryService : IRetryServcie
    {
        private readonly BlockingCollection<Retriable> retriables;

        public RetryService()
        {
            this.retriables = new BlockingCollection<Retriable>();
            var backgroundTask = new Task(
                async () => await RetryAsync(), TaskCreationOptions.LongRunning);

            backgroundTask.Start();
        }

        public void EnqueueTask(Func<Task> failedTask)
        {
            var retriable = new Retriable(failedTask);

            this.retriables.Add(retriable);
        }

        public async Task RetryAsync(Func<Task> call, int retryCount = 5)
        {
            while (true)
            {
                try
                {
                    await Task.Run(call);

                    return;
                }
                catch when (retryCount-- > 0)
                {
                    Console.WriteLine("Retries left: " + retryCount);
                }
            }
        }

        private async Task RetryAsync()
        {
            while (true)
            {
                var retriable = this.retriables.Take();

                if (!await retriable.RetryAsync())
                {
                    this.retriables.Add(retriable);
                }
            }
        }
    }
}
