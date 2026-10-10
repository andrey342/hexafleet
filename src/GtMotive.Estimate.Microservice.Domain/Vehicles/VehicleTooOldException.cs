using System;
using System.Diagnostics.CodeAnalysis;

namespace GtMotive.Estimate.Microservice.Domain.Vehicles
{
    /// <summary>
    /// Thrown when a vehicle is older than the fleet allows.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class VehicleTooOldException : DomainException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VehicleTooOldException"/> class.
        /// </summary>
        public VehicleTooOldException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VehicleTooOldException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        public VehicleTooOldException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VehicleTooOldException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public VehicleTooOldException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
