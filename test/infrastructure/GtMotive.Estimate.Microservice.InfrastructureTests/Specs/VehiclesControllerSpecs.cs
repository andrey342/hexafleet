using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using GtMotive.Estimate.Microservice.InfrastructureTests.Fakes;
using GtMotive.Estimate.Microservice.InfrastructureTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GtMotive.Estimate.Microservice.InfrastructureTests.Specs
{
    public sealed class VehiclesControllerSpecs(GenericInfrastructureTestServerFixture fixture) : InfrastructureTestBase(fixture)
    {
        private static readonly Uri VehiclesUri = new("api/vehicles", UriKind.Relative);

        [Fact]
        public async Task Post_returns_400_when_the_body_is_empty()
        {
            using var response = await Post(string.Empty);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            problem.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Post_returns_400_when_the_license_plate_is_missing()
        {
            using var response = await Post("""{ "brand": "Seat", "model": "Leon", "manufacturingDate": "2024-05-01" }""");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            problem.Errors.Should().ContainKey("LicensePlate");
            problem.Instance.Should().Be("/api/vehicles");
            problem.Extensions.Should().ContainKey("traceId");
        }

        [Fact]
        public async Task Post_returns_400_when_the_manufacturing_date_is_not_a_date()
        {
            using var response = await Post("""{ "licensePlate": "1234ABC", "brand": "Seat", "model": "Leon", "manufacturingDate": "not-a-date" }""");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            problem.Errors.Should().ContainKey("$.manufacturingDate");
        }

        [Fact]
        public async Task Post_returns_201_when_the_model_is_valid()
        {
            using var response = await Post("""{ "licensePlate": "1234 abc", "brand": "Seat", "model": "Leon", "manufacturingDate": "2024-05-01" }""");

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
            response.Headers.Location.AbsolutePath.Should().Be($"/api/vehicles/{CreateVehicleRequestStubHandler.CreatedVehicleId}");
        }

        private async Task<HttpResponseMessage> Post([StringSyntax(StringSyntaxAttribute.Json)] string json)
        {
            using var client = Fixture.Server.CreateClient();
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            return await client.PostAsync(VehiclesUri, content);
        }
    }
}
