using System;
using System.Diagnostics.CodeAnalysis;

namespace GtMotive.Estimate.Microservice.Domain.Rentals
{
    /// <summary>
    /// Thrown when the rental has already been returned.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class RentalAlreadyReturnedException : DomainException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RentalAlreadyReturnedException"/> class.
        /// </summary>
        public RentalAlreadyReturnedException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RentalAlreadyReturnedException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        public RentalAlreadyReturnedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RentalAlreadyReturnedException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public RentalAlreadyReturnedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
