using System;
using GtMotive.Estimate.Microservice.Domain.Vehicles;

namespace GtMotive.Estimate.Microservice.Domain.Rentals
{
    /// <summary>
    /// Rental of a vehicle by a customer. Aggregate root.
    /// </summary>
    public sealed class Rental
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Rental"/> class.
        /// </summary>
        /// <param name="id">Rental identifier.</param>
        /// <param name="vehicleId">Rented vehicle.</param>
        /// <param name="customerId">Customer who rents the vehicle.</param>
        /// <param name="startedAt">UTC date and time the rental started.</param>
        /// <param name="returnedAt">UTC date and time the vehicle was returned, or null while the rental is active.</param>
        public Rental(RentalId id, VehicleId vehicleId, CustomerId customerId, DateTime startedAt, DateTime? returnedAt)
        {
            if (returnedAt < startedAt)
            {
                throw new DomainException("The return date cannot be earlier than the start date.");
            }

            Id = id;
            VehicleId = vehicleId;
            CustomerId = customerId;
            StartedAt = startedAt;
            ReturnedAt = returnedAt;
        }

        /// <summary>
        /// Gets the rental identifier.
        /// </summary>
        public RentalId Id { get; }

        /// <summary>
        /// Gets the rented vehicle.
        /// </summary>
        public VehicleId VehicleId { get; }

        /// <summary>
        /// Gets the customer who rents the vehicle.
        /// </summary>
        public CustomerId CustomerId { get; }

        /// <summary>
        /// Gets the UTC date and time the rental started.
        /// </summary>
        public DateTime StartedAt { get; }

        /// <summary>
        /// Gets the UTC date and time the vehicle was returned, or null while the rental is active.
        /// </summary>
        public DateTime? ReturnedAt { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the vehicle has not been returned yet.
        /// </summary>
        public bool IsActive => ReturnedAt is null;

        /// <summary>
        /// Closes the rental.
        /// </summary>
        /// <param name="now">Current UTC date and time.</param>
        /// <exception cref="RentalAlreadyReturnedException">The rental is already closed.</exception>
        public void Return(DateTime now)
        {
            if (!IsActive)
            {
                throw new RentalAlreadyReturnedException($"The rental {Id} has already been returned.");
            }

            if (now < StartedAt)
            {
                throw new DomainException("The return date cannot be earlier than the start date.");
            }

            ReturnedAt = now;
        }
    }
}
