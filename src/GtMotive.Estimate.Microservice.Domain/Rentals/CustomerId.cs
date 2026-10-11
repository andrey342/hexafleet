using System;
using System.Linq;

namespace GtMotive.Estimate.Microservice.Domain.Rentals
{
    /// <summary>
    /// Customer identity document number. Stored in upper case, without spaces or hyphens.
    /// </summary>
    public readonly struct CustomerId : IEquatable<CustomerId>
    {
        /// <summary>
        /// Minimum number of characters.
        /// </summary>
        public const int MinLength = 5;

        /// <summary>
        /// Maximum number of characters.
        /// </summary>
        public const int MaxLength = 20;

        private readonly string _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerId"/> struct.
        /// </summary>
        /// <param name="value">Identity document number as entered, spaces and hyphens allowed.</param>
        public CustomerId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new DomainException("The customer id is required.");
            }

            var normalized = value
                .Replace(" ", string.Empty, StringComparison.Ordinal)
                .Replace("-", string.Empty, StringComparison.Ordinal)
                .ToUpperInvariant();

            if (normalized.Length < MinLength || normalized.Length > MaxLength || !normalized.All(char.IsAsciiLetterOrDigit))
            {
                throw new DomainException($"The customer id must have between {MinLength} and {MaxLength} letters or digits.");
            }

            _value = normalized;
        }

        /// <summary>
        /// Compares two customer ids for equality.
        /// </summary>
        /// <param name="left">Left customer id.</param>
        /// <param name="right">Right customer id.</param>
        /// <returns>True when both customer ids have the same value.</returns>
        public static bool operator ==(CustomerId left, CustomerId right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two customer ids for inequality.
        /// </summary>
        /// <param name="left">Left customer id.</param>
        /// <param name="right">Right customer id.</param>
        /// <returns>True when the customer ids have different values.</returns>
        public static bool operator !=(CustomerId left, CustomerId right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc />
        public bool Equals(CustomerId other)
        {
            return string.Equals(_value, other._value, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is CustomerId other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _value is null ? 0 : StringComparer.Ordinal.GetHashCode(_value);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return _value;
        }
    }
}
