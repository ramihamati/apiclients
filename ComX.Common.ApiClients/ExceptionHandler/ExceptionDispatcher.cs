using System;

namespace ComX.Common.ApiClients.ExceptionHandler
{
    public sealed class ExceptionDispatcher
    {
        public Exception Exception { get; set; }

        public bool IsFaulted => Exception != null;

        public void Capture(Exception exception)
        {
            this.Exception = exception;
        }

        public void ThrowIfCaptured()
        {
            if (Exception != null)
            {
                throw new Exception("Exception rethrown", Exception);
            }
        }

        public ApiResult AsApiResult()
        {
            return ApiResult.Fault(Exception);
        }

        public ApiResult<T> AsApiResult<T>()
        {
            return ApiResult<T>.Fault(Exception);
        }
    }
}
