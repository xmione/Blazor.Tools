namespace Blazor.Tools.BlazorBundler.Components.LoadingGif
{
    public class LoadingGifStateService
    {
        private readonly Dictionary<string, int> _loadingCounts = new();
        private readonly List<Action> _subscribers = new();
        private string _loadingMessage = string.Empty;

        public string LoadingMessage
        {
            get => _loadingMessage;
            private set
            {
                _loadingMessage = value;
                Notify();
            }
        }

        public void Subscribe(Action callback)
        {
            if (!_subscribers.Contains(callback))
            {
                _subscribers.Add(callback);
            }
        }

        public void Unsubscribe(Action callback)
        {
            _subscribers.Remove(callback);
        }

        public void Notify()
        {
            foreach (var subscriber in _subscribers)
            {
                subscriber.Invoke();
            }
        }

        // Overload for methods with parameters
        public async Task RunTaskAsync<T>(string loaderName, string message, Func<T, Task> longRunningTask, T parameter)
        {
            await StartLoadingAsync(loaderName, message); // Start the loading state

            try
            {
                await longRunningTask(parameter); // Call the long-running task with the parameter
            }
            finally
            {
                await EndLoadingAsync(loaderName); // Ensure loading ends
            }
        }

        // Overload for methods without parameters
        public async Task RunTaskAsync(string loaderName, string message, Func<Task> longRunningTask)
        {
            await StartLoadingAsync(loaderName, message); // Start the loading state

            try
            {
                await longRunningTask(); // Call the long-running task without a parameter
            }
            finally
            {
                await EndLoadingAsync(loaderName); // Ensure loading ends
            }
        }

        public async Task StartLoadingAsync(string loaderName, string message = "Loading...")
        {
            if (!_loadingCounts.ContainsKey(loaderName))
            {
                _loadingCounts[loaderName] = 0;
            }

            _loadingCounts[loaderName]++;
            LoadingMessage = message;

            await Task.CompletedTask;
        }

        public async Task EndLoadingAsync(string loaderName)
        {
            if (_loadingCounts.ContainsKey(loaderName) && _loadingCounts[loaderName] > 0)
            {
                _loadingCounts[loaderName]--;

                if (_loadingCounts[loaderName] == 0)
                {
                    _loadingCounts.Remove(loaderName);
                    LoadingMessage = "Loading complete.";
                    Notify();
                }
            }

            await Task.CompletedTask;
        }

        public bool IsLoading => _loadingCounts.Values.Any(count => count > 0);
    }
}
