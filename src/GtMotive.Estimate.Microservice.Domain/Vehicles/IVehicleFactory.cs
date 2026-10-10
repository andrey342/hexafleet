using System;

namespace GtMotive.Estimate.Microservice.Domain.Vehicles
{
    /// <summary>
    /// Creates <see cref="Vehicle"/> instances.
    /// </summary>
    public interface IVehicleFactory
    {
        /// <summary>
        /// Creates a vehicle with a new identifier, applying the fleet age policy.
        /// </summary>
        /// <param name="licensePlate">License plate.</param>
        /// <param name="brand">Brand.</param>
        /// <param name="model">Model.</param>
        /// <param name="manufacturingDate">Manufacturing date.</param>
        /// <param name="now">Current UTC date and time.</param>
        /// <returns>The new vehicle.</returns>
        /// <exception cref="VehicleTooOldException">The vehicle is older than the fleet allows.</exception>
        Vehicle NewVehicle(LicensePlate licensePlate, string brand, string model, ManufacturingDate manufacturingDate, DateTime now);
    }
}
