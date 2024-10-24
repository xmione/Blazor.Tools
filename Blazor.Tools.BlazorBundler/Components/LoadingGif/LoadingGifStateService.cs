/*====================================================================================================
    Class Name  : LSS.cs
    Created By  : Solomio S. Sisante
    Created On  : October 15, 2024
    Purpose     : To handle the LoadingGif component state service.
  ====================================================================================================*/
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

        public async Task RunTask(string loaderName, string message, Func<Task> longRunningTask)
        {
            StartLoading(loaderName, message);

            try
            {
                await longRunningTask();
            }
            finally
            {
                EndLoading(loaderName);
            }
        }

        public void StartLoading(string loaderName, string message = "Loading...")
        {
            if (!_loadingCounts.ContainsKey(loaderName))
            {
                _loadingCounts[loaderName] = 0;
            }

            _loadingCounts[loaderName]++;
            LoadingMessage = message;
        }

        public void EndLoading(string loaderName)
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
        }

        public bool IsLoading => _loadingCounts.Values.Any(count => count > 0);
    }
}

