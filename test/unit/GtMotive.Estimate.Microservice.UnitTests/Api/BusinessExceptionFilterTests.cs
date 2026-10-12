using System;
using FluentAssertions;
using GtMotive.Estimate.Microservice.Api.Filters;
using GtMotive.Estimate.Microservice.Domain;
using GtMotive.Estimate.Microservice.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.Api
{
    public sealed class BusinessExceptionFilterTests
    {
        private readonly BusinessExceptionFilter _filter = new(Mock.Of<IAppLogger<BusinessExceptionFilter>>());

        [Fact]
        public void Returns_400_with_the_message_of_a_domain_exception()
        {
            var context = Context(new DomainException("The license plate is required."));

            _filter.OnException(context);

            var problem = context.Result.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().BeOfType<ProblemDetails>().Subject;
            problem.Detail.Should().Be("The license plate is required.");
        }

        [Fact]
        public void Returns_500_without_the_message_of_an_unexpected_exception()
        {
            var context = Context(new InvalidOperationException("Client view of cluster state is { mongo:27017 }"));

            _filter.OnException(context);

            var result = context.Result.Should().BeOfType<InternalServerErrorObjectResult>().Subject;
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            result.Value.Should().BeOfType<ProblemDetails>()
                .Which.Detail.Should().NotContain("mongo");
            context.Exception.Should().BeNull();
        }

        private static ExceptionContext Context(Exception exception)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/api/vehicles";

            return new ExceptionContext(
                new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
                [])
            {
                Exception = exception,
            };
        }
    }
}
