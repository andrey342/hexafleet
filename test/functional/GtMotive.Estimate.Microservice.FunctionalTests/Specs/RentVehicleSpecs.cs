using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GtMotive.Estimate.Microservice.Api.Models;
using GtMotive.Estimate.Microservice.Api.UseCases.CreateVehicle;
using GtMotive.Estimate.Microservice.Api.UseCases.ListAvailableVehicles;
using GtMotive.Estimate.Microservice.Api.UseCases.RentVehicle;
using GtMotive.Estimate.Microservice.Api.UseCases.ReturnVehicle;
using GtMotive.Estimate.Microservice.Domain.Rentals;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using GtMotive.Estimate.Microservice.FunctionalTests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GtMotive.Estimate.Microservice.FunctionalTests.Specs
{
    public sealed class RentVehicleSpecs(CompositionRootTestFixture fixture) : FunctionalTestBase(fixture)
    {
        private const string Customer = "12345678A";

        [Fact]
        public async Task Renting_a_vehicle_creates_an_active_rental_and_returns_201()
        {
            var vehicleId = await CreateVehicle("1234ABC");

            var result = await Send(new RentVehicleRequest { VehicleId = vehicleId, CustomerId = "12345678-a" });

            var created = result.Should().BeOfType<ObjectResult>().Subject;
            created.StatusCode.Should().Be(StatusCodes.Status201Created);
            created.Value.Should().BeOfType<RentalResponse>().Which.CustomerId.Should().Be(Customer);

            Rental active = null;
            await Fixture.UsingRepository<IRentalRepository>(async repository => active = await repository.GetActiveByCustomer(new CustomerId(Customer)));

            active.Should().NotBeNull();
            active.VehicleId.Should().Be(VehicleId.From(vehicleId));
        }

        [Fact]
        public async Task Renting_a_second_vehicle_while_the_customer_has_an_active_rental_returns_409()
        {
            var first = await CreateVehicle("1234ABC");
            var second = await CreateVehicle("5678DEF");
            await Send(new RentVehicleRequest { VehicleId = first, CustomerId = Customer });

            var result = await Send(new RentVehicleRequest { VehicleId = second, CustomerId = Customer });

            result.Should().BeOfType<ConflictObjectResult>();
        }

        [Fact]
        public async Task The_database_rejects_a_second_active_rental_for_the_same_customer()
        {
            var first = await CreateVehicle("1234ABC");
            var second = await CreateVehicle("5678DEF");
            await Send(new RentVehicleRequest { VehicleId = first, CustomerId = Customer });

            await Fixture.UsingRepository<IRentalRepository>(async repository =>
            {
                var act = () => repository.Add(new Rental(RentalId.New(), VehicleId.From(second), new CustomerId(Customer), DateTime.UtcNow, null));

                await act.Should().ThrowAsync<CustomerAlreadyHasRentalException>();
            });
        }

        [Fact]
        public async Task Concurrent_rentals_for_the_same_customer_create_one_rental_and_return_409_to_the_rest()
        {
            var vehicleIds = new List<Guid>();
            for (var i = 0; i < 10; i++)
            {
                vehicleIds.Add(await CreateVehicle($"CONC{i:000}"));
            }

            var results = await Task.WhenAll(vehicleIds.Select(vehicleId => Send(new RentVehicleRequest { VehicleId = vehicleId, CustomerId = Customer })));

            results.Count(result => result is ObjectResult { StatusCode: StatusCodes.Status201Created }).Should().Be(1);
            results.Count(result => result is ConflictObjectResult).Should().Be(vehicleIds.Count - 1);

            Rental active = null;
            await Fixture.UsingRepository<IRentalRepository>(async repository => active = await repository.GetActiveByCustomer(new CustomerId(Customer)));
            active.Should().NotBeNull();
        }

        [Fact]
        public async Task Returning_a_vehicle_closes_the_rental_and_makes_the_vehicle_available_again()
        {
            var vehicleId = await CreateVehicle("1234ABC");
            var rented = await Send(new RentVehicleRequest { VehicleId = vehicleId, CustomerId = Customer });
            var rentalId = ((RentalResponse)((ObjectResult)rented).Value).Id;
            (await AvailableVehicleIds()).Should().NotContain(vehicleId);

            var result = await Send(new ReturnVehicleRequest(rentalId));

            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeOfType<RentalResponse>()
                .Which.ReturnedAt.Should().NotBeNull();
            (await AvailableVehicleIds()).Should().Contain(vehicleId);
        }

        [Fact]
        public async Task Returning_a_vehicle_twice_returns_409()
        {
            var vehicleId = await CreateVehicle("1234ABC");
            var rented = await Send(new RentVehicleRequest { VehicleId = vehicleId, CustomerId = Customer });
            var rentalId = ((RentalResponse)((ObjectResult)rented).Value).Id;
            await Send(new ReturnVehicleRequest(rentalId));

            var result = await Send(new ReturnVehicleRequest(rentalId));

            result.Should().BeOfType<ConflictObjectResult>();
        }

        private async Task<Guid> CreateVehicle(string licensePlate)
        {
            var result = await Send(new CreateVehicleRequest
            {
                LicensePlate = licensePlate,
                Brand = "Seat",
                Model = "Leon",
                ManufacturingDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-1),
            });

            return ((VehicleResponse)((CreatedAtRouteResult)result).Value).Id;
        }

        private async Task<IEnumerable<Guid>> AvailableVehicleIds()
        {
            var result = await Send(new ListAvailableVehiclesRequest());
            var vehicles = (IEnumerable<VehicleResponse>)((OkObjectResult)result).Value;

            return vehicles.Select(vehicle => vehicle.Id);
        }
    }
}
