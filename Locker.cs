using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MfcAdmin.Api.Common.Providers.Lock
{
    /// <summary>
    /// Lock object provider
    /// </summary>
    public static class Locker
    {
        /// <summary>
        /// Collection of locks
        /// </summary>
        private static readonly Dictionary<object, LockObject> _lockers = new();

        /// <summary>
        /// Emit lock object
        /// </summary>
        /// <param name="lockKey">Control lock key</param>
        /// <param name="timeout">Lock timeout</param>
        /// <returns>Lock object</returns>
        public static async Task<IDisposable> LockAsync(object lockKey, TimeSpan? timeout)
        {
            if (!_lockers.TryGetValue(lockKey, out var lockObject))
            {
                lock (_lockers)
                {
                    if (!_lockers.TryGetValue(lockKey, out lockObject))
                    {
                        lockObject = new LockObject(lockObj => LockDispose(lockKey, lockObj));

                        _lockers.Add(lockKey, lockObject);
                    }
                }
            }

            Interlocked.Increment(ref lockObject.ReferenceCount);

            var enteredSemaphore = await lockObject.Semaphore.WaitAsync((int?)timeout?.TotalMilliseconds ?? -1);

            if (!enteredSemaphore)
            {
                throw new LockTimeoutException($"Too many requests. Lock timeout has been occured");
            }

            return lockObject;
        }

        /// <summary>
        /// Method to release resources
        /// </summary>
        /// <param name="lockKey">Control lock key</param>
        /// <param name="lockObject">Lock object</param>
        private static void LockDispose(object lockKey, LockObject lockObject)
        {
            lockObject.Semaphore.Release();

            Interlocked.Decrement(ref lockObject.ReferenceCount);

            if (lockObject.ReferenceCount == 0)
            {
                lock (_lockers)
                {
                    if (lockObject.ReferenceCount == 0)
                    {
                        _lockers.Remove(lockKey);

                        lockObject.Semaphore.Dispose();
                    }
                }
            }
        }
    }
}
