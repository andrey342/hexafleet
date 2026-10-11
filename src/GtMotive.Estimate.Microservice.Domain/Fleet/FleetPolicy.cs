using System;
using System.Globalization;
using GtMotive.Estimate.Microservice.Domain.Vehicles;

namespace GtMotive.Estimate.Microservice.Domain.Fleet
{
    /// <summary>
    /// Age limit for the vehicles of the fleet.
    /// </summary>
    public static class FleetPolicy
    {
        /// <summary>
        /// Maximum age of a vehicle, counted from its manufacturing date.
        /// </summary>
        public const int MaximumVehicleAgeInYears = 5;

        /// <summary>
        /// Gets the oldest manufacturing date allowed in the fleet.
        /// </summary>
        /// <param name="now">Current UTC date and time.</param>
        /// <returns>The oldest allowed manufacturing date, at midnight UTC.</returns>
        public static DateTime OldestAllowedManufacturingDate(DateTime now)
        {
            return DateTime.SpecifyKind(now.Date.AddYears(-MaximumVehicleAgeInYears), DateTimeKind.Utc);
        }

        /// <summary>
        /// Checks that a vehicle manufactured on the given date can be part of the fleet.
        /// </summary>
        /// <param name="manufacturingDate">Manufacturing date of the vehicle.</param>
        /// <param name="now">Current UTC date and time.</param>
        /// <exception cref="VehicleTooOldException">The vehicle is older than <see cref="MaximumVehicleAgeInYears"/> years.</exception>
        public static void EnsureAllowed(ManufacturingDate manufacturingDate, DateTime now)
        {
            var oldestAllowed = OldestAllowedManufacturingDate(now);

            if (manufacturingDate.ToDateTime() < oldestAllowed)
            {
                throw new VehicleTooOldException(
                    $"Vehicles manufactured before {oldestAllowed.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} are not allowed in the fleet (maximum age {MaximumVehicleAgeInYears} years).");
            }
        }
    }
}
