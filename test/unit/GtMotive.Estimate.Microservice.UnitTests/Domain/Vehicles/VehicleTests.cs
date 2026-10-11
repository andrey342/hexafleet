using System;
using FluentAssertions;
using GtMotive.Estimate.Microservice.Domain;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.Domain.Vehicles
{
    public sealed class VehicleTests
    {
        private static readonly DateTime Now = new(2026, 10, 10, 9, 30, 0, DateTimeKind.Utc);

        [Fact]
        public void Create_accepts_a_vehicle_manufactured_exactly_five_years_ago()
        {
            var manufacturingDate = new DateTime(2021, 10, 10, 0, 0, 0, DateTimeKind.Utc);

            var vehicle = CreateVehicle(manufacturingDate);

            vehicle.ManufacturingDate.ToDateTime().Should().Be(manufacturingDate);
        }

        [Fact]
        public void Create_rejects_a_vehicle_manufactured_five_years_and_one_day_ago()
        {
            var act = () => CreateVehicle(new DateTime(2021, 10, 9, 0, 0, 0, DateTimeKind.Utc));

            act.Should().Throw<VehicleTooOldException>();
        }

        [Fact]
        public void Create_rejects_a_manufacturing_date_in_the_future()
        {
            var act = () => CreateVehicle(new DateTime(2026, 10, 11, 0, 0, 0, DateTimeKind.Utc));

            act.Should().Throw<DomainException>()
                .WithMessage("The manufacturing date cannot be in the future.");
        }

        [Fact]
        public void Create_accepts_a_vehicle_manufactured_today()
        {
            var vehicle = CreateVehicle(Now);

            vehicle.ManufacturingDate.ToDateTime().Should().Be(Now.Date);
        }

        [Theory]
        [InlineData("1234 abc")]
        [InlineData("1234-ABC")]
        [InlineData(" 1234abc ")]
        public void License_plate_is_stored_in_upper_case_without_spaces_or_hyphens(string value)
        {
            var licensePlate = new LicensePlate(value);

            licensePlate.ToString().Should().Be("1234ABC");
        }

        [Theory]
        [InlineData("")]
        [InlineData("AB1")]
        [InlineData("12345678901")]
        [InlineData("1234#ABC")]
        public void License_plate_rejects_invalid_values(string value)
        {
            var act = () => new LicensePlate(value);

            act.Should().Throw<DomainException>();
        }

        private static Vehicle CreateVehicle(DateTime manufacturingDate)
        {
            return Vehicle.Create(
                VehicleId.New(),
                new LicensePlate("1234ABC"),
                "Seat",
                "Leon",
                new ManufacturingDate(manufacturingDate),
                Now);
        }
    }
}
