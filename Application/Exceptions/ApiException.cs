using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Exceptions
{
    public class ApiException : Exception
    {
        public int StatusCode { get; set; } = 400;
        public object? Errors { get; set; }

        public ApiException() : base("An unexpected error occurred.") { }

        public ApiException(string message) : base(message) { }

        public ApiException(string message, Exception inner) : base(message, inner) { }

        public ApiException(string message, object errors) : base(message)
        {
            Errors = errors;
        }

        public ApiException(string message, int statusCode, object errors = null) : base(message)
        {
            StatusCode = statusCode;
            Errors = errors;
        }
    }
}
