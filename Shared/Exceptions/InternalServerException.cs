namespace Shared.Exceptions
{
 public class InternalServerException : Exception
    {
        public InternalServerException(string message = "INTERNAL_SERVER_ERROR")
            : base(message) { }
    }
}