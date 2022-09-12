using System;

namespace MfcAdmin.Api.Common.Providers.Lock
{
    /// <summary>
    /// Exception indicates waiting time expired 
    /// </summary>
    public class LockTimeoutException : Exception
    {
        public LockTimeoutException(string message) : base(message)
        {
        }
    }
}
