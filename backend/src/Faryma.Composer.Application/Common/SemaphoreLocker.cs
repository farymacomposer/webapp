namespace Faryma.Composer.Application.Common
{
    public sealed class SemaphoreLocker : IDisposable
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public void Dispose() => _semaphore.Dispose();

        public async Task<T> Lock<T>(Func<T> action)
        {
            await _semaphore.WaitAsync();
            try
            {
                return action();
            }
            finally
            {
                _semaphore.Release();
            }
        }

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
