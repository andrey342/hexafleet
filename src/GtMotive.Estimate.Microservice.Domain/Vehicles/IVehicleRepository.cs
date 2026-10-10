using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GtMotive.Estimate.Microservice.Domain.Vehicles
{
    /// <summary>
    /// Persistence of the <see cref="Vehicle"/> aggregate.
    /// </summary>
    public interface IVehicleRepository
    {
        /// <summary>
        /// Adds a vehicle to the fleet.
        /// </summary>
        /// <param name="vehicle">Vehicle to add.</param>
        /// <returns>Task.</returns>
        /// <exception cref="LicensePlateAlreadyRegisteredException">Another vehicle has the same license plate.</exception>
        Task Add(Vehicle vehicle);

        /// <summary>
        /// Gets a vehicle by its identifier.
        /// </summary>
        /// <param name="vehicleId">Vehicle identifier.</param>
        /// <returns>The vehicle, or null when it does not exist.</returns>
        Task<Vehicle> GetById(VehicleId vehicleId);

        /// <summary>
        /// Checks whether a vehicle with the given license plate exists.
        /// </summary>
        /// <param name="licensePlate">License plate.</param>
        /// <returns>True when the license plate is already registered.</returns>
        Task<bool> ExistsByLicensePlate(LicensePlate licensePlate);

        /// <summary>
        /// Gets the vehicles manufactured on or after a date, excluding the given ones.
        /// </summary>
        /// <param name="oldestAllowedManufacturingDate">Oldest manufacturing date to include.</param>
        /// <param name="rentedVehicleIds">Vehicles to exclude because they are rented.</param>
        /// <returns>The available vehicles.</returns>
        Task<IReadOnlyCollection<Vehicle>> FindAvailable(
            DateTime oldestAllowedManufacturingDate,
            IReadOnlyCollection<VehicleId> rentedVehicleIds);
    }
}
