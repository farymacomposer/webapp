namespace Faryma.Composer.Core.Utils
{
    public sealed class SemaphoreLocker
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public async Task Lock(Func<Task> action)
        {
            await _semaphore.WaitAsync();
            try
            {
                await action();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}