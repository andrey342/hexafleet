using System;
using System.Diagnostics.CodeAnalysis;

namespace GtMotive.Estimate.Microservice.Domain
{
    /// <summary>
    /// Thrown when another request writes the same data at the same time. The request can be retried.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class ConcurrencyConflictException : DomainException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrencyConflictException"/> class.
        /// </summary>
        public ConcurrencyConflictException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrencyConflictException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        public ConcurrencyConflictException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrencyConflictException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public ConcurrencyConflictException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
