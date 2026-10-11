using System.Collections.Generic;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Vehicles;

namespace GtMotive.Estimate.Microservice.Domain.Rentals
{
    /// <summary>
    /// Persistence of the <see cref="Rental"/> aggregate.
    /// </summary>
    public interface IRentalRepository
    {
        /// <summary>
        /// Adds a rental.
        /// </summary>
        /// <param name="rental">Rental to add.</param>
        /// <returns>Task.</returns>
        /// <exception cref="VehicleNotAvailableException">The vehicle already has an active rental.</exception>
        /// <exception cref="CustomerAlreadyHasRentalException">The customer already has an active rental.</exception>
        Task Add(Rental rental);

        /// <summary>
        /// Gets a rental by its identifier.
        /// </summary>
        /// <param name="rentalId">Rental identifier.</param>
        /// <returns>The rental, or null when it does not exist.</returns>
        Task<Rental> GetById(RentalId rentalId);

        /// <summary>
        /// Gets the active rental of a customer.
        /// </summary>
        /// <param name="customerId">Customer identifier.</param>
        /// <returns>The active rental, or null when the customer has none.</returns>
        Task<Rental> GetActiveByCustomer(CustomerId customerId);

        /// <summary>
        /// Gets the active rental of a vehicle.
        /// </summary>
        /// <param name="vehicleId">Vehicle identifier.</param>
        /// <returns>The active rental, or null when the vehicle is not rented.</returns>
        Task<Rental> GetActiveByVehicle(VehicleId vehicleId);

        /// <summary>
        /// Gets the vehicles that have an active rental.
        /// </summary>
        /// <returns>The identifiers of the rented vehicles.</returns>
        Task<IReadOnlyCollection<VehicleId>> GetActiveVehicleIds();

        /// <summary>
        /// Saves the changes of an existing rental.
        /// </summary>
        /// <param name="rental">Rental to update.</param>
        /// <returns>Task.</returns>
        Task Update(Rental rental);
    }
}
