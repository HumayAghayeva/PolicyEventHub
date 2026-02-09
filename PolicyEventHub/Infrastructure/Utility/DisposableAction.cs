namespace PolicyEventHub.Infrastructure.Utility
{
    public class DisposableAction:IDisposable
    {
        public static readonly IDisposable Empty = new DisposableAction();
        private Action? _onDispose;

        private DisposableAction()
        {
            _onDispose = null;  
        }
        public DisposableAction(Action onDispose)
        {
            _onDispose = onDispose?? throw new ArgumentNullException(nameof(onDispose));
        }
        public void Dispose() { 
            _onDispose?.Invoke();
            _onDispose = null;
        }
    }
}
