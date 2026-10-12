using System;
using System.Threading.Tasks;
using FluentAssertions;
using GtMotive.Estimate.Microservice.Api.Models;
using GtMotive.Estimate.Microservice.Api.UseCases.CreateVehicle;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using GtMotive.Estimate.Microservice.FunctionalTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GtMotive.Estimate.Microservice.FunctionalTests.Specs
{
    public sealed class CreateVehicleSpecs(CompositionRootTestFixture fixture) : FunctionalTestBase(fixture)
    {
        private static readonly DateOnly OneYearAgo = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-1);

        [Fact]
        public async Task Creating_a_vehicle_stores_it_and_returns_201()
        {
            var result = await Send(new CreateVehicleRequest { LicensePlate = "1234 abc", Brand = "Seat", Model = "Leon", ManufacturingDate = OneYearAgo });

            var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
            created.RouteName.Should().Be("GetVehicle");
            var response = created.Value.Should().BeOfType<VehicleResponse>().Subject;
            response.LicensePlate.Should().Be("1234ABC");

            Vehicle stored = null;
            await Fixture.UsingRepository<IVehicleRepository>(async repository => stored = await repository.GetById(VehicleId.From(response.Id)));

            stored.Should().NotBeNull();
            stored.LicensePlate.ToString().Should().Be("1234ABC");
            stored.ManufacturingDate.ToDateTime().Should().Be(OneYearAgo.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        }

        [Fact]
        public async Task Creating_a_vehicle_with_a_registered_license_plate_returns_409()
        {
            await Send(new CreateVehicleRequest { LicensePlate = "1234ABC", Brand = "Seat", Model = "Leon", ManufacturingDate = OneYearAgo });

            var result = await Send(new CreateVehicleRequest { LicensePlate = "1234-abc", Brand = "Seat", Model = "Ibiza", ManufacturingDate = OneYearAgo });

            result.Should().BeOfType<ConflictObjectResult>();
        }

        [Fact]
        public async Task Creating_a_vehicle_older_than_five_years_returns_422_and_stores_nothing()
        {
            var result = await Send(new CreateVehicleRequest { LicensePlate = "9999ZZZ", Brand = "Seat", Model = "Toledo", ManufacturingDate = OneYearAgo.AddYears(-5) });

            result.Should().BeOfType<UnprocessableEntityObjectResult>();

            var exists = true;
            await Fixture.UsingRepository<IVehicleRepository>(async repository => exists = await repository.ExistsByLicensePlate(new LicensePlate("9999ZZZ")));
            exists.Should().BeFalse();
        }
    }
}
