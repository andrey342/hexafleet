using System;
using System.Diagnostics.CodeAnalysis;

namespace GtMotive.Estimate.Microservice.Domain.Rentals
{
    /// <summary>
    /// Thrown when the vehicle already has an active rental.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class VehicleNotAvailableException : DomainException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VehicleNotAvailableException"/> class.
        /// </summary>
        public VehicleNotAvailableException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VehicleNotAvailableException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        public VehicleNotAvailableException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VehicleNotAvailableException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public VehicleNotAvailableException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
