using System;
using FluentAssertions;
using GtMotive.Estimate.Microservice.Domain;
using GtMotive.Estimate.Microservice.Domain.Rentals;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.Domain.Rentals
{
    public sealed class RentalTests
    {
        private static readonly DateTime StartedAt = new(2026, 10, 1, 10, 0, 0, DateTimeKind.Utc);

        [Fact]
        public void A_new_rental_is_active()
        {
            var rental = ActiveRental();

            rental.IsActive.Should().BeTrue();
            rental.ReturnedAt.Should().BeNull();
        }

        [Fact]
        public void Return_closes_the_rental()
        {
            var rental = ActiveRental();
            var returnedAt = StartedAt.AddDays(3);

            rental.Return(returnedAt);

            rental.IsActive.Should().BeFalse();
            rental.ReturnedAt.Should().Be(returnedAt);
        }

        [Fact]
        public void Dates_are_truncated_to_milliseconds()
        {
            var subMillisecond = TimeSpan.FromTicks(1234);
            var rental = new Rental(RentalId.New(), VehicleId.New(), new CustomerId("12345678A"), StartedAt.AddMilliseconds(5).Add(subMillisecond), null);

            rental.Return(StartedAt.AddDays(1).Add(subMillisecond));

            rental.StartedAt.Should().Be(StartedAt.AddMilliseconds(5));
            rental.ReturnedAt.Should().Be(StartedAt.AddDays(1));
        }

        [Fact]
        public void Return_rejects_a_rental_that_was_already_returned()
        {
            var rental = ActiveRental();
            rental.Return(StartedAt.AddDays(3));

            var act = () => rental.Return(StartedAt.AddDays(4));

            act.Should().Throw<RentalAlreadyReturnedException>();
            rental.ReturnedAt.Should().Be(StartedAt.AddDays(3));
        }

        [Fact]
        public void Return_rejects_a_date_before_the_start_of_the_rental()
        {
            var rental = ActiveRental();

            var act = () => rental.Return(StartedAt.AddMinutes(-1));

            act.Should().Throw<DomainException>();
            rental.IsActive.Should().BeTrue();
        }

        [Fact]
        public void Customer_id_is_stored_in_upper_case_without_spaces_or_hyphens()
        {
            var customerId = new CustomerId("12345678-a");

            customerId.ToString().Should().Be("12345678A");
        }

        private static Rental ActiveRental()
        {
            return new Rental(RentalId.New(), VehicleId.New(), new CustomerId("12345678A"), StartedAt, null);
        }
    }
}
