using System;
using System.ComponentModel.DataAnnotations;
using GtMotive.Estimate.Microservice.Domain.Vehicles;

namespace GtMotive.Estimate.Microservice.Api.Models
{
    public sealed class VehicleResponse
    {
        public VehicleResponse(Vehicle vehicle)
        {
            ArgumentNullException.ThrowIfNull(vehicle);

            Id = vehicle.Id.ToGuid();
            LicensePlate = vehicle.LicensePlate.ToString();
            Brand = vehicle.Brand;
            Model = vehicle.Model;
            ManufacturingDate = DateOnly.FromDateTime(vehicle.ManufacturingDate.ToDateTime());
        }

        /// <summary>
        /// Gets the vehicle ID.
        /// </summary>
        [Required]
        public Guid Id { get; }

        /// <summary>
        /// Gets the license plate, in upper case without spaces or hyphens.
        /// </summary>
        [Required]
        public string LicensePlate { get; }

        /// <summary>
        /// Gets the brand.
        /// </summary>
        [Required]
        public string Brand { get; }

        /// <summary>
        /// Gets the model.
        /// </summary>
        [Required]
        public string Model { get; }

        /// <summary>
        /// Gets the manufacturing date.
        /// </summary>
        [Required]
        public DateOnly ManufacturingDate { get; }
    }
}
