using System;
using GtMotive.Estimate.Microservice.Domain.Vehicles;

namespace GtMotive.Estimate.Microservice.Domain.Rentals
{
    /// <summary>
    /// Creates <see cref="Rental"/> instances.
    /// </summary>
    public interface IRentalFactory
    {
        /// <summary>
        /// Creates an active rental with a new identifier.
        /// </summary>
        /// <param name="vehicleId">Vehicle to rent.</param>
        /// <param name="customerId">Customer who rents the vehicle.</param>
        /// <param name="now">Current UTC date and time, used as the start of the rental.</param>
        /// <returns>The new rental.</returns>
        Rental NewRental(VehicleId vehicleId, CustomerId customerId, DateTime now);
    }
}
