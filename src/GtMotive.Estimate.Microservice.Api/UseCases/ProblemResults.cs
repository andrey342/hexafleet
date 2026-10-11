using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GtMotive.Estimate.Microservice.Api.UseCases
{
    /// <summary>
    /// Error results with a <see cref="ProblemDetails"/> body, in the same format as the BusinessExceptionFilter.
    /// </summary>
    public static class ProblemResults
    {
        public static IActionResult NotFound(string detail)
        {
            return new NotFoundObjectResult(new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = detail,
            });
        }

        public static IActionResult Conflict(string detail)
        {
            return new ConflictObjectResult(new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = detail,
            });
        }

        public static IActionResult UnprocessableEntity(string detail)
        {
            return new UnprocessableEntityObjectResult(new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Unprocessable Entity",
                Detail = detail,
            });
        }
    }
}
