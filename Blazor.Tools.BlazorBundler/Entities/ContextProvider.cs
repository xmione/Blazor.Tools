using Blazor.Tools.BlazorBundler.Interfaces;

namespace Blazor.Tools.BlazorBundler.Entities
{
    public class ContextProvider : IContextProvider
    {
        private readonly IDictionary<string, object> _contexts;

        public ContextProvider()
        {
            _contexts = new Dictionary<string, object>();
        }

        public void SetContext<T>(string key, T context)
        {
            _contexts[key] = context ?? default!;
        }

        public T GetContext<T>(string key)
        {
            var contextObject = _contexts.TryGetValue(key, out var context) ? (T)context : default;
            return contextObject ?? default!;
        }
    }

}
