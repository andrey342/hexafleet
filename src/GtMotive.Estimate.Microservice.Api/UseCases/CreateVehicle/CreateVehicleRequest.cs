using System;
using System.ComponentModel.DataAnnotations;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using MediatR;

namespace GtMotive.Estimate.Microservice.Api.UseCases.CreateVehicle
{
    public sealed class CreateVehicleRequest : IRequest<IWebApiPresenter>
    {
        /// <summary>
        /// Gets or sets the license plate. Spaces and hyphens are removed.
        /// </summary>
        /// <example>1234 ABC</example>
        [Required]
        [StringLength(20)]
        public string LicensePlate { get; set; }

        /// <summary>
        /// Gets or sets the brand.
        /// </summary>
        /// <example>Seat</example>
        [Required]
        [StringLength(Vehicle.MaxNameLength)]
        public string Brand { get; set; }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <example>Leon</example>
        [Required]
        [StringLength(Vehicle.MaxNameLength)]
        public string Model { get; set; }

        /// <summary>
        /// Gets or sets the manufacturing date. Vehicles older than five years are not accepted.
        /// </summary>
        /// <example>2024-03-15</example>
        [Required]
        public DateOnly? ManufacturingDate { get; set; }
    }
}
