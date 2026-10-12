using System;
using System.ComponentModel.DataAnnotations;
using MediatR;

namespace GtMotive.Estimate.Microservice.Api.UseCases.RentVehicle
{
    public sealed class RentVehicleRequest : IRequest<IWebApiPresenter>
    {
        /// <summary>
        /// Gets or sets the ID of the vehicle to rent.
        /// </summary>
        [Required]
        public Guid? VehicleId { get; set; }

        /// <summary>
        /// Gets or sets the identity document of the customer. Spaces and hyphens are removed.
        /// </summary>
        /// <example>12345678A</example>
        [Required]
        [StringLength(30)]
        public string CustomerId { get; set; }
    }
}
