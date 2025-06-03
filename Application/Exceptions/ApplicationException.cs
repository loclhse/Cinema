using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Application.Exceptions
{
    public class ApplicationException : Exception
    {
        public int StatusCode { get; }
        public string ErrorCode { get; }
        public ApplicationException(HttpStatusCode statusCode, string message) : base(message)
        {
            StatusCode = (int)statusCode;
        }
        public ApplicationException(HttpStatusCode statusCode, string errorCode, string message) : base(message)
        {
            StatusCode = (int)statusCode;
            ErrorCode = errorCode;
        }
        public ApplicationException(HttpStatusCode statusCode, string errorCode, string message, Exception innerException) : base(message, innerException)
        {
            StatusCode = (int)statusCode;
            ErrorCode = errorCode;
        }
    }
}
