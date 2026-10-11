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
        [Required]
        [StringLength(20)]
        public string LicensePlate { get; set; }

        /// <summary>
        /// Gets or sets the brand.
        /// </summary>
        [Required]
        [StringLength(Vehicle.MaxNameLength)]
        public string Brand { get; set; }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        [Required]
        [StringLength(Vehicle.MaxNameLength)]
        public string Model { get; set; }

        /// <summary>
        /// Gets or sets the manufacturing date. Vehicles older than five years are not accepted.
        /// </summary>
        [Required]
        public DateOnly? ManufacturingDate { get; set; }
    }
}
