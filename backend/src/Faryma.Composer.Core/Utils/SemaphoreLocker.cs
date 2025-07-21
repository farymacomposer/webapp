namespace Faryma.Composer.Core.Utils
{
    public sealed class SemaphoreLocker
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public async Task<T> Lock<T>(Func<Task<T>> action)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await action();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task Lock(Action action)
        {
            await _semaphore.WaitAsync();
            try
            {
                action();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}