using Microsoft.AspNetCore.Mvc;

namespace SWD.F_LocalBrand.API.Exceptions
{
    public class RequestValidationException : Exception
    {
        public ValidationProblemDetails ProblemDetails { get; set; }
        public RequestValidationException(ValidationProblemDetails details)
        {
            ProblemDetails = details;
        }
    }
}
