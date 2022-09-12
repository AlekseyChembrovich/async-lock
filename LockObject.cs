using System;
using System.Threading;

namespace MfcAdmin.Api.Common.Providers.Lock
{
    /// <summary>
    /// Wrap of semaphore
    /// </summary>
    internal class LockObject : IDisposable
    {
        /// <summary>
        /// Waiter count
        /// </summary>
        public volatile int ReferenceCount = 0;

        /// <summary>
        /// Wrapped semaphore
        /// </summary>
        public readonly SemaphoreSlim Semaphore;

        /// <summary>
        /// Delegate to release resources
        /// </summary>
        private readonly Action<LockObject> _disposer;

        public LockObject(Action<LockObject> disposer)
        {
            _disposer = disposer;
            Semaphore = new SemaphoreSlim(1, 1);
        }

        /// <summary>
        /// Release semaphore lock
        /// </summary>
        public void Dispose()
        {
            _disposer(this);
        }
    }
}
