using System;
using GtMotive.Estimate.Microservice.Domain.Fleet;

namespace GtMotive.Estimate.Microservice.Domain.Vehicles
{
    /// <summary>
    /// Vehicle of the renting fleet. Aggregate root.
    /// The constructor restores an existing vehicle and does not check the fleet age policy; use <see cref="Create"/> for new vehicles.
    /// </summary>
    /// <param name="id">Vehicle identifier.</param>
    /// <param name="licensePlate">License plate.</param>
    /// <param name="brand">Brand.</param>
    /// <param name="model">Model.</param>
    /// <param name="manufacturingDate">Manufacturing date.</param>
    public sealed class Vehicle(VehicleId id, LicensePlate licensePlate, string brand, string model, ManufacturingDate manufacturingDate)
    {
        /// <summary>
        /// Maximum length of the brand and the model.
        /// </summary>
        public const int MaxNameLength = 50;

        /// <summary>
        /// Gets the vehicle identifier.
        /// </summary>
        public VehicleId Id { get; } = id;

        /// <summary>
        /// Gets the license plate.
        /// </summary>
        public LicensePlate LicensePlate { get; } = licensePlate;

        /// <summary>
        /// Gets the brand.
        /// </summary>
        public string Brand { get; } = RequiredName(brand, "brand");

        /// <summary>
        /// Gets the model.
        /// </summary>
        public string Model { get; } = RequiredName(model, "model");

        /// <summary>
        /// Gets the manufacturing date.
        /// </summary>
        public ManufacturingDate ManufacturingDate { get; } = manufacturingDate;

        /// <summary>
        /// Creates a vehicle to be added to the fleet.
        /// </summary>
        /// <param name="id">Vehicle identifier.</param>
        /// <param name="licensePlate">License plate.</param>
        /// <param name="brand">Brand.</param>
        /// <param name="model">Model.</param>
        /// <param name="manufacturingDate">Manufacturing date.</param>
        /// <param name="now">Current UTC date and time.</param>
        /// <returns>The new vehicle.</returns>
        /// <exception cref="DomainException">The manufacturing date is in the future.</exception>
        /// <exception cref="VehicleTooOldException">The vehicle is older than the fleet allows.</exception>
        public static Vehicle Create(
            VehicleId id,
            LicensePlate licensePlate,
            string brand,
            string model,
            ManufacturingDate manufacturingDate,
            DateTime now)
        {
            if (manufacturingDate.ToDateTime() > now.Date)
            {
                throw new DomainException("The manufacturing date cannot be in the future.");
            }

            FleetPolicy.EnsureAllowed(manufacturingDate, now);

            return new Vehicle(id, licensePlate, brand, model, manufacturingDate);
        }

        private static string RequiredName(string value, string field)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new DomainException($"The {field} is required.");
            }

            var trimmed = value.Trim();

            return trimmed.Length <= MaxNameLength
                ? trimmed
                : throw new DomainException($"The {field} cannot be longer than {MaxNameLength} characters.");
        }
    }
}
