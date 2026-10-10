using System;
using System.Linq;

namespace GtMotive.Estimate.Microservice.Domain.Vehicles
{
    /// <summary>
    /// Vehicle license plate. Stored in upper case, without spaces or hyphens.
    /// </summary>
    public readonly struct LicensePlate : IEquatable<LicensePlate>
    {
        /// <summary>
        /// Minimum number of characters.
        /// </summary>
        public const int MinLength = 4;

        /// <summary>
        /// Maximum number of characters.
        /// </summary>
        public const int MaxLength = 10;

        private readonly string _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="LicensePlate"/> struct.
        /// </summary>
        /// <param name="value">License plate as entered, spaces and hyphens allowed.</param>
        public LicensePlate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new DomainException("The license plate is required.");
            }

            var normalized = value
                .Replace(" ", string.Empty, StringComparison.Ordinal)
                .Replace("-", string.Empty, StringComparison.Ordinal)
                .ToUpperInvariant();

            if (normalized.Length < MinLength || normalized.Length > MaxLength || !normalized.All(char.IsAsciiLetterOrDigit))
            {
                throw new DomainException($"The license plate must have between {MinLength} and {MaxLength} letters or digits.");
            }

            _value = normalized;
        }

        /// <summary>
        /// Compares two license plates for equality.
        /// </summary>
        /// <param name="left">Left license plate.</param>
        /// <param name="right">Right license plate.</param>
        /// <returns>True when both license plates have the same value.</returns>
        public static bool operator ==(LicensePlate left, LicensePlate right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two license plates for inequality.
        /// </summary>
        /// <param name="left">Left license plate.</param>
        /// <param name="right">Right license plate.</param>
        /// <returns>True when the license plates have different values.</returns>
        public static bool operator !=(LicensePlate left, LicensePlate right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc />
        public bool Equals(LicensePlate other)
        {
            return string.Equals(_value, other._value, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is LicensePlate other && Equals(other);
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
