using System;
using System.Threading.Tasks;

namespace OBT.Core.Models
{
    public class Retriable
    {
        public const int DefaultMaxAttempts = 5;

        private readonly int maxAttempts;
        private readonly Func<Task> failedTask;
        private int attemptsCount;

        public Retriable(Func<Task> failedTask, int maxAttempts = DefaultMaxAttempts)
        {
            this.maxAttempts = (maxAttempts <= 0) ? DefaultMaxAttempts : maxAttempts;
            this.failedTask = failedTask ?? throw new ArgumentNullException(nameof(failedTask));
            this.attemptsCount = 0;
        }

        public async Task<bool> RetryAsync()
        {
            if (this.attemptsCount <= this.maxAttempts)
            {
                try
                {
                    await this.failedTask();
                }
                catch
                {
                    this.attemptsCount++;

                    return false;
                }
            }

            return true;
        }
    }
}
