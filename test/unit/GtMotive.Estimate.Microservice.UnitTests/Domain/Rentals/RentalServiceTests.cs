using System;
using System.Threading.Tasks;
using FluentAssertions;
using GtMotive.Estimate.Microservice.Domain.Rentals;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using Moq;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.Domain.Rentals
{
    public sealed class RentalServiceTests
    {
        private static readonly DateTime Now = new(2026, 10, 10, 9, 30, 0, DateTimeKind.Utc);
        private static readonly CustomerId Customer = new("12345678A");

        private readonly Mock<IRentalFactory> _rentalFactory = new();
        private readonly Mock<IRentalRepository> _rentalRepository = new();
        private readonly RentalService _rentalService;

        public RentalServiceTests()
        {
            _rentalService = new RentalService(_rentalFactory.Object, _rentalRepository.Object);
        }

        [Fact]
        public async Task Rent_adds_an_active_rental_when_vehicle_and_customer_are_free()
        {
            var vehicle = AVehicle(new DateTime(2023, 5, 1, 0, 0, 0, DateTimeKind.Utc));
            var rental = new Rental(RentalId.New(), vehicle.Id, Customer, Now, null);
            _rentalFactory.Setup(f => f.NewRental(vehicle.Id, Customer, Now)).Returns(rental);

            var result = await _rentalService.Rent(vehicle, Customer, Now);

            result.Should().BeSameAs(rental);
            _rentalRepository.Verify(r => r.Add(rental), Times.Once);
        }

        [Fact]
        public async Task Rent_rejects_a_customer_who_already_has_an_active_rental()
        {
            var vehicle = AVehicle(new DateTime(2023, 5, 1, 0, 0, 0, DateTimeKind.Utc));
            var activeRental = new Rental(RentalId.New(), VehicleId.New(), Customer, Now.AddDays(-2), null);
            _rentalRepository.Setup(r => r.GetActiveByCustomer(Customer)).ReturnsAsync(activeRental);

            var act = () => _rentalService.Rent(vehicle, Customer, Now);

            await act.Should().ThrowAsync<CustomerAlreadyHasRentalException>();
            _rentalRepository.Verify(r => r.Add(It.IsAny<Rental>()), Times.Never);
        }

        [Fact]
        public async Task Rent_rejects_a_vehicle_that_is_already_rented()
        {
            var vehicle = AVehicle(new DateTime(2023, 5, 1, 0, 0, 0, DateTimeKind.Utc));
            var activeRental = new Rental(RentalId.New(), vehicle.Id, new CustomerId("87654321B"), Now.AddDays(-2), null);
            _rentalRepository.Setup(r => r.GetActiveByVehicle(vehicle.Id)).ReturnsAsync(activeRental);

            var act = () => _rentalService.Rent(vehicle, Customer, Now);

            await act.Should().ThrowAsync<VehicleNotAvailableException>();
            _rentalRepository.Verify(r => r.Add(It.IsAny<Rental>()), Times.Never);
        }

        [Fact]
        public async Task Rent_rejects_a_vehicle_older_than_the_fleet_allows()
        {
            var vehicle = AVehicle(new DateTime(2021, 10, 9, 0, 0, 0, DateTimeKind.Utc));

            var act = () => _rentalService.Rent(vehicle, Customer, Now);

            await act.Should().ThrowAsync<VehicleTooOldException>();
            _rentalRepository.Verify(r => r.Add(It.IsAny<Rental>()), Times.Never);
        }

        private static Vehicle AVehicle(DateTime manufacturingDate)
        {
            return new Vehicle(VehicleId.New(), new LicensePlate("1234ABC"), "Seat", "Leon", new ManufacturingDate(manufacturingDate));
        }
    }
}
