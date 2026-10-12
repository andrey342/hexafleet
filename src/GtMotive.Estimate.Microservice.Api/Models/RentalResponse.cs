using System;
using System.ComponentModel.DataAnnotations;
using GtMotive.Estimate.Microservice.Domain.Rentals;

namespace GtMotive.Estimate.Microservice.Api.Models
{
    public sealed class RentalResponse
    {
        public RentalResponse(Rental rental)
        {
            ArgumentNullException.ThrowIfNull(rental);

            Id = rental.Id.ToGuid();
            VehicleId = rental.VehicleId.ToGuid();
            CustomerId = rental.CustomerId.ToString();
            StartedAt = rental.StartedAt;
            ReturnedAt = rental.ReturnedAt;
        }

        /// <summary>
        /// Gets the rental ID.
        /// </summary>
        [Required]
        public Guid Id { get; }

        /// <summary>
        /// Gets the rented vehicle ID.
        /// </summary>
        [Required]
        public Guid VehicleId { get; }

        /// <summary>
        /// Gets the customer identity document, in upper case without spaces or hyphens.
        /// </summary>
        [Required]
        public string CustomerId { get; }

        /// <summary>
        /// Gets the UTC date and time the rental started.
        /// </summary>
        [Required]
        public DateTime StartedAt { get; }

        /// <summary>
        /// Gets the UTC date and time the vehicle was returned, or null while the rental is active.
        /// </summary>
        public DateTime? ReturnedAt { get; }
    }
}
