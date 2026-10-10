using System;

namespace GtMotive.Estimate.Microservice.Domain.Rentals
{
    /// <summary>
    /// Rental identifier.
    /// </summary>
    public readonly struct RentalId : IEquatable<RentalId>
    {
        private readonly Guid _value;

        private RentalId(Guid value)
        {
            if (value == Guid.Empty)
            {
                throw new DomainException("The rental id is required.");
            }

            _value = value;
        }

        /// <summary>
        /// Compares two identifiers for equality.
        /// </summary>
        /// <param name="left">Left identifier.</param>
        /// <param name="right">Right identifier.</param>
        /// <returns>True when both identifiers have the same value.</returns>
        public static bool operator ==(RentalId left, RentalId right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two identifiers for inequality.
        /// </summary>
        /// <param name="left">Left identifier.</param>
        /// <param name="right">Right identifier.</param>
        /// <returns>True when the identifiers have different values.</returns>
        public static bool operator !=(RentalId left, RentalId right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Creates a new identifier.
        /// </summary>
        /// <returns>A new rental identifier.</returns>
        public static RentalId New()
        {
            return new RentalId(Guid.NewGuid());
        }

        /// <summary>
        /// Creates an identifier from an existing value.
        /// </summary>
        /// <param name="value">Identifier value.</param>
        /// <returns>The rental identifier.</returns>
        public static RentalId From(Guid value)
        {
            return new RentalId(value);
        }

        /// <summary>
        /// Gets the identifier value.
        /// </summary>
        /// <returns>The identifier as a <see cref="Guid"/>.</returns>
        public Guid ToGuid()
        {
            return _value;
        }

        /// <inheritdoc />
        public bool Equals(RentalId other)
        {
            return _value.Equals(other._value);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is RentalId other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return _value.ToString();
        }
    }
}
