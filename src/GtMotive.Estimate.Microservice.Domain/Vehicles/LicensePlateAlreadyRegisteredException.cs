using System;
using System.Diagnostics.CodeAnalysis;

namespace GtMotive.Estimate.Microservice.Domain.Vehicles
{
    /// <summary>
    /// Thrown when the license plate already belongs to a vehicle of the fleet.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class LicensePlateAlreadyRegisteredException : DomainException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LicensePlateAlreadyRegisteredException"/> class.
        /// </summary>
        public LicensePlateAlreadyRegisteredException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LicensePlateAlreadyRegisteredException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        public LicensePlateAlreadyRegisteredException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LicensePlateAlreadyRegisteredException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public LicensePlateAlreadyRegisteredException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
