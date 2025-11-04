namespace Faryma.Composer.Desktop.Utils
{
    public sealed class TaskAggregator<T>
    {
        private readonly List<Func<T, Task>> _tasks = [];

        public void AddTask(Func<T, Task> task) => _tasks.Add(task);
        public Task RunAll(T parameter) => Task.WhenAll(_tasks.Select(x => x(parameter)));
    }
}