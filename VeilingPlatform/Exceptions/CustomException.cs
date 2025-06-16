using Microsoft.AspNetCore.Mvc;

namespace VeilingPlatform.Exceptions
{
    public class CustomException : Exception
    {
        public CustomException(string message) : base(message)
        {
        }
    }
}
