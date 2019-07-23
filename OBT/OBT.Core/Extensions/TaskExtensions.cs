using System;
using System.Threading.Tasks;

namespace OBT.Core.Extensions
{
    public static class TaskExtensions
    {
        public static async Task RetryAsync(
            this Func<Task> func, int maxAttempts = 5, int delay = 50)
        {
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                try
                {
                    await func();

                    return;
                }
                catch
                {
                    Console.WriteLine("Attempt " + attempts);

                    await Task.Delay(delay);
                }
                finally
                {
                    attempts++;
                }
            }
        }
    }
}
