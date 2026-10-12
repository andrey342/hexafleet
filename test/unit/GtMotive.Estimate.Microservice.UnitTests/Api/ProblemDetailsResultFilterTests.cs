using FluentAssertions;
using GtMotive.Estimate.Microservice.Api.Filters;
using GtMotive.Estimate.Microservice.Api.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.Api
{
    public sealed class ProblemDetailsResultFilterTests
    {
        [Fact]
        public void Adds_the_request_path_and_the_trace_id_to_presenter_errors()
        {
            var context = Context(ProblemResults.Conflict("The vehicle is already rented."));

            new ProblemDetailsResultFilter().OnResultExecuting(context);

            var problem = (ProblemDetails)((ObjectResult)context.Result).Value;
            problem.Instance.Should().Be("/api/rentals");
            problem.Extensions.Should().ContainKey("traceId").WhoseValue.Should().Be("trace-1");
        }

        [Fact]
        public void Keeps_the_values_already_set()
        {
            var problem = new ProblemDetails { Instance = "/original" };
            problem.Extensions["traceId"] = "original-trace";
            var context = Context(new BadRequestObjectResult(problem));

            new ProblemDetailsResultFilter().OnResultExecuting(context);

            problem.Instance.Should().Be("/original");
            problem.Extensions["traceId"].Should().Be("original-trace");
        }

        private static ResultExecutingContext Context(IActionResult result)
        {
            var httpContext = new DefaultHttpContext { TraceIdentifier = "trace-1" };
            httpContext.Request.Path = "/api/rentals";

            return new ResultExecutingContext(
                new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
                [],
                result,
                controller: null);
        }
    }
}
